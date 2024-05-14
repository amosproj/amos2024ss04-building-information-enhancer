using System;
using System.Data.Common;

namespace BIE.Data
{
    public enum DatabaseType
    {
        SQL,
        MySql,
        Oracle
    }

    public class DatabaseFactory
    {
        public DbProviderFactory GetFactory(DatabaseType type)
        {
            AbstractDBFactory con;
            switch (type)
            {
                case DatabaseType.SQL:
                    con = new SqlFactory();
                    break;
                case DatabaseType.MySql:
                    con = new MySqlFactory();
                    break;
                case DatabaseType.Oracle:
                    con = new OracleFactory();
                    break;
                default:
                    throw new Exception("This database type is not implemented.");
            }

            return con.CreateFactory();
        }
    }
}
