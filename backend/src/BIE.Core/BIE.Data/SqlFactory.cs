using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace BIE.Data
{
    public class SqlFactory : AbstractDBFactory
    {
        const string NAME = "SQL";
        public override DbProviderFactory CreateFactory()
        {
            DbProviderFactories.RegisterFactory(NAME, SqlClientFactory.Instance);
            _factoryObject = DbProviderFactories.GetFactory(NAME);

            return _factoryObject;
        }
    }
}
