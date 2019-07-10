using Microsoft.EntityFrameworkCore;
using System.Linq;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;

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
