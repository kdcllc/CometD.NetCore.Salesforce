using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthApp.Host
{
    public class CustomHost
    {
        private IHost _host;

        public CustomHost()
        {
            _host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddJsonFile("hostsettings.json", optional: true);
                     configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                 })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);

                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = new SfConfig();
                    hostContext.Configuration.Bind("Salesforce", config);
                    services.AddSingleton(config);

                    services.AddHostedService<HttpServer>();
                })
                .UseConsoleLifetime()
                .Build();
        }

        public void Start()
        {            
            _host.Start();
        }

        public async Task StopAsync()
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }
    }
}
