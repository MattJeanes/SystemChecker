using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckNotificationTypeRepository : Repository<CheckNotificationType>, ICheckNotificationTypeRepository
    {
        public CheckNotificationTypeRepository(ICheckerContext context)
            : base(context) { }

        public override IQueryable<CheckNotificationType> GetAll()
        {
            return base.GetAll()
                .Include(x => x.Options);
        }
    }
}
