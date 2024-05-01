using System;
using System.Data.Common;
using System.Data.OracleClient;

using MySql.Data.MySqlClient;

namespace BIE.Data
{
    public abstract class AbstractDBFactory
    {
        protected string _factoryName;

        protected DbProviderFactory _factoryObject;

        public abstract DbProviderFactory CreateFactory();
        public virtual DbProviderFactory Factory
        {
            get { return _factoryObject; }
        }

    }
}

