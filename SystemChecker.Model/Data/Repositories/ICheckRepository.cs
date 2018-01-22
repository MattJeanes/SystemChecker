using SystemChecker.Model.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface ICheckRepository : IRepository<Check>
    {
        Task<Check> GetDetails(int id, bool includeResults = false);
        Task<List<Check>> GetDetails(bool activeOnly = false);
    }
}
