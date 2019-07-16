using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace SystemChecker.Model.Jobs
{
    public abstract class BaseJob : IJob
    {
        private readonly ILogger _logger;

        public BaseJob(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ExecuteJob(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to run job {GetType().Name}");
            }
        }

        public abstract Task ExecuteJob(IJobExecutionContext context);
    }
}