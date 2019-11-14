using System;
using System.IO;
using System.Threading.Tasks;

using CometD.NetCore.Salesforce.Resilience;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestApp
{
#pragma warning disable RCS1102 // Make class static.
    internal sealed class Program
#pragma warning restore RCS1102 // Make class static.
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddJsonFile("hostsettings.json", optional: true);
                     configHost.AddCommandLine(args);
                 })
                 .ConfigureAppConfiguration((hostContext, configBuilder) =>
                 {
                     configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                     configBuilder.AddJsonFile(
                         $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                         optional: true);

                     configBuilder.AddAzureKeyVault(hostingEnviromentName: hostContext.HostingEnvironment.EnvironmentName);
                     configBuilder.AddCommandLine(args);

                     // print out the environment
                     var config = configBuilder.Build();
                     config.DebugConfigurations();
                 })
                 .ConfigureServices((context, services) =>
                 {
                     services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

                     services.AddResilientStreamingClient(context.Configuration);
                 })
                 .ConfigureLogging((hostContext, configLogging) =>
                 {
                     configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                     configLogging.AddConsole();
                     configLogging.AddDebug();
                 })
                 .UseConsoleLifetime()
                 .Build();

            var srv = host.Services;

            await host.StartAsync();

            var test = srv.GetRequiredService<IResilientForceClient>();

            var result = await test.CountQueryAsync("SELECT COUNT() FROM ACCOUNT");

            Console.WriteLine(result);

            await Task.Delay(TimeSpan.FromSeconds(15));

            var result1 = await test.CountQueryAsync("SELECT COUNT() FROM ACCOUNT");
            Console.WriteLine(result1);

            await host.StopAsync();
        }
    }
}
