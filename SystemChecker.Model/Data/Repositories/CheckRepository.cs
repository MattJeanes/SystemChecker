using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckRepository : EFRepository<Check>, ICheckRepository
    {
        public CheckRepository(DbContext dbContext)
            : base(dbContext) { }

        public async Task<Check> GetDetails(int id)
        {
            return await GetDetailsQuery()
                .Where(x => x.ID == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Check>> GetDetails()
        {
            return await GetDetailsQuery()
                .ToListAsync();
        }

        private IQueryable<Check> GetDetailsQuery()
        {
            return GetAll()
                .Include(x => x.Schedules)
                .Include(x => x.Data)
                .Include(x => x.Type);
        }
    }
}
