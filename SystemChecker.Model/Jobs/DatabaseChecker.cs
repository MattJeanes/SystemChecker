using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;

namespace SystemChecker.Model.Jobs
{
    public class DatabaseChecker : BaseChecker
    {
        private readonly ILogger _logger;
        public DatabaseChecker(ILogger<DatabaseChecker> logger)
        {
            _logger = logger;
        }
        public override void PerformCheck(Check check)
        {
            _logger.LogInformation($"Running '{check.Name}' check");
        }
    }
}
