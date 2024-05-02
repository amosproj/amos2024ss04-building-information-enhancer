using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;

namespace BIE.Core.DBRepository
{
    public class DBRepositoryFactroy : IRepositoryFactory
    {

        IInfrastructureRepository _batchRepository;

        public IInfrastructureRepository BatchRepository
        {
            get
            {
                if (_batchRepository == null)
                {
                    _batchRepository = new InfrastructureRepository();
                }

                return _batchRepository;
            }
        }


    }
}
