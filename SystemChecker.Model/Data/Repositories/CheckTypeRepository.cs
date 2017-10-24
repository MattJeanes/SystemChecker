using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckTypeRepository : EFRepository<CheckType>, ICheckTypeRepository
    {
        public CheckTypeRepository(DbContext dbContext)
            : base(dbContext) { }

        public override IQueryable<CheckType> GetAll()
        {
            return base.GetAll()
                .Include("Options");
        }
    }
}
