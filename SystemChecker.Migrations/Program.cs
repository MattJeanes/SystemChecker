using DbUp;
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
            var scriptsFolder = "Scripts";
            var database = "SystemChecker";
            try
            {
                var upgrader = DeployChanges.To
                    .SqlDatabase(_config.GetConnectionString(database))
                    .JournalToSqlTable("dbo", "tblVersionInfo")
                    .WithExecutionTimeout(TimeSpan.FromSeconds(120))
                    .WithScriptsFromFileSystem(scriptsFolder)
                    .WithTransaction()
                    .LogScriptOutput()
                    .LogToConsole();

                var result = upgrader.Build().PerformUpgrade();

                if (!result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.ResetColor();
                    retVal = 1;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success!");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("FATAL ERROR");
                Console.WriteLine(e.ToString());
                Console.ResetColor();
                retVal = 1;
            }
#if DEBUG
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
#endif
            return retVal;
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
