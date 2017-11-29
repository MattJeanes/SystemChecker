using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class SubCheckTypeRepository : EFRepository<SubCheckType>, ISubCheckTypeRepository
    {
        public SubCheckTypeRepository(DbContext dbContext)
            : base(dbContext) { }

        public override IQueryable<SubCheckType> GetAll()
        {
            return base.GetAll()
                .Include("Options");
        }
    }
}
