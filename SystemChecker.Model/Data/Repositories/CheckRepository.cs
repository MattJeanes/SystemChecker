using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class CheckRepository : EFRepository<Check>, ICheckRepository
    {
        public CheckRepository(DbContext dbContext)
            : base(dbContext) { }

        public async Task<Check> GetDetails(int id)
        {
            return await GetAll()
                .Where(x => x.ID == id)
                .Include("Schedules")
                .Include("Data")
                .FirstOrDefaultAsync();
        }
    }
}
