using Microsoft.EntityFrameworkCore;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Helpers;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Data
{
    public class CheckerUow : BaseUow<CheckerContext>, ICheckerUow
    {
        public CheckerUow(IRepositoryProvider repositoryProvider, DbContextOptions options)
            : base(repositoryProvider, options)
        { }

        public ICheckRepository Checks { get { return GetRepo<ICheckRepository>(); } }
        public IRepository<Login> Logins { get { return GetStandardRepo<Login>(); } }
        public IRepository<ConnString> ConnStrings { get { return GetStandardRepo<ConnString>(); } }
        public ICheckTypeRepository CheckTypes { get { return GetRepo<ICheckTypeRepository>(); } }
        public IRepository<CheckSchedule> CheckSchedules { get { return GetStandardRepo<CheckSchedule>(); } }
        public IRepository<CheckData> CheckData { get { return GetStandardRepo<CheckData>(); } }
        public IRepository<CheckResult> CheckResults { get { return GetStandardRepo<CheckResult>(); } }
        public ISubCheckTypeRepository SubCheckTypes { get { return GetRepo<ISubCheckTypeRepository>(); } }
    }
}