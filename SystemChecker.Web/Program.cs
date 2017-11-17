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

namespace SystemChecker.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            bool isService = true;
            if (Debugger.IsAttached)
            {
                isService = false;
            }

            var app = new CommandLineApplication();
            var console = app.Option("-c|--console", "Launch in console mode", CommandOptionType.NoValue);
            var port = 5000;
            var portArgument = app.Option("-p|--port", "Port to host on", CommandOptionType.SingleValue);
            var help = app.Option("-? | -h | --help", "Show help information", CommandOptionType.NoValue);

            app.Execute(args);

            if (help.Value() != null)
            {
                app.ShowHelp();
                return 0;
            }

            if (console.Value() != null)
            {
                isService = false;
            }

            if (portArgument.Value() != null)
            {
                if(!int.TryParse(portArgument.Value(), out port))
                {
                    Console.WriteLine("Invalid port");
                    return 1;
                }
            }

            var pathToContentRoot = Directory.GetCurrentDirectory();
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(pathToContentRoot)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls($"http://localhost:{port}")
                .Build();

            if (isService)
            {
                host.RunAsSchedulerService();
            }
            else
            {
                var schedulerManager = host.Services.GetRequiredService<ISchedulerManager>();
                schedulerManager.Start();
                host.Run();
            }
            return 0;
        }
    }
}
