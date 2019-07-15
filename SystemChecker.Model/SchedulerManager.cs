using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Contracts.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Jobs;
using SystemChecker.Model.Listeners;

namespace SystemChecker.Model
{
    public interface ISchedulerManager
    {
        Task Start();
        Task Stop();
        void CriticalError(Exception ex, string message);
        Task UpdateSchedules();
        Task UpdateSchedule(int id);
        Task UpdateSchedule(Check check);
        Task RemoveSchedule(Check check);
        Task<List<ITrigger>> GetAllTriggers();
        Task UpdateCleanupJob(GlobalSettings global = null);
        Task UpdateMaintenanceJobs(GlobalSettings global);
        event EventHandler OnCriticalError;
    }

    public class SchedulerManager : ISchedulerManager
    {
        private readonly IScheduler _scheduler;
        private readonly ISettingsHelper _settingsHelper;
        private readonly ICheckRepository _checks;
        private readonly ILogger _logger;
        private readonly IJobHelper _jobHelper;
        private bool _failed;

        public event EventHandler OnCriticalError;

        public SchedulerManager(ISchedulerFactory factory, IJobFactory jobFactory, ISettingsHelper settingsHelper, ICheckRepository checks,
            ILogger<SchedulerManager> logger, ILoggerFactory loggerFactory, IJobHelper jobHelper)
        {
            _scheduler = factory.GetScheduler().Result;
            _scheduler.JobFactory = jobFactory;
            _scheduler.ListenerManager.AddJobListener(new GlobalJobListener(loggerFactory.CreateLogger<GlobalJobListener>()), GroupMatcher<JobKey>.AnyGroup());
            _scheduler.ListenerManager.AddSchedulerListener(new GlobalSchedulerListener(loggerFactory.CreateLogger<GlobalSchedulerListener>(), this));
            _settingsHelper = settingsHelper;
            _checks = checks;
            _logger = logger;
            _jobHelper = jobHelper;
        }

        public async Task Start()
        {
            _logger.LogInformation("Scheduler starting");
            await _scheduler.Start();
            await UpdateSchedules();
            await UpdateMaintenanceJobs();
            if (_failed)
            {
                await Stop();
                throw new Exception("Critical scheduler startup failure");
            }
            _logger.LogInformation("Scheduler started");
        }

        public async Task Stop()
        {
            _logger.LogInformation("Scheduler stopping");
            await _scheduler.Shutdown(true);
            _logger.LogInformation("Scheduler stopped");
        }

        public void CriticalError(Exception ex, string message)
        {
            _failed = true;
            _logger.LogError(ex, $"Critical scheduler error: {message}");
            OnCriticalError?.Invoke(this, new EventArgs());
        }

        public async Task UpdateMaintenanceJobs(GlobalSettings global = null)
        {
            if (global == null)
            {
                global = await _settingsHelper.GetGlobal();
            }
            await UpdateCleanupJob(global);
        }

        public async Task UpdateSchedules()
        {
            var checks = await _checks.GetDetails(true);
            foreach (var check in checks)
            {
                await UpdateSchedule(check);
            }
        }

        public async Task UpdateSchedule(int id)
        {
            var check = await _checks.GetDetails(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check {id} does not exist");
            }
            await UpdateSchedule(check);
        }

        public async Task UpdateSchedule(Check check)
        {
            await RemoveSchedule(check);
            if (!check.Active) { return; }
            var type = _jobHelper.GetJobForCheck(check);
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["CheckID"] = check.ID,
            };
            var job = JobBuilder.Create(type)
                .WithIdentity(GetJobKeyForCheck(check))
                .SetJobData(new JobDataMap(data))
                .Build();

            await _scheduler.AddJob(job, false, true);

            var timeZone = await _jobHelper.GetTimeZone();

            foreach (var schedule in check.Schedules.Where(x => x.Active))
            {
                if (CronExpression.IsValidExpression(schedule.Expression))
                {
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(GetTriggerKeyForSchedule(schedule))
                        .WithCronSchedule(schedule.Expression, x => x.InTimeZone(timeZone))
                        .ForJob(job)
                        .UsingJobData("ScheduleID", schedule.ID)
                        .Build();

                    await _scheduler.ScheduleJob(trigger);
                }
                else
                {
                    _logger.LogError($"Failed to create schedule for ID {schedule.ID} on check {check.Name} ({check.ID}): {schedule.Expression}");
                }
            }
        }

        public async Task RemoveSchedule(Check check)
        {
            var existing = await _scheduler.GetJobDetail(GetJobKeyForCheck(check));
            if (existing != null)
            {
                await _scheduler.DeleteJob(existing.Key);
            }
        }

        public async Task<List<ITrigger>> GetAllTriggers()
        {
            var allTriggerKeys = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            var triggers = new List<ITrigger>();
            foreach (var triggerKey in allTriggerKeys)
            {
                triggers.Add(await _scheduler.GetTrigger(triggerKey));
            }
            return triggers;
        }

        public async Task UpdateCleanupJob(GlobalSettings global)
        {
            var key = new JobKey("cleanup");
            var existing = await _scheduler.GetJobDetail(key);
            if (existing != null)
            {
                await _scheduler.DeleteJob(existing.Key);
            }
            if (string.IsNullOrEmpty(global.CleanupSchedule))
            {
                _logger.LogWarning("Cleanup job schedule not set, cancelling job");
                return;
            }

            var job = JobBuilder.Create<CleanupJob>()
                .WithIdentity(key)
                .Build();

            await _scheduler.AddJob(job, false, true);

            var timeZone = await _jobHelper.GetTimeZone();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("cleanup")
                .WithCronSchedule(global.CleanupSchedule, x => x.InTimeZone(timeZone))
                .ForJob(job)
                .Build();

            await _scheduler.ScheduleJob(trigger);
            _logger.LogInformation($"Setup cleanup job with schedule {global.CleanupSchedule}");
            //await _scheduler.TriggerJob(new JobKey("cleanup"));
        }

        private TriggerKey GetTriggerKeyForSchedule(CheckSchedule schedule)
        {
            return new TriggerKey($"schedule-{schedule.ID}");
        }

        private JobKey GetJobKeyForCheck(Check check)
        {
            return new JobKey($"check-{check.ID}");
        }
    }
}
