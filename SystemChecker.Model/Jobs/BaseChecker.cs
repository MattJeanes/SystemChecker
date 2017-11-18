using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public abstract class BaseChecker : IJob
    {
        private readonly ISchedulerManager _manager;
        public BaseChecker(ISchedulerManager manager)
        {
            _manager = manager;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var check = context.JobDetail.JobDataMap.Get("Check") as Check;
            var logger = context.JobDetail.JobDataMap.Get("Logger") as ICheckLogger;
            await Run(check, logger);
        }

        public async Task Run(Check check, ICheckLogger logger)
        {
            try
            {
                var settings = await _manager.GetSettings();
                logger.Info($"Starting check using {GetType().Name}");
                var result = await PerformCheck(check, settings, logger);
                logger.Info("Result:");
                logger.Info(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception e)
            {
                logger.Error("Failed to run check");
                logger.Error(e.ToString());
            }
            finally
            {
                logger.Done("Check completed");
            }
        }

        public abstract Task<ICheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger);
    }
}
