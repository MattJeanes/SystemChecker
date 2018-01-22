using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public Repository(ICheckerContext ctx)
        {
            _ctx = ctx;
            _db = _ctx.Set<T>();
        }
        private readonly DbSet<T> _db;
        private readonly ICheckerContext _ctx;

        public async Task<T> Find(object key)
        {
            return await _db.FindAsync(key);
        }

        public virtual IQueryable<T> GetAll()
        {
            return _db.AsQueryable();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.FirstOrDefaultAsync(predicate);
        }

        public void AddRange(IEnumerable<T> content)
        {
            _db.AddRange(content);
        }

        public T Add(T content)
        {
            _db.Add(content);
            return content;
        }

        public void Delete(T request)
        {
            _db.Remove(request);
        }

        public void DeleteRange(IEnumerable<T> req)
        {
            _db.RemoveRange(req);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _ctx.SaveChangesAsync();
        }

        public IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            return source.Include(navigationPropertyPath);
        }


        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _ctx?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
