using Quartz;
using System;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Contracts.Enums;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public abstract class BaseChecker : IJob
    {
        protected readonly ICheckerHelper _helper;
        protected ICheckLogger _logger;
        protected Check _check;
        protected CheckerSettings _settings;
        public BaseChecker(ICheckerHelper helper)
        {
            _helper = helper;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checkID = Convert.ToInt32(context.JobDetail.JobDataMap.Get("CheckID"));
            var check = await _helper.GetDetails(checkID);
            var logger = _helper.GetCheckLogger();
            await Run(check, logger);
            try
            {
                await _helper.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.Error("Failed to commit changes");
                logger.Error(e.ToString());
            }
        }

        public async Task Run(Check check, ICheckLogger logger)
        {
            _check = check;
            _logger = logger;

            var result = new CheckResult
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
                result.DTS = DateTime.UtcNow;

                try
                {
                    await _helper.SaveResult(result);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to save check result");
                    logger.Error(e.ToString());
                }

                try
                {
                    await _helper.RunNotifiers(check, result, _settings, logger);
                }
                catch (Exception e)
                {
                    logger.Error("Failed to run notifiers");
                    logger.Error(e.ToString());
                }

                logger.Done("Check completed");
            }
        }

        public abstract Task<CheckResult> PerformCheck(CheckResult result);
    }
}
