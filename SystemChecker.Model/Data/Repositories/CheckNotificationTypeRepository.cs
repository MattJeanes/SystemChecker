using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckNotificationTypeRepository : EFRepository<CheckNotificationType>, ICheckNotificationTypeRepository
    {
        public CheckNotificationTypeRepository(DbContext dbContext)
            : base(dbContext) { }

        public override IQueryable<CheckNotificationType> GetAll()
        {
            return base.GetAll()
                .Include(x => x.Options);
        }
    }
}
