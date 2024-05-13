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
            _factory.InfrastructureRepository.Add(service);
        }

        public void Delete(long id)
        {
            var found = _factory.InfrastructureRepository.GetById(id);

            if (found == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            _factory.InfrastructureRepository.Delete(found);
        }

        public Infrastructure FindByID(long id)
        {
            var found = _factory.InfrastructureRepository.GetById(id);

            return found;
        }

        public IEnumerable<Infrastructure> GetAll()
        {
            return _factory.InfrastructureRepository.GetAll();
        }

        public void Update(Infrastructure service)
        {
            if (service.InfrastructureID == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            var found = _factory.InfrastructureRepository.GetById(service.InfrastructureID.Value);

            if (found == null)
            {
                throw new ServiceException(Errors.NotExistsError);
            }

            _factory.InfrastructureRepository.Update(service);
        }
    }
}
