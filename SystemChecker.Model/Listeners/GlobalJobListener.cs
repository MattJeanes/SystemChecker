using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemChecker.Model.Listeners
{
    public class GlobalJobListener : IJobListener
    {
        private ILogger _logger;
        public GlobalJobListener(ILogger logger)
        {
            _logger = logger;
        }

        public virtual string Name
        {
            get { return "MainJobListener"; }
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {

        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {

        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            if (jobException != null)
            {
                // Log/handle error here
                _logger.LogError($"Job Errored : {context.JobDetail.Description} - {jobException.ToString()}");
            }
            else
            {
                _logger.LogInformation($"Job Executed : {context.JobDetail.Description} ({context.JobDetail.Key}) Result ({context.Result ?? "null"}) Next run at {context.NextFireTimeUtc}");
            }
        }
    }
}
