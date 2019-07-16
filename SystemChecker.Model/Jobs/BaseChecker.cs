using Microsoft.Extensions.Logging;
using Nager.Date;
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
    public abstract class BaseChecker : BaseJob
    {
        protected readonly ICheckerHelper _helper;
        protected ICheckLogger _logger;
        protected Check _check;
        protected CheckSchedule _schedule;
        protected CheckerSettings _settings;
        public BaseChecker(ICheckerHelper helper, ILogger<BaseChecker> logger) : base(logger)
        {
            _helper = helper;
        }

        public override async Task ExecuteJob(IJobExecutionContext context)
        {
            var checkID = Convert.ToInt32(context.JobDetail.JobDataMap.Get("CheckID"));
            var scheduleID = Convert.ToInt32(context.Trigger.JobDataMap.Get("ScheduleID"));
            var (check, schedule) = await _helper.GetDetails(checkID, scheduleID);
            var logger = _helper.GetCheckLogger();
            await Run(check, schedule, logger);
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

        public async Task Run(Check check, CheckSchedule schedule, ICheckLogger logger)
        {
            _check = check;
            _schedule = schedule;
            _logger = logger;

            var result = new CheckResult
            {
                CheckID = check.ID
            };
            try
            {
                await _helper.LoadResultStatuses();
                _settings = await _helper.GetSettings();
                if (_schedule?.SkipPublicHolidays ?? false)
                {
                    if (!Enum.TryParse<CountryCode>(_settings.Global.CountryCode, out var countryCode))
                    {
                        throw new Exception($"Please configure country code in global settings to use {nameof(schedule.SkipPublicHolidays)}");
                    }
                    if (DateSystem.IsPublicHoliday(DateTime.Now, countryCode))
                    {
                        logger.Info("Check cancelled as it is a public holiday");
                        return;
                    }
                }
                result.Status = _helper.GetResultStatus(ResultStatusEnum.Success);
                logger.Info($"Starting check using {GetType().Name}");
                result = await PerformCheck(result);
            }
            catch (SubCheckException e)
            {
                result.Status = _helper.GetResultStatus(ResultStatusEnum.SubCheckFailed);
                logger.Error("Failed to run check - sub-check failed");
                logger.Error(e.ToString());
            }
            catch (Exception e)
            {
                result.Status = _helper.GetResultStatus(ResultStatusEnum.Failed);
                logger.Error("Failed to run check");
                logger.Error(e.ToString());
            }
            Action<string> log = logger.Error;
            switch (result.Status?.Type.Identifier)
            {
                case nameof(ResultTypeEnum.Success):
                    log = logger.Info;
                    break;
                case nameof(ResultTypeEnum.Warning):
                    log = logger.Warn;
                    break;
                case nameof(ResultTypeEnum.Failed):
                    log = logger.Error;
                    break;
            }
            log($"Result: {result.Status?.Name ?? "Unknown"} (Type: {result.Status?.Type.Name ?? "Unknown"})");
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

        public abstract Task<CheckResult> PerformCheck(CheckResult result);
    }
}
