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

namespace SystemChecker.Web
{
    public class Program
    {
        public static int Main(string[] args)
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
                var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
                var path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
                Directory.SetCurrentDirectory(path);
            }

            var hostBuilder = new WebHostBuilder()
                .UseContentRoot(pathToContentRoot)
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

            if (isService)
            {
                host.RunAsSchedulerService();
            }
            else
            {
                if (noScheduler.Value() == null)
                {
                    var schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
                    schedulerManager.Start();
                }
                host.Run();
            }
            return 0;
        }
    }
}
