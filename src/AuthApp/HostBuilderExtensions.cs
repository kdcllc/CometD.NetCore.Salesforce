using Bet.AspNetCore.Options;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using Console = Colorful.Console;

namespace AuthApp
{
    internal static class HostBuilderExtensions
    {
        internal static IHostBuilder CreateDefaultBuilder(HostBuilderOptions options)
        {
            var builder = new HostBuilder();

            builder.UseEnvironment(options.HostingEnviroment);

            var path = Directory.GetCurrentDirectory();
            builder.UseContentRoot(path);

            var configPath = Path.Combine(path, "appsettings.json");

            if (!string.IsNullOrWhiteSpace(options.ConfigFile))
            {
                configPath = Path.Combine(path, options.ConfigFile);
            }

            if (options.Verbose)
            {
                Console.WriteLine(configPath, color: Color.Green);
            }

            builder
                .ConfigureAppConfiguration((context, config) =>
                {
                    // appsettings file or others
                    config.AddJsonFile($"{Path.GetFileName(configPath).Split(".")[0]}.json", optional: true)
                          .AddJsonFile($"{Path.GetFileName(configPath).Split(".")[0]}.{options.HostingEnviroment}.json", optional: true);

                    // add secrets if specified
                    if (options.UserSecrets)
                    {
                        config.AddUserSecrets(ConsoleHandler.GetUserSecretsId());
                    }

                    // configure Azure Vault from the other settings.
                    var appAzureVaultUrl = config.Build().Bind<AzureVaultOptions>("AzureVault",enableValidation: false); ;

                    try
                    {
                        // use appsettings vault information
                        if (!string.IsNullOrWhiteSpace(appAzureVaultUrl.BaseUrl)
                            && string.IsNullOrWhiteSpace(options.AzureVault))
                        {
                            config.AddAzureKeyVault(hostingEnviromentName:options.HostingEnviroment, options.UseAzureKeyPrefix);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, ConsoleColor.Red);
                    }
                });

            builder
                .ConfigureLogging((_, configureBuilder) =>
                {
                    if (options.Verbose)
                    {
                        configureBuilder.AddConsole();
                        configureBuilder.AddDebug();
                    }
                });

            builder
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<HostStartupService>();

                    // disable hosting messages
                    services.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true);

                    if (options.Verbose)
                    {
                        services.AddLogging(x => x.AddFilter((_) => true));
                    }

                    services.AddSingleton(PhysicalConsole.Singleton);
                });

            return builder;
        }
    }
}
