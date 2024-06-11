using System.Runtime.InteropServices;
using System.Text;
using BIE.Data;
using BIE.DataPipeline.Import;

namespace BIE.DataPipeline
{
    internal class DBHelper
    {
        private string mInputQueryString;
        private readonly StringBuilder mStringBuilder;

        private int mCount;
        private const int MaxCount = 900;

        public DBHelper()
        {
            ConfigureEnvironmentVariables();
            mStringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Set the main INSERT INTO QueryString.
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

                // Handle Table Behavior
                if (description.options.if_table_exists == InsertBehaviour.skip)
                {
                    var tableExists =
                        (int)db.ExecuteScalar(db.CreateCommand($"SELECT count(*) as Exist" +
                                                               $" from INFORMATION_SCHEMA.TABLES" +
                                                               $" where table_name = '{description.table_name}'"));

                    if (tableExists == 1)
                    {
                        Console.WriteLine($"Table {description.table_name} already exists, stopping...");
                        return false;
                    }
                }

                if (description.options.if_table_exists == InsertBehaviour.replace)
                {
                    Console.WriteLine($"Dropping table {description.table_name} if it exists.");
                    db.ExecuteScalar(db.CreateCommand($"DROP TABLE IF EXISTS {description.table_name}"));
                }

                var query = GetCreationQuery(description);

                var cmd = db.CreateCommand(query);
                db.Execute(cmd);

                Console.WriteLine("Table created.");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while creating Table:");
                Console.WriteLine(e);
                return false;
            }
        }

        private string GetCreationQuery(DataSourceDescription description)
        {
            if (description.source.data_format == "SHAPE")
            {
                return $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{description.table_name}')
BEGIN
    CREATE TABLE {description.table_name} (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Location GEOGRAPHY
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
        /// To set enviornment variables
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