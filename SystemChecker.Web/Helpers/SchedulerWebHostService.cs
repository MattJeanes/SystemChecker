using DasMulli.Win32.ServiceUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model;

namespace SystemChecker.Web.Helpers
{
    internal class SchedulerWebHostService : IWin32Service
    {
        public string ServiceName => "SystemChecker";
        private readonly ISchedulerManager _schedulerManager;
        private readonly IWebHost _host;
        private bool stopRequestedByWindows;
        public SchedulerWebHostService(IWebHost host)
        {
            _schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
            _host = host;
        }

        public void Start(string[] args, ServiceStoppedCallback serviceStoppedCallback)
        {
            _host
               .Services
               .GetRequiredService<IApplicationLifetime>()
               .ApplicationStopped
               .Register(() =>
               {
                   if (stopRequestedByWindows == false)
                   {
                       serviceStoppedCallback();
                   }
               });

            _host.Start();
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping service");
            stopRequestedByWindows = true;
            _host.Dispose();
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
