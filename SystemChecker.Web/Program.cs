using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;

namespace SystemChecker.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var pathToContentRoot = Directory.GetCurrentDirectory();
            var hostBuilder = new WebHostBuilder()
                .UseContentRoot(pathToContentRoot)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    var loggingConfiguration = hostingContext.Configuration.GetSection("Logging");
                    logging.AddConfiguration(loggingConfiguration);
                    logging.AddConsole();
                    logging.AddDebug();
                    if (!env.IsDevelopment())
                    {
                        logging.AddFile(loggingConfiguration);
                    }
                })
                .UseStartup<Startup>()
                .UseKestrel()
                .UseIISIntegration();


            IWebHost host;
            try
            {
                host = hostBuilder.Build();
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

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                host.Run();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Fatal error");
                Thread.Sleep(1000); // allow time to flush logs in the event of a crash
                return 1;
            }

            return 0;
        }
    }
}
