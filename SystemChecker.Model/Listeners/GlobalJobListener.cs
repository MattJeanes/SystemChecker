using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SystemChecker.Model.Listeners
{
    public class GlobalJobListener : IJobListener
    {
        private readonly ILogger _logger;
        public GlobalJobListener(ILogger<GlobalJobListener> logger)
        {
            _logger = logger;
        }

        public virtual string Name
        {
            get { return "MainJobListener"; }
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken token)
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
            return Task.CompletedTask;
        }
    }
}
