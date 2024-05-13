using System.Data.Common;
using MySql.Data.MySqlClient;

namespace BIE.Data
{
    public class MySqlFactory : AbstractDBFactory
    {
        const string NAME = "MYSQL";

        public override DbProviderFactory CreateFactory()
        {
            DbProviderFactories.RegisterFactory(NAME, MySqlClientFactory.Instance);
            _factoryObject = DbProviderFactories.GetFactory(NAME);

            return _factoryObject;
        }
    }
}
