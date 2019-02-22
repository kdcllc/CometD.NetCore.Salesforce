using AuthApp.Host;
using Bet.AspNetCore.Options;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
namespace AuthApp
{
    [Command("get-tokens",
        Description = "Generates Salesforce Access and Refresh Tokens",
        ThrowOnUnexpectedArgument = false)]
    internal class TokenGeneratorCommand
    {
        [Option("--azure",
            Description = "Allows to specify Azure Vault Url. It overrides url specified in the appsetting.json file or any other configuration provider.")]
        public string AzureVault { get; set; }

        [Option("--azureprefix", Description = "Enables or disables Hosting Environment prefix to be used for Azure Key Vault. Default is true.")]
        public bool UseAzureKeyPrefix { get; set; }

        [Option("--configfile", Description = "Allows to specify a configuration file besides appsettings.json to be specified.")]
        public string ConfigFile { get; set; }

        [Option("--verbose", Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public bool Verbose { get; set; }

        [Option("--usesecrets", Description = "Enable UserSecrets.")]
        public bool UserSecrets { get; set; }

        [Option("--environment", Description = "Specify Hosting Environment Name for the cli tool execution.")]
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

            try
            {
                var builder = HostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                                .ConfigureServices((hostingContext, services) =>
                                {
                                    services.ConfigureWithDataAnnotationsValidation<SfConfig>("Salesforce");
                                    services.AddHostedService<HttpServer>();
                                });

                await builder.RunConsoleAsync();
                return 0;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
