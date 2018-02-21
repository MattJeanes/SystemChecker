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
        private readonly ILogger _logger;
        private readonly IWebHost _host;
        private bool stopRequestedByWindows;
        public SchedulerWebHostService(IWebHost host, ILogger logger)
        {
            _schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
            _logger = logger;
            _host = host;
        }

        public void Start(string[] args, ServiceStoppedCallback serviceStoppedCallback)
        {
            try
            {
                _logger.LogInformation("Starting service");
                _host
                   .Services
                   .GetRequiredService<IApplicationLifetime>()
                   .ApplicationStopped
                   .Register(() =>
                   {
                       if (stopRequestedByWindows == false)
                       {
                           _logger.LogInformation("Service has stopped");
                           serviceStoppedCallback();
                       }
                   });

                _host.Start();
                _logger.LogInformation("Started service");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start service");
            }
        }

        public void Stop()
        {
            try
            {
                _logger.LogInformation("Stopping service");
                stopRequestedByWindows = true;
                _host.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start service");
            }
        }
    }

    public static class WebHostServiceExtensions
    {
        public static void RunAsSchedulerService(this IWebHost host)
        {
            var logger = host.Services.GetRequiredService<ILogger<SchedulerWebHostService>>();
            logger.LogInformation(Directory.GetCurrentDirectory());
            try
            {
                var webHostService = new SchedulerWebHostService(host, logger);
                var serviceHost = new Win32ServiceHost(webHostService);
                serviceHost.Run();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to run scheduler service");
            }
        }
    }
}
