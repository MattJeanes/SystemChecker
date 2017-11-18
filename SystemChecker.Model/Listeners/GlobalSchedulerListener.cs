using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Listeners
{
    internal class GlobalSchedulerListener : ISchedulerListener
    {
        private ILogger _logger;

        public GlobalSchedulerListener(ILogger logger)
        {
            _logger = logger;
        }

        public void JobAdded(IJobDetail jobDetail)
        {
            _logger.LogDebug($"Job added: {jobDetail.Key.Group} - {jobDetail.Key.Name}");
        }

        public void JobDeleted(JobKey jobKey)
        {
            _logger.LogDebug($"Job deleted: {jobKey.Group} - {jobKey.Name}");
        }

        public void JobPaused(JobKey jobKey)
        {
            _logger.LogDebug($"Job paused: {jobKey.Group} - {jobKey.Name}");
        }

        public void JobResumed(JobKey jobKey)
        {
            _logger.LogDebug($"Job resumed: {jobKey.Group} - {jobKey.Name}");
        }

        public void JobScheduled(ITrigger trigger)
        {
            _logger.LogDebug($"Job scheduled: {trigger.Key.Group} - {trigger.Key.Name}");
        }

        public void JobsPaused(string jobGroup)
        {
            _logger.LogDebug($"Jobs paused: {jobGroup}");
        }

        public void JobsResumed(string jobGroup)
        {
            _logger.LogDebug($"Jobs resumed: {jobGroup}");
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            _logger.LogDebug($"Job unscheduled: {triggerKey.Group} - {triggerKey.Name}");
        }

        public void SchedulerError(string msg, SchedulerException cause)
        {
            _logger.LogError($"Scheduler error: {msg}: {cause.Message}");
        }

        public void SchedulerInStandbyMode()
        {
            _logger.LogDebug("Scheduler standby");
        }

        public void SchedulerShutdown()
        {
            _logger.LogDebug("Scheduler shutdown");
        }

        public void SchedulerShuttingdown()
        {
            _logger.LogDebug("Scheduler shutting down");
        }

        public void SchedulerStarted()
        {
            _logger.LogDebug("Scheduler started");
        }

        public void SchedulerStarting()
        {
            _logger.LogDebug("Scheduler starting");
        }

        public void SchedulingDataCleared()
        {
            _logger.LogDebug("Scheduling data cleared");
        }

        public void TriggerFinalized(ITrigger trigger)
        {
            _logger.LogDebug($"Trigger finalized: {trigger.Key.Group} - {trigger.Key.Name}");
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            _logger.LogDebug($"Trigger paused: {triggerKey.Group} - {triggerKey.Name}");
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            _logger.LogDebug($"Trigger resumed: {triggerKey.Group} - {triggerKey.Name}");
        }

        public void TriggersPaused(string triggerGroup)
        {
            _logger.LogDebug($"Triggers paused: {triggerGroup}");
        }

        public void TriggersResumed(string triggerGroup)
        {
            _logger.LogDebug($"Triggers resumed: {triggerGroup}");
        }
    }

}
