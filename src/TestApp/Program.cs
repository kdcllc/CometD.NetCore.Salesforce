using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddJsonFile("hostsettings.json", optional: true);
                     configHost.AddEnvironmentVariables(prefix: "TESTAPP_");
                     configHost.AddCommandLine(args);
                 })
                 .ConfigureAppConfiguration((hostContext, configBuilder) =>
                 {
                     configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                     configBuilder.AddJsonFile(
                         $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                         optional: true);

                     configBuilder.AddAzureKeyVault(hostingEnviromentName:hostContext.HostingEnvironment.EnvironmentName);

                     configBuilder.AddEnvironmentVariables(prefix: "TESTAPP_");
                     configBuilder.AddCommandLine(args);

                     // print out the environment
                     var config = configBuilder.Build();
                     config.DebugConfigurations();
                 })
                 .ConfigureHost()
                 .ConfigureLogging((hostContext, configLogging) =>
                 {
                     configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                     configLogging.AddConsole();
                     configLogging.AddDebug();
                 })
                 .UseConsoleLifetime()
                 .Build();

            var srv = host.Services;

            await host.RunAsync();
        }
    }
}
