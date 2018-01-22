using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public class CleanupJob : IJob
    {
        private readonly IRepository<CheckResult> _checkResults;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        public CleanupJob(IRepository<CheckResult> checkResults, ILogger<CleanupJob> logger, IOptions<AppSettings> appSettings)
        {
            _checkResults = checkResults;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var resultsToAggregate = await _checkResults.GetAll()
                .Where(x => x.DTS < DateTime.Now.AddDays(-_appSettings.ResultAggregateDays))
                .GroupBy(x => new { x.DTS.Hour, x.DTS.Date, x.Status, x.CheckID })
                .Where(x => x.Count() > 1)
                .ToListAsync();

            foreach (var group in resultsToAggregate)
            {
                var timeMSAverage = Convert.ToInt32(group.Average(x => x.TimeMS));
                var ticks = group.Select(x => x.DTS.Ticks);
                var avgTicks = ticks.Select(i => i / ticks.Count()).Sum() + ticks.Select(i => i % ticks.Count()).Sum() / ticks.Count();
                var dateAverage = new DateTime(avgTicks);
                await _checkResults.Add(new CheckResult
                {
                    CheckID = group.Key.CheckID,
                    DTS = dateAverage,
                    Status = group.Key.Status,
                    TimeMS = timeMSAverage
                });
            }

            await _checkResults.DeleteRange(resultsToAggregate.SelectMany(x => x));

            var resultsToDelete = await _checkResults.GetAll()
                .Where(x => x.DTS < DateTime.Now.AddMonths(-_appSettings.ResultRetentionMonths))
                .ToListAsync();

            await _checkResults.DeleteRange(resultsToDelete);
        }
    }
}
