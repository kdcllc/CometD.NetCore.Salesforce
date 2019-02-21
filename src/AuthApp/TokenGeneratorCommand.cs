using AuthApp.Host;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace AuthApp
{
    [Command("refresh-token", Description ="Generates Salesforce Refresh Token", ThrowOnUnexpectedArgument = false)]
    internal class TokenGeneratorCommand
    {
        [Option("-az|--azure", Description = "Allows to specify Azure Vault Url. It overrides url specified in the appsetting.json file or any other configuration provider.")]
        public string AzureVault { get; set; }

        [Option("-azp|--azureprefix", Description = "Enables or disables Hosting Environment prefix to be used for Azure Key Vault. Default is true.")]
        public bool UseAzureKeyPrefix { get; set; }

        [Option("-c|--config", Description = "Allows to specify a configuration file besides appsettings.json to be specified.")]
        public string ConfigFile { get; set; }

        [Option("-v|--verbose", Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public bool Verbose { get; set; }

        [Option("-s|--usersecrets", Description = "Enable UserSecrets.")]
        public bool UserSecrets { get; set; }

        [Option("-e|--env", Description = "Specify Hosting Enviroment Name for the cli tool execution.")]
        public string HostingEnviroment { get; set; }

        private async Task<int> OnExecuteAsync()
        {
            var builderConfig = new HostBuilderOptions
            {
                AzureVault = AzureVault,
                UseAzureKeyPrefix = !UseAzureKeyPrefix,
                ConfigFile = ConfigFile,
                Verbose = Verbose,
                UserSecrets = UserSecrets,
                HostingEnviroment = !string.IsNullOrWhiteSpace(HostingEnviroment) ? HostingEnviroment : "Development"
            };

            var host = HostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                .ConfigureServices((hostingContext,services) =>
                {
                    services.ConfigureWithDataAnnotationsValidation<SfConfig>("Salesforce");
                    services.AddHostedService<HttpServer>();
                })
                .Build();

            var hostedServices = host.Services;

            using (host)
            {
                try
                {
                    await host.StartAsync();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString(), Color.Red);
                    return 1;
                }

                await host.StopAsync();
                return 0;
            }
        }
    }
}
