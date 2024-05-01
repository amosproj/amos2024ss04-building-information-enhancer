using System.Collections.Generic;

namespace BIE.Core.BaseRepository
{
    public interface IGenericRepository<T> where T : class
    {
        T GetById(long id);

        IEnumerable<T> GetAll();

        void Add(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
