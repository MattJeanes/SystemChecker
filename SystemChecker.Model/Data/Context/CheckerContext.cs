using Microsoft.EntityFrameworkCore;
using SystemChecker.Model.Data.Entities;

namespace SystemChecker.Model.Data
{
    public class CheckerContext : DbContext, ICheckerContext
    {
        public CheckerContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubCheckTypeOption>()
                .HasKey(x => new { x.ID, x.SubCheckTypeID });

            modelBuilder.Entity<CheckTypeOption>()
                .HasKey(x => new { x.ID, x.CheckTypeID });

            modelBuilder.Entity<CheckNotificationTypeOption>()
                .HasKey(x => new { x.ID, x.CheckNotificationTypeID });
        }

        public DbSet<Check> Checks { get; set; }
        public DbSet<CheckType> CheckTypes { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<ConnString> ConnStrings { get; set; }
        public DbSet<CheckData> CheckData { get; set; }
        public DbSet<CheckResult> CheckResults { get; set; }
        public DbSet<SubCheckType> SubCheckTypes { get; set; }
        public DbSet<CheckNotificationType> CheckNotificationTypes { get; set; }
        public DbSet<CheckNotification> CheckNotifications { get; set; }
        public DbSet<Environment> Environments { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactType> ContactTypes { get; set; }
        public DbSet<CheckGroup> CheckGroups { get; set; }
        public DbSet<GlobalSetting> GlobalSettings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ResultStatus> ResultStatuses { get; set; }
        public DbSet<ResultType> ResultTypes { get; set; }
    }
}