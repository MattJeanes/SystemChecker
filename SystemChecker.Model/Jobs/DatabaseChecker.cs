using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public class DatabaseChecker : BaseChecker
    {
        public DatabaseChecker(ISchedulerManager manager) : base(manager) { }
        public async override Task<ICheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger)
        {
            var result = new CheckResult { Status = CheckResultStatus.Success };
            return result;
        }
    }
}
