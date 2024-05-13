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
       
        internal static void CreateDBConnection()
        {
            if(Convert.ToBoolean(ConfigurationManager.AppSettings["Local_server"])==true) 
            {
                var dbServer = ConfigurationManager.AppSettings["DB_SERVER"];
                var dbName = ConfigurationManager.AppSettings["DB_NAME"];
                var dbUser = ConfigurationManager.AppSettings["DB_USERNAME"];
                var dbPassword = ConfigurationManager.AppSettings["DB_PASSWORD"];
                DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                ConfigurationManager.AppSettings["DB_TYPE"]);
                Database.Instance.SetConnectionString((DatabaseType)Enum.Parse(typeof(DatabaseType),ConfigurationManager.AppSettings["DB_TYPE"]), "Data Source="+dbServer+";Initial Catalog="+dbName+";User Id="+dbUser+";Password="+dbPassword+ ";TrustServerCertificate=True;");
            }
            else
            {
                ConfigureEnviornmentVaraiables();
            }
        }
        internal static void CreateTable(DataSourceDescription description)
        {
            var db = Database.Instance;
            string query = "CREATE TABLE "+description.table_name;
            query += " (";
            foreach ( var column in description.table_cols)
            {
                query += " " + column.name +" " + column.type + ", ";
            }
            query += ");";
            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);
        }

        internal static void InsertData(string tableName, string columnNames ,string values)
        {
            var db = Database.Instance;
            string query = "INSERT INTO " + tableName + "("+columnNames+")" +"VALUES"+ "(" + values + ")";
            DbCommand cmd = db.CreateCommand(query);
            db.Execute(cmd);
        }

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
