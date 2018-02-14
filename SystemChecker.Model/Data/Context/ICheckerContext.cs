using SystemChecker.Model.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace SystemChecker.Model.Data
{
    public interface ICheckerContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        DatabaseFacade Database { get; }
        EntityEntry<T> Entry<T>(T entry) where T : class;
        EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry Update(object entity);
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;

        DbSet<Check> Checks { get; set; }
        DbSet<CheckType> CheckTypes { get; set; }
        DbSet<Login> Logins { get; set; }
        DbSet<ConnString> ConnStrings { get; set; }
        DbSet<CheckData> CheckData { get; set; }
        DbSet<CheckResult> CheckResults { get; set; }
        DbSet<SubCheckType> SubCheckTypes { get; set; }
        DbSet<CheckNotificationType> CheckNotificationTypes { get; set; }
        DbSet<CheckNotification> CheckNotifications { get; set; }
        DbSet<Entities.Environment> Environments { get; set; }
        DbSet<Contact> Contacts { get; set; }
        DbSet<ContactType> ContactTypes { get; set; }
        DbSet<CheckGroup> CheckGroups { get; set; }
        DbSet<GlobalSetting> GlobalSettings { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<ApiKey> ApiKeys { get; set; }
    }
}