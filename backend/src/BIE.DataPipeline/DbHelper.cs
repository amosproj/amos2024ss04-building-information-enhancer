using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using BIE.Data;
using BIE.DataPipeline.Import;

namespace BIE.DataPipeline
{
    internal sealed class DbHelper
    {
        private string mInputQueryString;
        private readonly StringBuilder mStringBuilder;

        private int mCount;
        private const int MaxCount = 900;


        public DbHelper()
        {
            ConfigureEnvironmentVariables();
            mStringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Check if a connection to the database is possible.
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {
            var db = Database.Instance;
            return db.CheckConnection();
        }

        public static bool CanSkip(DataSourceDescription description)
        {
            var db = Database.Instance;
            // Handle Table Behavior
            if (description.options.if_table_exists == InsertBehaviour.skip && TableExists(description.table_name, db))
            {
                Console.WriteLine($"Table {description.table_name} already exists, stopping...");
                return true;
            }

            if (description.options.if_table_exists == InsertBehaviour.replace)
            {
                Console.WriteLine($"Dropping table {description.table_name} if it exists.");
                db.ExecuteScalar(db.CreateCommand($"DROP TABLE IF EXISTS {description.table_name}"));
            }

            return false;
        }

        /// <summary>
        /// Checks if a table exists in the database
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static bool TableExists(string tableName, IDatabase db)
        {
            var exists = (int)db.ExecuteScalar(db.CreateCommand($"SELECT count(*) as Exist" +
                                                                $" from INFORMATION_SCHEMA.TABLES" +
                                                                $" where table_name = '{tableName}'"));
            return exists == 1;
        }

        /// <summary>
        /// Set the main INSERT INTO QueryString.
        /// Sets the table name and the necessary column names.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        public void SetInfo(string tableName, string columnNames)
        {
            mInputQueryString = $"INSERT INTO {tableName} ( {columnNames} )  VALUES ";
            mStringBuilder.Clear();
            mStringBuilder.Append(mInputQueryString);
        }

        /// <summary>
        /// Creates a Table in the database based on the name given in the description. Returns false if unable to.
        /// </summary>
        /// <param name="description"></param>
        internal bool CreateTable(DataSourceDescription description)
        {
            try
            {
                Console.WriteLine("Creating Table...");
                var db = Database.Instance;


                var query = GetCreationQuery(description);

                var cmd = db.CreateCommand(query);
                db.Execute(cmd);

                Console.WriteLine("Table created.");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while creating Table:");
                Console.Error.WriteLine(e);
                return false;
            }
        }
        /// <summary>
        /// Check if location column exists
        /// </summary>
        /// <param name="description"></param>
        public bool CheckIfColumnExists(DataSourceDescription description)
        {
            string query = @"
            SELECT t.name AS table_name, c.name AS column_name, ty.name AS data_type
            FROM sys.columns c
            JOIN sys.tables t ON c.object_id = t.object_id
            JOIN sys.types ty ON c.user_type_id = ty.user_type_id
            WHERE t.name = '"+description.table_name+"' AND c.name = 'Location' AND ty.name = 'geometry';";
            var db = Database.Instance;

            using (var command = db.CreateCommand(query))
            {
                var (reader, connection) = db.ExecuteReader(command);

                try
                {
                    return reader.HasRows;
                }
                finally
                {
                    reader.Close();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Create indexes for shape dataset on location column
        /// </summary>
        /// <param name="description"></param>
        internal bool CreateIndexes(DataSourceDescription description)
        {
            try
            {
                Console.WriteLine("Creating Index...");
                var db = Database.Instance;

                // Step 1: Check if the ID column exists, and add it if it doesn't
                string addColumnQuery = @"
    USE BIEDB;
    IF NOT EXISTS (
        SELECT * 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = '" + description.table_name + @"' 
        AND COLUMN_NAME = 'ID'
    )
    BEGIN
        ALTER TABLE dbo." + description.table_name + @" 
        ADD ID INT IDENTITY(1,1);
    END;
";

                var addColumnCmd = db.CreateCommand(addColumnQuery);
                db.Execute(addColumnCmd);

                // Step 2: Ensure the ID column is the primary key
                string primaryKeyQuery = @"
    USE BIEDB;
    IF NOT EXISTS (
        SELECT * 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE TABLE_NAME = '" + description.table_name + @"' 
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
    )
    BEGIN
        ALTER TABLE dbo." + description.table_name + @" 
        ADD CONSTRAINT PK_" + description.table_name + @"_ID PRIMARY KEY CLUSTERED (ID);
    END;
";

                var pkCmd = db.CreateCommand(primaryKeyQuery);
                db.Execute(pkCmd);

                // Step 3: Calculate the bounding box
                string bboxQuery = @"
    USE BIEDB;
    DECLARE @MinX FLOAT, @MinY FLOAT, @MaxX FLOAT, @MaxY FLOAT;

    WITH ConvertedGeography AS (
        SELECT Location.STAsText() AS WKT
        FROM dbo." + description.table_name + @"
    )
    SELECT
        @MinX = geometry::EnvelopeAggregate(geometry::STGeomFromText(WKT, 4326)).STPointN(1).STX,
        @MinY = geometry::EnvelopeAggregate(geometry::STGeomFromText(WKT, 4326)).STPointN(1).STY,
        @MaxX = geometry::EnvelopeAggregate(geometry::STGeomFromText(WKT, 4326)).STPointN(3).STX,
        @MaxY = geometry::EnvelopeAggregate(geometry::STGeomFromText(WKT, 4326)).STPointN(3).STY
    FROM ConvertedGeography;
    
    SELECT @MinX AS MinX, @MinY AS MinY, @MaxX AS MaxX, @MaxY AS MaxY;
";

                var bboxCmd = db.CreateCommand(bboxQuery);
                var (bboxReader, bboxConnection) = db.ExecuteReader(bboxCmd);

                double minX = 0, minY = 0, maxX = 0, maxY = 0;

                if (bboxReader.Read())
                {
                    minX = (double)bboxReader["MinX"];
                    minY = (double)bboxReader["MinY"];
                    maxX = (double)bboxReader["MaxX"];
                    maxY = (double)bboxReader["MaxY"];
                }

                var cultureInfo = new CultureInfo("en-US");
                Console.WriteLine($"BBox: MinX = {minX.ToString(cultureInfo)}, MinY = {minY.ToString(cultureInfo)}, MaxX = {maxX.ToString(cultureInfo)}, MaxY = {maxY.ToString(cultureInfo)}");

                bboxReader.Close();
                bboxConnection.Close();

                // Step 4: Create the spatial index
                string indexQuery = @"
    USE BIEDB;
    SET QUOTED_IDENTIFIER ON;
    IF NOT EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'SI_" + description.table_name + @"_Location'
        AND object_id = OBJECT_ID('dbo." + description.table_name + @"')
    )
    BEGIN
        CREATE SPATIAL INDEX SI_" + description.table_name + @"_Location
        ON dbo." + description.table_name + @"(Location)
        USING GEOMETRY_AUTO_GRID
        WITH (
            BOUNDING_BOX = (
                XMIN = " + minX.ToString(cultureInfo) + @",
                YMIN = " + minY.ToString(cultureInfo) + @",
                XMAX = " + maxX.ToString(cultureInfo) + @",
                YMAX = " + maxY.ToString(cultureInfo) + @"
            )
        );
    END;
    UPDATE STATISTICS dbo." + description.table_name + @";
";

                var cmd = db.CreateCommand(indexQuery);
                db.Execute(cmd);

                Console.WriteLine("Index created.");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while creating Table:");
                Console.Error.WriteLine(e);
                return false;
            }
        }


        private string GetCreationQuery(DataSourceDescription? description)
        {
            if (description.source.data_format == "SHAPE")
            {
                return $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{description.table_name}')
BEGIN
    CREATE TABLE {description.table_name} (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Location GEOMETRY
    );
END";
            }

            if (description.source.data_format == "CITYGML")
            {
                return $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{description.table_name}')
BEGIN
    CREATE TABLE {description.table_name} (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Location GEOGRAPHY,
        XmlData XML,
        GroundHeight FLOAT,
        DistrictKey VARCHAR(255),
        CheckDate DATE,
        GroundArea FLOAT,
        BuildingWallHeight FLOAT,
        LivingArea FLOAT,
        RoofArea FLOAT,
    );
END";
            }

                var query = $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{description.table_name}')
BEGIN CREATE TABLE {description.table_name} (";

            foreach (var column in description.table_cols)
            {
                query += $" {column.name_in_table} {column.type}, ";
            }

            if(description.options.location_to_SQL_point != null)
            {
                query += $" {description.options.location_to_SQL_point.name_in_table} GEOMETRY,";
            }

            query += "); END";

            return query;
        }


        /// <summary>
        /// Insert Data into the Table. Inserts will be bundled.
        /// </summary>
        /// <param name="values"></param>
        internal void InsertData(string values)
        {
            if (mCount > 0)
            {
                mStringBuilder.Append(',');
            }

            mStringBuilder.Append($"({values})");

            mCount++;
            if (mCount >= MaxCount)
            {
                ExecuteInsert();
            }
        }

        /// <summary>
        /// Execute the Actual Insert Statement.
        /// </summary>
        private void ExecuteInsert()
        {
            mStringBuilder.Append(';');
            var query = mStringBuilder.ToString();

            try
            {
                var db = Database.Instance;
                var cmd = db.CreateCommand(query);
                db.Execute(cmd);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed with:\n" + query);
                throw;
            }

            mCount = 0;
            mStringBuilder.Clear();
            mStringBuilder.Append(mInputQueryString);
        }

        /// <summary>
        /// Get Database Settings from Environment Variables.
        /// </summary>
        private void ConfigureEnvironmentVariables()
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbTrusted = Environment.GetEnvironmentVariable("TRUSTED")?.ToLower() == "true";
            var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                                                  Environment.GetEnvironmentVariable("DB_TYPE") ??
                                                  throw new Exception("Could not get EnvironmentVariable DB_TYPE"));

            
            if (dbServer == null || dbName == null || dbUser == null || dbPassword == null)
            {
                throw new ExternalException("Could not get Environment Variables.");
            }

            Database.Instance.SetConnectionString(dbType,
                                                  dbServer,
                                                  dbName,
                                                  dbUser,
                                                  dbPassword,
                                                  dbTrusted);
        }
    }
}