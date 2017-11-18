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

namespace SystemChecker.Model
{
    public interface ISchedulerManager
    {
        void Start();
        void Stop();
        Task UpdateSchedules();
        Task UpdateSchedule(int id);
        void UpdateSchedule(Check check);
        void RemoveSchedule(Check check);
    }

    public class SchedulerManager : ISchedulerManager
    {
        private readonly ISchedulerFactory _factory;
        private readonly IScheduler _scheduler;
        private readonly AppSettings _appSettings;
        private readonly ICheckerUow _uow;
        private readonly ILogger _logger;
        public SchedulerManager(ISchedulerFactory factory, IJobFactory jobFactory, IOptions<AppSettings> appSettings, ICheckerUow uow, ILogger<SchedulerManager> logger)
        {
            _factory = factory;
            _scheduler = factory.GetScheduler();
            _scheduler.JobFactory = jobFactory;
            _scheduler.ListenerManager.AddJobListener(new GlobalJobListener(logger), GroupMatcher<JobKey>.AnyGroup());
            _scheduler.ListenerManager.AddSchedulerListener(new GlobalSchedulerListener(logger));
            _appSettings = appSettings.Value;
            _uow = uow;
            _logger = logger;
        }

        public void Start()
        {
            _scheduler.Start();
            UpdateSchedules().Wait();
        }

        public void Stop()
        {
            _scheduler.Shutdown(true);
        }

        public async Task UpdateSchedules()
        {
            var checks = await _uow.Checks.GetDetails(true);
            foreach (var check in checks)
            {
                UpdateSchedule(check);
            }
        }

        public async Task UpdateSchedule(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
            if (check == null)
            {
                throw new InvalidOperationException($"Check {id} does not exist");
            }
            UpdateSchedule(check);
        }

        public void UpdateSchedule(Check check)
        {
            RemoveSchedule(check);
            if (!check.Active) { return; }
            var type = GetJobForCheck(check);
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["Check"] = check
            };
            var job = JobBuilder.Create(type)
                .WithIdentity(GetJobKeyForCheck(check))
                .SetJobData(new JobDataMap(data))
                .Build();

            _scheduler.AddJob(job, false, true);

            foreach (var schedule in check.Schedules.Where(x => x.Active))
            {
                if (CronExpression.IsValidExpression(schedule.Expression))
                {
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(GetTriggerKeyForSchedule(schedule))
                        .WithCronSchedule(schedule.Expression)
                        .ForJob(job)
                        .Build();

                    _scheduler.ScheduleJob(trigger);
                }
                else
                {
                    _logger.LogError($"Failed to create schedule for ID {schedule.ID} on check {check.Name} ({check.ID}): {schedule.Expression}");
                }
            }
        }

        public void RemoveSchedule(Check check)
        {
            var existing = _scheduler.GetJobDetail(GetJobKeyForCheck(check));
            if (existing != null)
            {
                _scheduler.DeleteJob(existing.Key);
            }
        }


        private TriggerKey GetTriggerKeyForSchedule(CheckSchedule schedule)
        {
            return new TriggerKey($"schedule-{schedule.ID}");
        }

        private JobKey GetJobKeyForCheck(Check check)
        {
            return new JobKey($"check-{check.ID}");
        }

        private Type GetJobForCheck(Check check)
        {
            switch ((Enums.CheckType)check.TypeID)
            {
                case Enums.CheckType.WebRequest:
                    return typeof(WebRequestChecker);
                case Enums.CheckType.Database:
                    return typeof(DatabaseChecker);
                default:
                    throw new InvalidOperationException("Invalid check type");
            }
        }
    }
}
