using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;

namespace BIE.Core.DBRepository
{
    public class DBRepositoryFactory : IRepositoryFactory
    {

        IInfrastructureRepository _infrastructureRepository;

        public IInfrastructureRepository InfrastructureRepository
        {
            get
            {
                if (_infrastructureRepository == null)
                {
                    _infrastructureRepository = new InfrastructureRepository();
                }

                return _infrastructureRepository;
            }
        }


    }
}
