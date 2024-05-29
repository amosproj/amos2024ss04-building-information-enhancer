using System.Text;
using BIE.Data;
using BIE.DataPipeline.Import;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace BIE.DataPipeline
{
    internal class DBHelper
    {
        private string mInputQueryString;
        private StringBuilder mStringBuilder;

        private int mCount;
        private int maxCount = 900;

        public DBHelper()
        {
            ConfigureEnviornmentVaraiables();
            mStringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Set the main INSERT INTO QueryString.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        public void SetInfo(string tableName, string columnNames)
        {
            mInputQueryString = "INSERT INTO " + tableName + " ( " + columnNames + " ) " + " VALUES ";
            mStringBuilder.Clear();
            mStringBuilder.Append(mInputQueryString);
        }

        /// <summary>
        /// To create table in DB
        /// </summary>
        /// <param name="description"></param>
        /// <param name="forshape"></param>
        internal bool CreateTable(DataSourceDescription description, bool forshape = false)
        {
            Console.WriteLine("Creating Table...");

            var db = Database.Instance;

            if (description.options.if_table_exists == "skip")
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

            if (description.options.if_table_exists == "replace")
            {
                Console.WriteLine($"Dropping existing table {description.table_name}");
                db.ExecuteScalar(db.CreateCommand($"DROP TABLE IF EXISTS {description.table_name}"));
            }
            
            string query;
            if (!forshape)
            {
                query = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" +
                        description.table_name + "')\r\nBEGIN CREATE TABLE " + description.table_name;
                query += " (";
                foreach (var column in description.table_cols)
                {
                    query += " " + column.name_in_table + " " + column.type + ", ";
                }

                query += "); END";
            }
            else
            {
                query = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SpatialData')
            BEGIN
                CREATE TABLE SpatialData (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Location GEOGRAPHY
                );
            END";
            }

            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);

            Console.WriteLine("Table created.");

            return true;
        }


        /// <summary>
        /// Insert Data into the Table. Inserts will be bundled.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="values"></param>
        internal void InsertData(string values)
        {
            // var query = mInputQueryString + $"({values});";

            if (mCount > 0)
            {
                mStringBuilder.Append(',');
            }

            mStringBuilder.Append($"({values})");

            mCount++;
            if (mCount >= maxCount)
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
                Console.WriteLine("Failed at:\n" + query);
                throw;
            }

            mCount = 0;
            mStringBuilder.Clear();
            mStringBuilder.Append(mInputQueryString);
        }

        /// <summary>
        /// To set enviornment variables
        /// </summary>
        private void ConfigureEnviornmentVaraiables()
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            bool dbTrusted = Environment.GetEnvironmentVariable("TRUSTED").ToLower() == "true";
            DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                                                           Environment.GetEnvironmentVariable("DB_TYPE"));

            Database.Instance.SetConnectionString(dbType,
                                                  dbServer,
                                                  dbName,
                                                  dbUser,
                                                  dbPassword,
                                                  dbTrusted);
        }
    }
}