using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SystemChecker.Model.Helpers
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _container;
        private readonly ILogger _logger;
        private Dictionary<IJob, IServiceScope> _scopes = new Dictionary<IJob, IServiceScope>();
        public JobFactory(IServiceProvider container, ILogger<JobFactory> logger)
        {
            _container = container;
            _logger = logger;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var scope = _container.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
                _scopes[job] = scope;
                return job;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create {bundle.JobDetail.JobType.Name} for {bundle.JobDetail.Key}");
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
            _scopes[job]?.Dispose();
            _scopes.Remove(job);
        }
    }
}
