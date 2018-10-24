using System;
using System.Threading.Tasks;
using SystemChecker.Client;

namespace SystemChecker.TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter API key:");
            var apiKey = Console.ReadLine();
            var client = new SystemCheckerClient("https://localhost:44394/api", apiKey);

            var checks = await client.GetAllAsync();
            Console.WriteLine(checks);

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
