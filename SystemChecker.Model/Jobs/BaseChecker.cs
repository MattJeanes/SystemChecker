using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public abstract class BaseChecker : IJob
    {
        private readonly ICheckerHelper _helper;
        public BaseChecker(ICheckerHelper helper)
        {
            _helper = helper;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var check = context.JobDetail.JobDataMap.Get("Check") as Check;
            var logger = context.JobDetail.JobDataMap.Get("Logger") as ICheckLogger;
            await Run(check, logger);
        }

        public async Task Run(Check check, ICheckLogger logger)
        {
            CheckResult result = new CheckResult
            {
                Check = check,
                Status = CheckResultStatus.Success
            };
            try
            {
                var settings = await _helper.GetSettings();
                logger.Info($"Starting check using {GetType().Name}");
                result = await PerformCheck(check, settings, logger, result);
            }
            catch (Exception e)
            {
                result.Status = CheckResultStatus.Failed;
                logger.Error("Failed to run check");
                logger.Error(e.ToString());
            }
            finally
            {
                Action<string> log;
                if (result.Status == CheckResultStatus.Success)
                {
                    log = logger.Info;
                }
                else if ((int)result.Status > (int)CheckResultStatus.Success)
                {
                    log = logger.Warn;
                }
                else
                {
                    log = logger.Error;
                }
                log($"Result: {result.Status.ToString()}");
                if (result.TimeMS > 0)
                {
                    log($"TimeMS: {result.TimeMS}");
                }
                logger.Done("Check completed");

                try
                {
                    await _helper.SaveResult(result);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to save check result");
                    logger.Error(e.ToString());
                }
            }
        }

        public abstract Task<CheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger, CheckResult result);
    }
}
