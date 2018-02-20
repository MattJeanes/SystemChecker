using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SystemChecker.Model.Helpers
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _container;
        private readonly ILogger _logger;
        private ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();
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
                _scopes.TryAdd(job, scope);
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
            _scopes.TryRemove(job, out var serviceScope);
            serviceScope?.Dispose();
        }
    }
}
