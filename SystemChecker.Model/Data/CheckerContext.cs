using SystemChecker.Model.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data
{
    public class CheckerContext : DbContext
    {
        public CheckerContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Check> Checks { get; set; }
        public DbSet<CheckType> CheckTypes { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<ConnString> ConnStrings { get; set; }
    }
}