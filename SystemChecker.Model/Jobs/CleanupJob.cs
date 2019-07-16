using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class CleanupJob : BaseJob
    {
        private readonly IRepository<CheckResult> _checkResults;
        private readonly ILogger _logger;
        private readonly ISettingsHelper _settingsHelper;
        public CleanupJob(IRepository<CheckResult> checkResults, ILogger<CleanupJob> logger, ISettingsHelper settingsHelper) : base(logger)
        {
            _checkResults = checkResults;
            _logger = logger;
            _settingsHelper = settingsHelper;
        }

        public override async Task ExecuteJob(IJobExecutionContext context)
        {
            var global = await _settingsHelper.GetGlobal();
            if (!global.ResultAggregateDays.HasValue)
            {
                _logger.LogWarning("ResultAggregateDays is unset, cannot run cleanup job");
                return;
            }
            if (!global.ResultRetentionMonths.HasValue)
            {
                _logger.LogWarning("ResultRetentionMonths is unset, cannot run cleanup job");
                return;
            }

            var resultsToAggregate = await _checkResults.GetAll()
                .Where(x => x.DTS < DateTimeOffset.UtcNow.AddDays(-global.ResultAggregateDays.Value))
                .GroupBy(x => new { x.DTS.UtcDateTime.Hour, x.DTS.UtcDateTime.Date, x.StatusID, x.CheckID })
                .Where(x => x.Count() > 1)
                .ToListAsync();

            foreach (var group in resultsToAggregate)
            {
                var timeMSAverage = Convert.ToInt32(group.Average(x => x.TimeMS));
                var ticks = group.Select(x => x.DTS.UtcTicks);
                var avgTicks = ticks.Select(i => i / ticks.Count()).Sum() + ticks.Select(i => i % ticks.Count()).Sum() / ticks.Count();
                var dateAverage = new DateTimeOffset(avgTicks, TimeSpan.Zero);
                _checkResults.Add(new CheckResult
                {
                    CheckID = group.Key.CheckID,
                    DTS = dateAverage,
                    StatusID = group.Key.StatusID,
                    TimeMS = timeMSAverage
                });
            }

            _checkResults.DeleteRange(resultsToAggregate.SelectMany(x => x));
            await _checkResults.SaveChangesAsync();

            var resultsToDelete = await _checkResults.GetAll()
                .Where(x => x.DTS < DateTimeOffset.UtcNow.AddMonths(-global.ResultRetentionMonths.Value))
                .ToListAsync();

            _checkResults.DeleteRange(resultsToDelete);
            await _checkResults.SaveChangesAsync();
        }
    }
}
