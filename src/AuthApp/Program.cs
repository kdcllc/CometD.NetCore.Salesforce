using System;
using System.IO;
using System.Threading.Tasks;
using AuthApp.Host;
using Microsoft.Extensions.Configuration;

namespace AuthApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new CustomHost();
            
            Console.WriteLine("Starting!");
            host.Start();
            Console.WriteLine("Started!");
            Console.ReadLine();

            Console.WriteLine("Stopping!");
            await host.StopAsync();
            Console.WriteLine("Stopped!");
        }

    }
}
