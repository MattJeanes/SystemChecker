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
    public class CleanupJob : IJob, IDisposable
    {
        private readonly ICheckerUow _uow;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        public CleanupJob(ICheckerUow uow, ILogger<CleanupJob> logger, IOptions<AppSettings> appSettings)
        {
            _uow = uow;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var resultsToAggregate = await _uow.CheckResults.GetAll()
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
                _uow.CheckResults.Add(new CheckResult
                {
                    CheckID = group.Key.CheckID,
                    DTS = dateAverage,
                    Status = group.Key.Status,
                    TimeMS = timeMSAverage
                });
            }

            _uow.CheckResults.DeleteRange(resultsToAggregate.SelectMany(x => x));
            await _uow.Commit();

            var resultsToDelete = await _uow.CheckResults.GetAll()
                .Where(x => x.DTS < DateTime.Now.AddMonths(-_appSettings.ResultRetentionMonths))
                .ToListAsync();

            _uow.CheckResults.DeleteRange(resultsToDelete);

            await _uow.Commit();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_uow != null)
                {
                    _uow.Dispose();
                }
            }
        }
    }
}
