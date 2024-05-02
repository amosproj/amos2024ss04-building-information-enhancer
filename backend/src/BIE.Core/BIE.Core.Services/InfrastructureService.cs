using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Core.Services;
using System.Collections.Generic;

namespace Parbat.Core.Services
{
    public class InfrastructureService : IService
    {
        private readonly IRepositoryFactory _factory;

        public InfrastructureService(IRepositoryFactory factory)
        {
            _factory = factory;
        }

        public void Create(Infrastructure service)
        {
            _factory.BatchRepository.Add(service);
        }

        public void Delete(long id)
        {
            var found = _factory.BatchRepository.GetById(id);

            if (found == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            _factory.BatchRepository.Delete(found);
        }

        public Infrastructure FindByID(long id)
        {
            var found = _factory.BatchRepository.GetById(id);

            return found;
        }

        public IEnumerable<Infrastructure> GetAll()
        {
            return _factory.BatchRepository.GetAll();
        }

        public void Update(Infrastructure service)
        {
            if (service.InfrastructureID == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            var found = _factory.BatchRepository.GetById(service.InfrastructureID.Value);

            if (found == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            _factory.BatchRepository.Update(service);
        }
    }
}
