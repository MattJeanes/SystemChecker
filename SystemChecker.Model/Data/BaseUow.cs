using System;
using SystemChecker.Model.Data.Helpers;
using SystemChecker.Model.Data.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data
{
    public abstract class BaseUow<TDbContext> : IBaseUow where TDbContext : DbContext
    {
        private DbContextOptions _options;

        protected BaseUow(IRepositoryProvider repositoryProvider, DbContextOptions options)
        {
            _options = options;

            CreateDbContext();

            repositoryProvider.DbContext = DbContext;
            RepositoryProvider = repositoryProvider;
        }

        public async virtual Task Commit()
        {
            await DbContext.SaveChangesAsync();
        }

        protected virtual void CreateDbContext()
        {
            DbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), new DbContextOptions[] { _options });
        }

        protected IRepositoryProvider RepositoryProvider { get; set; }

        protected IRepository<T> GetStandardRepo<T>() where T : class
        {
            return RepositoryProvider.GetRepositoryForEntityType<T>();
        }
        protected T GetRepo<T>() where T : class
        {
            return RepositoryProvider.GetRepository<T>();
        }

        protected TDbContext DbContext { get; set; }

        public DbContext DatabaseContext { get => DbContext; }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbContext != null)
                {
                    DbContext.Dispose();
                }
            }
        }

        #endregion
    }
}
