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
        protected readonly ICheckerHelper _helper;
        protected ICheckLogger _logger;
        protected Check _check;
        protected ISettings _settings;
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
            _logger = logger;
            _check = check;

            CheckResult result = new CheckResult
            {
                CheckID = check.ID,
                Status = CheckResultStatus.Success
            };
            try
            {
                _settings = await _helper.GetSettings();
                logger.Info($"Starting check using {GetType().Name}");
                result = await PerformCheck(result);
            }
            catch (SubCheckException e)
            {
                result.Status = CheckResultStatus.SubCheckFailed;
                logger.Error("Failed to run check - sub-check failed");
                logger.Error(e.ToString());
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
                result.DTS = DateTime.Now;

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

        public abstract Task<CheckResult> PerformCheck(CheckResult result);
    }
}
