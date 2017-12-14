
using SystemChecker.Model.Data.Entities;

namespace SystemChecker.Model.Data.Interfaces
{
    public interface ICheckerUow : IBaseUow
    {
        ICheckRepository Checks { get; }
        IRepository<Login> Logins { get; }
        IRepository<ConnString> ConnStrings { get; }
        ICheckTypeRepository CheckTypes { get; }
        IRepository<CheckSchedule> CheckSchedules { get; }
        IRepository<CheckData> CheckData { get; }
        IRepository<CheckResult> CheckResults { get; }
        ISubCheckTypeRepository SubCheckTypes { get; }
        ICheckNotificationTypeRepository CheckNotificationTypes { get; }
        IRepository<CheckNotification> CheckNotifications { get; }
        IRepository<Environment> Environments { get; }
    }
}