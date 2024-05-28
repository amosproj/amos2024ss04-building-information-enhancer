using System.Text;
using BIE.Data;
using BIE.DataPipeline.Import;
using System.Data.Common;

namespace BIE.DataPipeline
{
    internal class DBHelper
    {
        private string mInputQueryString;
        private StringBuilder mStringBuilder;

        private int mCount;
        private int maxCount = 1;

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

            mStringBuilder.Append(mInputQueryString);
        }

        /// <summary>
        /// To create table in DB
        /// </summary>
        /// <param name="description"></param>
        internal void CreateTable(DataSourceDescription description)
        {
            Console.WriteLine("Creating Table...");

            var db = Database.Instance;
            //string query = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" +
            //               description.table_name + "')\r\nBEGIN CREATE TABLE " + description.table_name;
            string query = "CREATE TABLE SpatialData (    Id INT PRIMARY KEY IDENTITY(1,1),    Location GEOGRAPHY );";
            //query += " (";
            //foreach (var column in description.table_cols)
            //{
            //    query += " " + column.name_in_table + " " + column.type + ", ";
            //}

            //query += "); END";
            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);

            Console.WriteLine("Table created.");
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