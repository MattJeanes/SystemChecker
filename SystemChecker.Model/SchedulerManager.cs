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

namespace SystemChecker.Model
{
    public interface ISchedulerManager
    {
        void Start();
        void Stop();
        Task UpdateSchedules();
        Task UpdateSchedule(int id);
        Task UpdateSchedule(Check check);
        Task RemoveSchedule(Check check);
        Task<List<RunLog>> RunCheck(Check check);
        Task<ISettings> GetSettings();
    }

    public class SchedulerManager : ISchedulerManager
    {
        private readonly ISchedulerFactory _factory;
        private readonly IScheduler _scheduler;
        private readonly AppSettings _appSettings;
        private readonly ICheckerUow _uow;
        private readonly ILogger _logger;
        private readonly IServiceProvider _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICheckLogger _checkLogger;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly IMapper _mapper;
        public SchedulerManager(ISchedulerFactory factory, IJobFactory jobFactory, IOptions<AppSettings> appSettings, ICheckerUow uow,
            ILogger<SchedulerManager> logger, IServiceProvider container, ILoggerFactory loggerFactory, ICheckLogger checkLogger,
            IEncryptionHelper encryptionHelper, IMapper mapper)
        {
            _factory = factory;
            _scheduler = factory.GetScheduler().Result;
            _scheduler.JobFactory = jobFactory;
            _scheduler.ListenerManager.AddJobListener(new GlobalJobListener(loggerFactory.CreateLogger<GlobalJobListener>()), GroupMatcher<JobKey>.AnyGroup());
            _scheduler.ListenerManager.AddSchedulerListener(new GlobalSchedulerListener(loggerFactory.CreateLogger<GlobalSchedulerListener>()));
            _appSettings = appSettings.Value;
            _uow = uow;
            _logger = logger;
            _container = container;
            _loggerFactory = loggerFactory;
            _checkLogger = checkLogger;
            _encryptionHelper = encryptionHelper;
            _mapper = mapper;
        }

        public void Start()
        {
            _scheduler.Start().Wait();
            UpdateSchedules().Wait();
        }

        public void Stop()
        {
            _scheduler.Shutdown(true).Wait();
        }

        public async Task UpdateSchedules()
        {
            var checks = await _uow.Checks.GetDetails(true);
            foreach (var check in checks)
            {
                await UpdateSchedule(check);
            }
        }

        public async Task UpdateSchedule(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
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
            var type = GetJobForCheck(check);
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["Check"] = check,
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

        public async Task<List<RunLog>> RunCheck(Check check)
        {
            var type = GetJobForCheck(check);
            var checker = _container.GetService(type) as BaseChecker;
            var logger = new ManualRunLogger();
            await checker.Run(check, logger);
            return logger.GetLog();
        }

        public async Task<ISettings> GetSettings()
        {
            var logins = await _uow.Logins.GetAll().ToListAsync();
            
            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            
            var settings = new Settings
            {
                Logins = _mapper.Map<List<LoginDTO>>(logins),
                ConnStrings = _mapper.Map<List<ConnStringDTO>>(connStrings)
            };

            foreach (var login in settings.Logins)
            {
                login.Password = _encryptionHelper.Decrypt(login.Password);
            }

            foreach (var connString in settings.ConnStrings)
            {
                connString.Value = _encryptionHelper.Decrypt(connString.Value);
            }

            return settings;
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
