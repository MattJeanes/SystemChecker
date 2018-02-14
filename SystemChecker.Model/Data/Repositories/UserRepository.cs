using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Model.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ICheckerContext context)
            : base(context) { }

        public async Task<User> GetByUsername(string username, bool? isWindowsUser = null)
        {
            var query = base.GetAll();

            if (isWindowsUser.HasValue)
            {
                query = query.Where(x => x.IsWindowsUser == isWindowsUser);
            }

            return await query.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<User> GetDetails(string username)
        {
            return await base.GetAll()
                .Include(x => x.ApiKeys)
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<User> GetDetails(int id)
        {
            return await base.GetAll()
                .Include(x => x.ApiKeys)
                .FirstOrDefaultAsync(x => x.ID == id);
        }
    }
}
