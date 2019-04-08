using Microsoft.EntityFrameworkCore;
using System.Linq;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Data.Repositories
{
    public class SubCheckTypeRepository : Repository<SubCheckType>, ISubCheckTypeRepository
    {
        public SubCheckTypeRepository(ICheckerContext context)
            : base(context) { }

        public override IQueryable<SubCheckType> GetAll()
        {
            return base.GetAll()
                .Include(x => x.Options);
        }
    }
}
