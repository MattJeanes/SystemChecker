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
        private readonly IWebHost _host;
        private readonly ISchedulerManager _schedulerManager;
        private readonly ILogger _logger;
        private bool stopRequestedByWindows;
        public SchedulerWebHostService(IWebHost host)
        {
            _host = host;
            _schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
            _logger = host.Services.GetRequiredService<ILogger<SchedulerWebHostService>>();
        }

        public void Start(string[] args, ServiceStoppedCallback serviceStoppedCallback)
        {
            _logger.LogInformation("Service starting");
            _host
               .Services
               .GetRequiredService<IApplicationLifetime>()
               .ApplicationStopped
               .Register(() =>
               {
                   if (stopRequestedByWindows == false)
                   {
                       _logger.LogInformation("Service stopped");
                       serviceStoppedCallback();
                   }
               });

            _host.Start();
            _schedulerManager.Start().Wait();
        }

        public void Stop()
        {
            _logger.LogInformation("Stop requested by Windows");
            stopRequestedByWindows = true;
            _host.Dispose();
            _schedulerManager.Stop().Wait();
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
