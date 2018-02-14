using SystemChecker.Model.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetDetails(string username);
        Task<User> GetDetails(int id);
        Task<User> GetByUsername(string username, bool? isWindowsUser = null);
    }
}
