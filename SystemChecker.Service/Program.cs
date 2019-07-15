using DasMulli.Win32.ServiceUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SystemChecker.Model;

namespace SystemChecker.Service
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var isService = false;
            var app = new CommandLineApplication();
            var service = app.Option("-s|--service", "Launch in service mode", CommandOptionType.NoValue);
            var help = app.Option("-? | -h | --help", "Show help information", CommandOptionType.NoValue);

            app.Execute(args);

            if (help.Value() != null)
            {
                app.ShowHelp();
                return 0;
            }

            if (service.Value() != null)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.WriteLine("Running as a service only available on Windows");
                }
                else
                {
                    isService = true;
                }
            }

            var workingDirectory = Directory.GetCurrentDirectory();
            if (isService)
            {
                var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Directory.SetCurrentDirectory(path);
                workingDirectory = path;
            }

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder();
            config.SetBasePath(workingDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();

            var Configuration = config.Build();

            var services = new ServiceCollection();

            services.AddLogging(logging =>
            {
                var loggingConfiguration = Configuration.GetSection("Logging");
                logging.AddConfiguration(loggingConfiguration);
                logging.AddConsole();
                logging.AddDebug();
                if (env != EnvironmentName.Development)
                {
                    logging.AddFile(loggingConfiguration);
                }
            });

            IServiceProvider serviceProvider;
            try
            {
                services.AddSystemChecker(Configuration);
                serviceProvider = services.BuildServiceProvider();
            }
            catch (Exception e)
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging(builder =>
                {
                    builder
                        .AddFile("logs/systemchecker-critical-{Date}.log")
                        .AddDebug()
                        .AddConsole();
                });
                using (var logProvider = serviceCollection.BuildServiceProvider())
                {
                    var critLogger = logProvider.GetRequiredService<ILogger<Program>>();
                    critLogger.LogInformation($"Current directory: {Directory.GetCurrentDirectory()}");
                    critLogger.LogError(e, "Critical failure in host build");
                }
                return 1;
            }

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                if (isService)
                {
                    logger.LogInformation("Starting as service");
                    RunAsService(serviceProvider);
                }
                else
                {
                    logger.LogInformation("Starting with scheduler");
                    await Run(serviceProvider);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Fatal error");
                Thread.Sleep(1000); // allow time to flush logs in the event of a crash
                return 1;
            }

            return 0;
        }

        private static void RunAsService(IServiceProvider services)
        {
            var schedulerService = new SchedulerService(services);
            var serviceHost = new Win32ServiceHost(schedulerService);
            serviceHost.Run();
        }

        private static async Task Run(IServiceProvider services)
        {
            var schedulerManager = services.GetRequiredService<ISchedulerManager>();
            await schedulerManager.Start();

            var exitEvent = new ManualResetEvent(false);
            schedulerManager.OnCriticalError += (_, __) =>
            {
                exitEvent.Set();
            };
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };
            exitEvent.WaitOne();

            await schedulerManager.Stop();
        }
    }

    internal class SchedulerService : IWin32Service
    {
        public string ServiceName => "SystemChecker";
        private readonly ISchedulerManager _schedulerManager;
        private readonly ILogger _logger;
        public SchedulerService(IServiceProvider services)
        {
            _schedulerManager = services.GetRequiredService<ISchedulerManager>();
            _logger = services.GetRequiredService<ILogger<SchedulerService>>();
        }

        public void Start(string[] args, ServiceStoppedCallback serviceStoppedCallback)
        {
            _logger.LogInformation("Service starting");
            _schedulerManager.OnCriticalError += (_, __) =>
            {
                _schedulerManager.Stop().Wait();
                serviceStoppedCallback();
            };
            _schedulerManager.Start().Wait();
        }

        public void Stop()
        {
            _logger.LogInformation("Stop requested by Windows");
            _schedulerManager.Stop().Wait();
        }
    }
}
