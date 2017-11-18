using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public abstract class BaseChecker : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var check = context.JobDetail.JobDataMap.Get("Check") as Check;
            PerformCheck(check);
        }

        public abstract void PerformCheck(Check check);
    }
}
