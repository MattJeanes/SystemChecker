using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading;
using System.Threading.Tasks;

namespace SystemChecker.Model.Listeners
{
    public class GlobalSchedulerListener : ISchedulerListener
    {
        private readonly ILogger _logger;
        private readonly ISchedulerManager _schedulerManager;

        public GlobalSchedulerListener(ILogger<GlobalSchedulerListener> logger, ISchedulerManager schedulerManager)
        {
            _logger = logger;
            _schedulerManager = schedulerManager;
        }

        public Task JobAdded(IJobDetail jobDetail, CancellationToken token)
        {
            _logger.LogDebug($"Job added: {jobDetail.Key.Group} - {jobDetail.Key.Name}");
            return Task.CompletedTask;
        }

        public Task JobDeleted(JobKey jobKey, CancellationToken token)
        {
            _logger.LogDebug($"Job deleted: {jobKey.Group} - {jobKey.Name}");
            return Task.CompletedTask;
        }

        public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task JobPaused(JobKey jobKey, CancellationToken token)
        {
            _logger.LogDebug($"Job paused: {jobKey.Group} - {jobKey.Name}");
            return Task.CompletedTask;
        }

        public Task JobResumed(JobKey jobKey, CancellationToken token)
        {
            _logger.LogDebug($"Job resumed: {jobKey.Group} - {jobKey.Name}");
            return Task.CompletedTask;
        }

        public Task JobScheduled(ITrigger trigger, CancellationToken token)
        {
            _logger.LogDebug($"Job scheduled: {trigger.Key.Group} - {trigger.Key.Name}");
            return Task.CompletedTask;
        }

        public Task JobsPaused(string jobGroup, CancellationToken token)
        {
            _logger.LogDebug($"Jobs paused: {jobGroup}");
            return Task.CompletedTask;
        }

        public Task JobsResumed(string jobGroup, CancellationToken token)
        {
            _logger.LogDebug($"Jobs resumed: {jobGroup}");
            return Task.CompletedTask;
        }

        public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken token)
        {
            _logger.LogDebug($"Job unscheduled: {triggerKey.Group} - {triggerKey.Name}");
            return Task.CompletedTask;
        }

        public Task SchedulerError(string msg, SchedulerException cause, CancellationToken token)
        {
            _logger.LogError(cause, msg);
            return Task.CompletedTask;
        }

        public Task SchedulerInStandbyMode(CancellationToken token)
        {
            _logger.LogDebug("Scheduler standby");
            return Task.CompletedTask;
        }

        public Task SchedulerShutdown(CancellationToken token)
        {
            _logger.LogDebug("Scheduler shutdown");
            return Task.CompletedTask;
        }

        public Task SchedulerShuttingdown(CancellationToken token)
        {
            _logger.LogDebug("Scheduler shutting down");
            return Task.CompletedTask;
        }

        public Task SchedulerStarted(CancellationToken token)
        {
            _logger.LogDebug("Scheduler started");
            return Task.CompletedTask;
        }

        public Task SchedulerStarting(CancellationToken token)
        {
            _logger.LogDebug("Scheduler starting");
            return Task.CompletedTask;
        }

        public Task SchedulingDataCleared(CancellationToken token)
        {
            _logger.LogDebug("Scheduling data cleared");
            return Task.CompletedTask;
        }

        public Task TriggerFinalized(ITrigger trigger, CancellationToken token)
        {
            _logger.LogDebug($"Trigger finalized: {trigger.Key.Group} - {trigger.Key.Name}");
            return Task.CompletedTask;
        }

        public Task TriggerPaused(TriggerKey triggerKey, CancellationToken token)
        {
            _logger.LogDebug($"Trigger paused: {triggerKey.Group} - {triggerKey.Name}");
            return Task.CompletedTask;
        }

        public Task TriggerResumed(TriggerKey triggerKey, CancellationToken token)
        {
            _logger.LogDebug($"Trigger resumed: {triggerKey.Group} - {triggerKey.Name}");
            return Task.CompletedTask;
        }

        public Task TriggersPaused(string triggerGroup, CancellationToken token)
        {
            _logger.LogDebug($"Triggers paused: {triggerGroup}");
            return Task.CompletedTask;
        }

        public Task TriggersResumed(string triggerGroup, CancellationToken token)
        {
            _logger.LogDebug($"Triggers resumed: {triggerGroup}");
            return Task.CompletedTask;
        }
    }

}
