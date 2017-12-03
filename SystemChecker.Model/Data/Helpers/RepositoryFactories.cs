using SystemChecker.Model.Data.Interfaces;
using System;
using System.Collections.Generic;
using SystemChecker.Model.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Helpers
{
    public class RepositoryFactories
    {
        private IDictionary<Type, Func<DbContext, object>> GetFactories()
        {
            return new Dictionary<Type, Func<DbContext, object>>
            {
                { typeof(ICheckRepository), dbContext => new CheckRepository(dbContext) },
                { typeof(ICheckTypeRepository), dbContext => new CheckTypeRepository(dbContext) },
                { typeof(ISubCheckTypeRepository), dbContext => new SubCheckTypeRepository(dbContext) },
                { typeof(ICheckNotificationTypeRepository), dbContext => new CheckNotificationTypeRepository(dbContext) },
            };
        }

        public RepositoryFactories()
        {
            _repositoryFactories = GetFactories();
        }

        public RepositoryFactories(IDictionary<Type, Func<DbContext, object>> factories)
        {
            _repositoryFactories = factories;
        }

        public Func<DbContext, object> GetRepositoryFactory<T>()
        {
            _repositoryFactories.TryGetValue(typeof(T), out Func<DbContext, object> factory);
            return factory;
        }

        public Func<DbContext, object> GetRepositoryFactoryForEntityType<T>() where T : class
        {
            return GetRepositoryFactory<T>() ?? DefaultEntityRepositoryFactory<T>();
        }

        protected virtual Func<DbContext, object> DefaultEntityRepositoryFactory<T>() where T : class
        {
            return dbContext => new EFRepository<T>(dbContext);
        }

        private readonly IDictionary<Type, Func<DbContext, object>> _repositoryFactories;
    }
}
