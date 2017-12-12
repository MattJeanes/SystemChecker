using DatabaseMigrations;
using Microsoft.Extensions.Configuration;
using NDesk.Options;
using System;
using System.IO;

namespace SystemChecker.Migrations
{
    class Program
    {
        static IConfiguration _config = GetConfiguration();
        static int Main(string[] args)
        {
            var retVal = 0;

            var showHelp = false;
            var runLatest = false;
            var testMode = false;
            string user = null;
            string pass = null;
            var integrated = false;

            try
            {
                var p = new OptionSet()
                {
                    { "u|user|username=", "SQL username to authenticate with", x => user = x },
                    { "p|pass|password=", "SQL password to authenticate with", x => pass = x },
                    { "i|integrated", "Use integrated authentication", x => integrated = x != null },
                    { "l|latest|runLatest", "Run latest sprint on top of normal migrations", x => runLatest = x != null },
                    { "t|test|testMode", "Only print scripts to be run, does not actually run them", x => testMode = x != null },
                    { "h|help", "Shows help", x => showHelp = x != null }
                };
                p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid arguments");
                Console.WriteLine(e.Message);
                retVal = 1;
            }

            if (retVal == 0 && !showHelp)
            {
                var scriptsFolder = "Scripts";
                try
                {
                    var runner = new MigrationRunner(new MigrationOptions()
                    {

                        Login = new Login()
                        {
                            userID = user,
                            password = pass,
                            integrated = integrated
                        },
                        TestMode = testMode,
                        ScriptsFolder = scriptsFolder,
                        ConnStringResolver = ResolveConnectionString,
                        ApplicationName = _config["ApplicationName"]
                    });
                    var success = runner.RunMigrations(runLatest);
                    if (!success)
                    {
                        retVal = 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("FATAL ERROR");
                    Console.WriteLine(e.ToString());
                    retVal = 1;
                }
            }
#if DEBUG
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
#endif
            return retVal;
        }
        static string ResolveConnectionString(string database)
        {
            return _config.GetConnectionString(database);
        }

        static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }
    }
}
