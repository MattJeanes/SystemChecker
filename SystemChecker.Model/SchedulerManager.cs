using System;
using Quartz;
using SystemChecker.Model.Jobs;
using Quartz.Spi;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using SystemChecker.Model.Data.Interfaces;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Quartz.Impl.Matchers;
using SystemChecker.Model.Data.Entities;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SystemChecker.Model.Listeners;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;
using SystemChecker.Model.Data;
using AutoMapper;
using SystemChecker.Model.DTO;
using Microsoft.Extensions.DependencyInjection;

namespace SystemChecker.Model
{
    public interface ISchedulerManager
    {
        Task Start();
        Task Stop();
        Task UpdateSchedules();
        Task UpdateSchedule(int id);
        Task UpdateSchedule(Check check);
        Task RemoveSchedule(Check check);
        Task<List<ITrigger>> GetAllTriggers();
    }

    public class SchedulerManager : ISchedulerManager
    {
        private readonly ISchedulerFactory _factory;
        private readonly IScheduler _scheduler;
        private readonly AppSettings _appSettings;
        private readonly ICheckRepository _checks;
        private readonly ILogger _logger;
        private readonly IServiceProvider _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICheckLogger _checkLogger;
        private readonly IJobHelper _jobHelper;
        private readonly IMapper _mapper;
        public SchedulerManager(ISchedulerFactory factory, IJobFactory jobFactory, IOptions<AppSettings> appSettings, ICheckRepository checks,
            ILogger<SchedulerManager> logger, IServiceProvider container, ILoggerFactory loggerFactory, ICheckLogger checkLogger, IJobHelper jobHelper, IMapper mapper)
        {
            _factory = factory;
            _scheduler = factory.GetScheduler().Result;
            _scheduler.JobFactory = jobFactory;
            _scheduler.ListenerManager.AddJobListener(new GlobalJobListener(loggerFactory.CreateLogger<GlobalJobListener>()), GroupMatcher<JobKey>.AnyGroup());
            _scheduler.ListenerManager.AddSchedulerListener(new GlobalSchedulerListener(loggerFactory.CreateLogger<GlobalSchedulerListener>()));
            _appSettings = appSettings.Value;
            _checks = checks;
            _logger = logger;
            _container = container;
            _loggerFactory = loggerFactory;
            _checkLogger = checkLogger;
            _jobHelper = jobHelper;
            _mapper = mapper;
        }

        public async Task Start()
        {
            await _scheduler.Start();
            await UpdateSchedules();
            await SetupMaintenanceJobs();
        }

        public async Task Stop()
        {
            await _scheduler.Shutdown(true);
        }

        public async Task SetupMaintenanceJobs()
        {
            var job = JobBuilder.Create<CleanupJob>()
                .WithIdentity("cleanup")
                .Build();

            await _scheduler.AddJob(job, false, true);

            var trigger = TriggerBuilder.Create()
                .WithIdentity("cleanup")
                .WithCronSchedule(_appSettings.CleanupSchedule)
                .ForJob(job)
                .Build();

            await _scheduler.ScheduleJob(trigger);
            //await _scheduler.TriggerJob(new JobKey("cleanup"));
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
                ["Logger"] = _checkLogger
            };
            var job = JobBuilder.Create(type)
                .WithIdentity(GetJobKeyForCheck(check))
                .SetJobData(new JobDataMap(data))
                .Build();

            await _scheduler.AddJob(job, false, true);

            foreach (var schedule in check.Schedules.Where(x => x.Active))
            {
                if (CronExpression.IsValidExpression(schedule.Expression))
                {
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(GetTriggerKeyForSchedule(schedule))
                        .WithCronSchedule(schedule.Expression)
                        .ForJob(job)
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
