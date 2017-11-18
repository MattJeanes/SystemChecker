using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T> GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Detach(T entity);
        void Delete(T entity);
        void Delete(int id);
    }
}