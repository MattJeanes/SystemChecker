using DasMulli.Win32.ServiceUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model;

namespace SystemChecker.Web.Helpers
{
    internal class SchedulerWebHostService : IWin32Service
    {
        public string ServiceName => "SystemChecker";
        private readonly ISchedulerManager _schedulerManager;
        public SchedulerWebHostService(IWebHost host)
        {
            _schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
        }

        public void Start(string[] args, ServiceStoppedCallback callback)
        {
            _schedulerManager.Start();
        }

        public void Stop()
        {
            _schedulerManager.Stop();
        }
    }

    public static class WebHostServiceExtensions
    {
        public static void RunAsSchedulerService(this IWebHost host)
        {
            var webHostService = new SchedulerWebHostService(host);
            var serviceHost = new Win32ServiceHost(webHostService);
            serviceHost.Run();
        }
    }
}
