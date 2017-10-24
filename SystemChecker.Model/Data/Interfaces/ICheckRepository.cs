using SystemChecker.Model.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface ICheckRepository : IRepository<Check>
    {
        Task<Check> GetDetails(int id);
    }
}
