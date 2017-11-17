using System;
using Quartz;
using SystemChecker.Model.Jobs;
using Quartz.Spi;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace SystemChecker.Model
{
    public interface ISchedulerManager
    {
        void Start();
        void Stop();
        void UpdateSchedules();
    }

    public class SchedulerManager : ISchedulerManager
    {
        private readonly ISchedulerFactory _factory;
        private readonly IScheduler _scheduler;
        private readonly AppSettings _appSettings;
        public SchedulerManager(ISchedulerFactory factory, IJobFactory jobFactory, IOptions<AppSettings> appSettings)
        {
            _factory = factory;
            _scheduler = factory.GetScheduler();
            _scheduler.JobFactory = jobFactory;
            _appSettings = appSettings.Value;
        }
        public void Start()
        {
            _scheduler.Start();
            SetupScheduleUpdater();
        }

        public void Stop()
        {
            _scheduler.Shutdown(true);
        }

        public void UpdateSchedules()
        {
            Debug.WriteLine("Hello");
        }

        private void SetupScheduleUpdater()
        {
            var job = JobBuilder.Create<ScheduleUpdater>()
                    .WithIdentity("scheduleupdater", "updater")
                    .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("scheduleupdater", "updater")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_appSettings.UpdateInterval).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, trigger);
        }
    }
}
