using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class
    {
        Task<T> Find(object key);
        IQueryable<T> GetAll();
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        void AddRange(IEnumerable<T> content);
        T Add(T content);
        void DeleteRange(IEnumerable<T> req);
        void Delete(T request);
        Task<int> SaveChangesAsync();

        IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class;
    }
}