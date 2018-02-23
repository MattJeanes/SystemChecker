using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using SystemChecker.Web.Helpers;
using Microsoft.Extensions.DependencyInjection;
using SystemChecker.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace SystemChecker.Web
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
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
                .UseStartup<Startup>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                hostBuilder.UseHttpSys(x =>
                {
                    x.Authentication.Schemes = AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                    x.Authentication.AllowAnonymous = true;
                });
            }
            else
            {
                hostBuilder.UseKestrel();
            }

            IWebHost host;
            try
            {
                host = hostBuilder.Build();
            }
            catch (Exception e)
            {
                var critLogger = new LoggerFactory()
                    .AddFile("logs/systemchecker-critical-{Date}.log")
                    .AddDebug()
                    .AddConsole()
                    .CreateLogger<Program>();

                critLogger.LogInformation($"Current directory: {Directory.GetCurrentDirectory()}");
                critLogger.LogError(e, "Critical failure in host build");
                Thread.Sleep(1000); // allow time to flush logs in the event of a crash
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
