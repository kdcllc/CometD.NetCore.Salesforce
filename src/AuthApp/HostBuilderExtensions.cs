using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Bet.AspNetCore.Options;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Console = Colorful.Console;

namespace AuthApp
{
    internal static class HostBuilderExtensions
    {
        internal static IHostBuilder CreateDefaultBuilder(HostBuilderOptions options)
        {
            var builder = new HostBuilder();

            builder.UseEnvironment(options.HostingEnviroment);
            var fullPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(options.ConfigFile)))
            {
                fullPath = Path.GetDirectoryName(options.ConfigFile);
            }

            builder.UseContentRoot(fullPath);

            var defaultConfigName = !string.IsNullOrWhiteSpace(options.ConfigFile) ? Path.GetFileName(options.ConfigFile) : "appsettings.json";

            if (options.Verbose)
            {
                Console.WriteLine($"ContentRoot:{fullPath}", color: Color.Green);
            }

            builder
                .UseOptionValidation()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // appsettings file or others
                    config.AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.json"), optional: true)
                          .AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.{options.HostingEnviroment}.json"), optional: true);

                    // add secrets if specified
                    if (options.UserSecrets)
                    {
                        config.AddUserSecrets(ConsoleHandler.GetUserSecretsId());
                    }

                    // configure Azure Vault from the other settings.
                    var appAzureVaultUrl = config.Build().Bind<AzureVaultOptions>("AzureVault", enableValidation: false);

                    // build azure key vault from passed in parameter
                    if (!string.IsNullOrWhiteSpace(options.AzureVault))
                    {
                        var dic = new Dictionary<string, string>
                            {
                                { "AzureVault:BaseUrl", options.AzureVault }
                            };

                        config.AddInMemoryCollection(dic);
                    }

                    // use appsettings vault information
                    if (!string.IsNullOrWhiteSpace(appAzureVaultUrl.BaseUrl)
                        || !string.IsNullOrWhiteSpace(options.AzureVault))
                    {
                        config.AddAzureKeyVault(hostingEnviromentName: options.HostingEnviroment, options.UseAzureKeyPrefix);
                    }

                    if (!string.IsNullOrWhiteSpace(options.Settings.ClientId)
                        && !string.IsNullOrWhiteSpace(options.Settings.ClientSecret))
                    {
                        var inputValues = new Dictionary<string, string>
                            {
                                { $"{options.SectionName}:ClientId", options.Settings.ClientId },
                                { $"{options.SectionName}:ClientSecret", options.Settings.ClientSecret },
                                { $"{options.SectionName}:LoginUrl", options.Settings.LoginUrl },
                                { $"{options.SectionName}:OAuthUri", options.Settings.OAuthUri },
                                { $"{options.SectionName}:OAuthorizeUri", options.Settings.OAuthorizeUri },
                            };

                        config.AddInMemoryCollection(inputValues);
                    }

                    if ((options.Verbose && options.Level == LogLevel.Debug)
                        || options.Level == LogLevel.Trace)
                    {
                        config.Build().DebugConfigurations();
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
                    services.AddSingleton(options);

                    services.AddHostedService<HostStartupService>();

                    // disable hosting messages
                    services.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true);

                    if (options.Verbose)
                    {
                        services.AddLogging(x => x.AddFilter((loglevel) =>
                        {
                            return loglevel == options.Level;
                        }));
                    }

                    services.AddSingleton(PhysicalConsole.Singleton);
                });

            return builder;
        }
    }
}
