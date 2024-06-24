using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.Json;
using BIE.Data;

namespace BIE.Core.DBRepository
{
    public static class DbHelper
    {
        public static T Convert<T>(string text)
        {
            T result;
            try
            {
                if (text.Length > 0)
                {
                    result = JsonSerializer.Deserialize<T>(text);
                    return result;
                }
            }
            catch (JsonException je)
            {

            }

            return default(T);
        }
        
        /// <summary>
        /// Get database info from environment variables and make connection.
        /// </summary>
        public static void CreateDbConnection()
        {
            // Configure Environment Variables.
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbTrusted = Environment.GetEnvironmentVariable("TRUSTED")?.ToLower() == "true";
            var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),
                                                  Environment.GetEnvironmentVariable("DB_TYPE") ?? string.Empty);

            Database.Instance.SetConnectionString(dbType,
                                                  dbServer,
                                                  dbName,
                                                  dbUser,
                                                  dbPassword,
                                                  dbTrusted);
        }

        public static IEnumerable<Dictionary<string, string>> GetData(string sqlQuery)
        {
            var command = Database.Instance.CreateCommand(sqlQuery);
            var (reader, connection) = Database.Instance.ExecuteReader(command);
            

            if (!reader.HasRows)
            {
                Console.WriteLine("Reader has found no rows...");
                connection.Close();
                yield break;
            }
            // get the column names
            var columnNames = new string[reader.FieldCount];
            
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
            }
            
            // read the results and return lazy
            while (reader.Read())
            {
                var dict = new Dictionary<string, string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict[columnNames[i]] = reader[i].ToString();
                }
                yield return dict;
            }
            
            connection.Close();
        }
        public static IEnumerable<Dictionary<string, string>> GetData(string sqlQuery, int commandTimeout)
        {
            var command = Database.Instance.CreateCommand(sqlQuery,commandTimeout);
            var (reader, connection) = Database.Instance.ExecuteReader(command);

            if (!reader.HasRows)
            {
                Console.WriteLine("Reader has found no rows...");
                connection.Close();
                yield break;
            }

            // get the column names
            var columnNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
            }

            // read the results and return lazy
            while (reader.Read())
            {
                var dict = new Dictionary<string, string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict[columnNames[i]] = reader[i].ToString();
                }
                yield return dict;
            }

            connection.Close();
        }


    }
}

