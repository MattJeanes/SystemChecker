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

namespace SystemChecker.Web
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var isService = false;
            var app = new CommandLineApplication();
            var service = app.Option("-s|--service", "Launch in service mode", CommandOptionType.NoValue);
            int? port = null;
            var portArgument = app.Option("-p|--port", "Port to host on", CommandOptionType.SingleValue);
            var help = app.Option("-? | -h | --help", "Show help information", CommandOptionType.NoValue);
            var noScheduler = app.Option("-n|--no-scheduler", "Launch without scheduler running", CommandOptionType.NoValue);

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

            if (portArgument.Value() != null)
            {
                if (!int.TryParse(portArgument.Value(), out var portTmp))
                {
                    Console.WriteLine("Invalid port");
                    return 1;
                }
                else
                {
                    port = portTmp;
                    Console.WriteLine($"Using port {port}");
                }
            }

            var pathToContentRoot = Directory.GetCurrentDirectory();
            if (isService)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            }

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

            if (port.HasValue)
            {
                hostBuilder.UseUrls($"http://localhost:{port}");
            }

            var host = hostBuilder.Build();
            try
            {
                if (isService)
                {
                    host.RunAsSchedulerService();
                }
                else
                {
                    if (noScheduler.Value() == null)
                    {
                        var schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
                        await schedulerManager.Start();
                    }
                    host.Run();
                }
            }
            finally
            {
                host.Dispose();
            }

            return 0;
        }
    }
}
