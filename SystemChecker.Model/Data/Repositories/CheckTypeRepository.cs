using Microsoft.EntityFrameworkCore;
using System.Linq;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckTypeRepository : Repository<CheckType>, ICheckTypeRepository
    {
        public CheckTypeRepository(ICheckerContext context)
            : base(context) { }

        public override IQueryable<CheckType> GetAll()
        {
            return base.GetAll()
                .Include(x => x.Options);
        }
    }
}
