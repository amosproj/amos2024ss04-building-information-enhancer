using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIE.Data;
using System.Configuration;
using System.Data;
using System.Xml.Linq;
using BIE.DataPipeline.Import;
using System.Data.Common;

namespace BIE.DataPipeline
{
    internal class DBHelper
    {
        /// <summary>
        /// To create connection with DB
        /// </summary>
        internal static void CreateDBConnection()
        {
           ConfigureEnviornmentVaraiables();
        }
        /// <summary>
        /// To create table in DB
        /// </summary>
        /// <param name="description"></param>
        internal static void CreateTable(DataSourceDescription description)
        {
            var db = Database.Instance;
            string query = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EV_charging_stations')\r\nBEGIN CREATE TABLE " + description.table_name;
            query += " (";
            foreach ( var column in description.table_cols)
            {
                query += " " + column.name_in_table +" " + column.type + ", ";
            }
            query += "); END";
            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);
        }
        /// <summary>
        /// To insert data into DB
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="values"></param>
        internal static void InsertData(string tableName, string columnNames ,string values)
        {
            var db = Database.Instance;
            string query = "INSERT INTO " + tableName + " ( "+columnNames+" ) " +" VALUES "+ " ( " + values + " );";
            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);
        }
        /// <summary>
        /// To set enviornment variables
        /// </summary>
        private static void ConfigureEnviornmentVaraiables()
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            bool dbTrusted = Environment.GetEnvironmentVariable("TRUSTED").ToLower() == "true";
            DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                Environment.GetEnvironmentVariable("DB_TYPE"));

            Database.Instance.SetConnectionString(dbType, dbServer,
                dbName, dbUser, dbPassword, dbTrusted);
        }
    }
}
