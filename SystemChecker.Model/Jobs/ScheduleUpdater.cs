using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Jobs
{
    public class ScheduleUpdater : IJob
    {
        private readonly ISchedulerManager _manager;
        public ScheduleUpdater(ISchedulerManager manager)
        {
            _manager = manager;
        }
        public void Execute(IJobExecutionContext context)
        {
            _manager.UpdateSchedules();
        }
    }
}
