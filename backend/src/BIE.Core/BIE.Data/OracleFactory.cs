using System.Data.Common;
using System.Data.OracleClient;

namespace BIE.Data
{
    public class OracleFactory : AbstractDBFactory
    {
        const string NAME = "Oracle";
        public override DbProviderFactory CreateFactory()
        {
            DbProviderFactories.RegisterFactory(NAME, OracleClientFactory.Instance);
            _factoryObject = DbProviderFactories.GetFactory(NAME);

            return _factoryObject;
        }
    }
}
