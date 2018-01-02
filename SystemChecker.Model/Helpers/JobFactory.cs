using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace SystemChecker.Model.Helpers
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _container;
        private Dictionary<IJob, IServiceScope> _scopes = new Dictionary<IJob, IServiceScope>();
        public JobFactory(IServiceProvider container)
        {
            _container = container;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _container.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            _scopes[job] = scope;
            return job;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
            _scopes[job]?.Dispose();
            _scopes.Remove(job);
        }
    }
}
