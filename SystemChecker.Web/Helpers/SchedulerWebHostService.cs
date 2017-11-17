using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using SystemChecker.Model;

namespace SystemChecker.Web.Helpers
{
    internal class SchedulerWebHostService : WebHostService
    {
        private readonly ISchedulerManager _schedulerManager;
        public SchedulerWebHostService(IWebHost host) : base(host)
        {
            _schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
        }

        protected override void OnStarting(string[] args)
        {
            _schedulerManager.Start();
            base.OnStarting(args);
        }

        protected override void OnStopping()
        {
            _schedulerManager.Stop();
            base.OnStopping();
        }
    }

    public static class WebHostServiceExtensions
    {
        public static void RunAsSchedulerService(this IWebHost host)
        {
            var webHostService = new SchedulerWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}
