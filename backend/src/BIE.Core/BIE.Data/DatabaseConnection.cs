using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BIE.Data
{
    public interface IDatabase
    {
        void SetConnectionString(DatabaseType databaseType, string connectionString);

        void SetConnectionString(DatabaseType dbType,
            string dbServer,
            string dbName,
            string userId,
            string password,
            bool trustedConnection);

        DbProviderFactory Factory { get; }
        DbCommand CreateSPCommand(string spName, List<DbParameter> parameters = null, DbTransaction trans = null);
        DbCommand CreateCommand(string query, DbTransaction trans = null);
        void AddParameter(DbCommand cmd, string paramName, object value);
        DataSet LoadData(DbCommand cmd);
        object ExecuteScalar(DbCommand command);
        void Execute(DbCommand command);

        (DbDataReader reader, DbConnection connection) ExecuteReader(DbCommand command);
    }

    public class Database : IDatabase
    {
        private static Database _instance;
        private DatabaseFactory _factoryCreator;
        private string _connectionString;
        private DatabaseType _dbType;

        private Database()
        {
        }

        public static IDatabase Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new Database();
                }

                return _instance;
            }
        }

        public void SetConnectionString(DatabaseType dbType,
            string dbServer,
            string dbName,
            string userId,
            string password,
            bool trustedConnection)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = dbServer;
            builder.InitialCatalog = dbName;
            if (trustedConnection)
                builder.IntegratedSecurity = true;
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = userId;
                builder.Password = password;
            }

            this.SetConnectionString(dbType, builder.ConnectionString);
        }

        public void SetConnectionString(DatabaseType dbType, string connectionString)
        {
            _dbType = dbType;
            _connectionString = connectionString;

            _factoryCreator = new DatabaseFactory();
            Factory = _factoryCreator.GetFactory(dbType);
        }

        private DbConnection CreateConnection()
        {
            DbConnection con = Factory.CreateConnection();
            con.ConnectionString = _connectionString;
            return con;
        }

        public DbProviderFactory Factory { get; private set; }

        public DbCommand CreateSPCommand(string spName, List<DbParameter> parameters = null, DbTransaction trans = null)
        {
            DbCommand cmd = Factory.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (trans != null)
                cmd.Transaction = trans;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                    cmd.Parameters.Add(p);
            }

            return cmd;
        }

        public DbCommand CreateCommand(string query, DbTransaction trans = null)
        {
            DbCommand cmd = Factory.CreateCommand();
            cmd.CommandText = query;

            if (trans != null)
                cmd.Transaction = trans;

            return cmd;
        }

        public void AddParameter(DbCommand cmd, string paramName, object value)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = paramName;

            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = value;

            cmd.Parameters.Add(parameter);
        }

        public object ExecuteScalar(DbCommand command)
        {
            object r;
            using (DbConnection con = this.CreateConnection())
            {
                con.Open();
                command.Connection = con;
                r = command.ExecuteScalar();
                con.Close();
            }

            return r;
        }

        public ( DbDataReader reader, DbConnection connection) ExecuteReader(DbCommand command)
        {
            DbDataReader r;
            var con = CreateConnection();
            con.Open();
            command.Connection = con;
            r = command.ExecuteReader();

            return ( r, con );
        }

        public void Execute(DbCommand command)
        {
            using (DbConnection con = this.CreateConnection())
            {
                con.Open();
                command.Connection = con;
                command.ExecuteNonQuery();
                con.Close();
            }
        }

        public DataSet LoadData(DbCommand command)
        {
            using (DbConnection con = this.CreateConnection())
            {
                command.Connection = con;
                DbDataAdapter adapter = Factory.CreateDataAdapter();
                adapter.SelectCommand = command;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                con.Close();

                return ds;
            }
        }
    }
}