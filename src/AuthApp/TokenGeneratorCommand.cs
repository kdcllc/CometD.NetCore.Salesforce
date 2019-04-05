using AuthApp.Host;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

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

        /// <summary>
        /// Property types of ValueTuple{bool,T} translate to CommandOptionType.SingleOrNoValue.
        /// Input                   | Value
        /// ------------------------|--------------------------------
        /// (none)                  | (false, default(TraceLevel))
        /// --verbose               | (true, LogLevel.Information)
        /// --verbose:information   | (true, LogLevel.Information)
        /// --verbose:debug         | (true, LogLevel.Debug)
        /// --verbose:trace         | (true, LogLevel.Trace)
        /// </summary>
        [Option(Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public (bool HasValue, LogLevel level) Verbose { get; }

        [Option("--usesecrets", Description = "Enable UserSecrets.")]
        public bool UserSecrets { get; set; }

        [Option("--environment", Description = "Specify Hosting Environment Name for the cli tool execution.")]
        public string HostingEnviroment { get; set; }

        [Option("--section", Description ="Configuration Section Name to retrieve the options. The Default value is Salesforce.")]
        public string SectionName { get; set; }

        private async Task<int> OnExecuteAsync()
        {
            var builderConfig = new HostBuilderOptions
            {
                AzureVault = AzureVault,
                UseAzureKeyPrefix = !UseAzureKeyPrefix,
                ConfigFile = ConfigFile,
                Verbose = Verbose.HasValue,
                Level = Verbose.level,
                UserSecrets = UserSecrets,
                HostingEnviroment = !string.IsNullOrWhiteSpace(HostingEnviroment) ? HostingEnviroment : "Development"
            };

            try
            {
                var builder = HostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                                .ConfigureServices((hostingContext, services) =>
                                {
                                    var configSection = string.IsNullOrWhiteSpace(SectionName) ? "Salesforce" : SectionName;

                                    services.ConfigureWithDataAnnotationsValidation<SfConfig>(hostingContext.Configuration, configSection);
                                    services.AddHostedService<HttpServer>();
                                });

                await builder.RunConsoleAsync();

                return 0;
            }
            catch(Microsoft.Extensions.Options.OptionsValidationException)
            {
                Console.WriteLine("Not all of the required configurations has been provided.", Color.Red);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.Red);
            }

            return 0;
        }
    }
}
