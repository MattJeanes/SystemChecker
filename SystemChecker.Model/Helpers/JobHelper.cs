using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Jobs;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Helpers
{
    public interface IJobHelper
    {
        Type GetJobForCheck(Check check);
        Task<ManualRunLogger> RunManualUICheck(Check check);
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
            switch ((Enums.CheckType)check.TypeID)
            {
                case Enums.CheckType.WebRequest:
                    return typeof(WebRequestChecker);
                case Enums.CheckType.Database:
                    return typeof(DatabaseChecker);
                case Enums.CheckType.Ping:
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
                var checker = _container.GetService(type) as BaseChecker;
                await checker.Run(check, logger);
            }
            return logger;
        }
    }
}
