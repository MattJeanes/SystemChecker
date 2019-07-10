using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Jobs;
using SystemChecker.Model.Loggers;
using TimeZoneConverter;

namespace SystemChecker.Model.Helpers
{
    public interface IJobHelper
    {
        Type GetJobForCheck(Check check);
        Task<ManualRunLogger> RunManualUICheck(Check check);
        Task<TimeZoneInfo> GetTimeZone();
    }
    public class JobHelper : IJobHelper
    {
        private readonly IServiceProvider _container;
        public JobHelper(IServiceProvider container)
        {
            _container = container;
        }

        public Type GetJobForCheck(Check check)
        {
            switch ((Contracts.Enums.CheckType)check.TypeID)
            {
                case Contracts.Enums.CheckType.WebRequest:
                    return typeof(WebRequestChecker);
                case Contracts.Enums.CheckType.Database:
                    return typeof(DatabaseChecker);
                case Contracts.Enums.CheckType.Ping:
                    return typeof(PingChecker);
                default:
                    throw new InvalidOperationException("Invalid check type");
            }
        }

        public async Task<ManualRunLogger> RunManualUICheck(Check check)
        {
            var type = GetJobForCheck(check);
            var logger = new ManualRunLogger();
            using (var scope = _container.CreateScope())
            {
                var checker = scope.ServiceProvider.GetRequiredService(type) as BaseChecker;
                await checker.Run(check, null, logger);
            }
            return logger;
        }

        public async Task<TimeZoneInfo> GetTimeZone()
        {
            using (var scope = _container.CreateScope())
            {
                var settingsHelper = scope.ServiceProvider.GetRequiredService<ISettingsHelper>();
                var global = await settingsHelper.GetGlobal();
                if (string.IsNullOrEmpty(global.TimeZoneId))
                {
                    throw new Exception("TimeZoneId is invalid");
                }
                return TZConvert.GetTimeZoneInfo(global.TimeZoneId);
            }
        }
    }
}
