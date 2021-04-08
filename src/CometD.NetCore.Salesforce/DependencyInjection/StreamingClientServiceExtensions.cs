using System;
using System.Threading;

using CometD.NetCore.Salesforce;
using CometD.NetCore.Salesforce.Resilience;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using NetCoreForce.Client;
using NetCoreForce.Client.Models;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An extension method for <see cref="CometD.NetCore.Salesforce"/>.
    /// </summary>
    public static class StreamingClientServiceExtensions
    {
        /// <summary>
        /// Add custom implementation for <see cref="IStreamingClient"/>.
        /// There can be only one implementation registered with DI at any time.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name for the root options configuration. The default value is Salesfoce.</param>
        /// <param name="optionName"></param>
        /// <param name="configureOptions">The option configuration that can be override the configuration provides.</param>
        /// <returns></returns>
        public static IServiceCollection AddResilientStreamingClient<T>(
            this IServiceCollection services,
            string sectionName = "Salesforce",
            string optionName = "",
            Action<SalesforceConfiguration, IServiceProvider>? configureOptions = default)
            where T : IStreamingClient
        {
            services.AddChangeTokenOptions<SalesforceConfiguration>(
                sectionName,
                optionName: optionName,
                configureAction: (o, sp) => configureOptions?.Invoke(o, sp));

            services.AddResilentForceClient(optionName);

            services.TryAdd(new ServiceDescriptor(typeof(IStreamingClient), typeof(T), ServiceLifetime.Singleton));

            return services;
        }

        /// <summary>
        /// Add <see cref="IStreamingClient"/> to DI.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name for the root options configuration. The default value is Salesfoce.</param>
        /// <param name="optionName"></param>
        /// <param name="configureOptions">The option configuration that can be override the configuration provides.</param>
        /// <returns></returns>
        public static IServiceCollection AddResilientStreamingClient(
            this IServiceCollection services,
            string sectionName = "Salesforce",
            string optionName = "",
            Action<SalesforceConfiguration, IServiceProvider>? configureOptions = default)
        {
            services.AddChangeTokenOptions<SalesforceConfiguration>(
                sectionName,
                optionName: optionName,
                configureAction: (o, sp) => configureOptions?.Invoke(o, sp));

            services.AddResilentForceClient(optionName);

            services.TryAddSingleton<IStreamingClient, ResilientStreamingClient>();

            return services;
        }

        /// <summary>
        /// Adds ForecClient Resilient version of it with Refresh Token Authentication.
        /// <see cref="!:https://help.salesforce.com/articleView?id=remoteaccess_oauth_refresh_token_flow.htm%26type%3D5"/>
        /// Can be used in the code with <see cref="IResilientForceClient"/>.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name for the root options configuration. The default value is Salesfoce.</param>
        /// <param name="optionName"></param>
        /// <param name="configureOptions">The option configuration that can be override the configuration provides.</param>
        /// <returns></returns>
        public static IServiceCollection AddResilientStreamingClient(
            this IServiceCollection services,
            string sectionName = "Salesforce",
            string optionName = "",
            Action<SalesforceConfiguration>? configureOptions = default)
        {
            services.AddChangeTokenOptions<SalesforceConfiguration>(
                sectionName,
                optionName: optionName,
                configureAction: x => configureOptions?.Invoke(x));

            services.AddResilentForceClient(optionName);

            services.TryAddSingleton<IStreamingClient, ResilientStreamingClient>();

            return services;
        }

        private static IServiceCollection AddResilentForceClient(this IServiceCollection services, string optionName)
        {
            services.TryAddSingleton<IResilientForceClient, ResilientForceClient>();

            services.TryAddSingleton<Func<AsyncExpiringLazy<AccessTokenResponse>>>(sp => () =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<SalesforceConfiguration>>().Get(optionName);

                if (!TimeSpan.TryParse(options.TokenExpiration, out var tokenExpiration))
                {
                    tokenExpiration = TimeSpan.FromHours(1);
                }

                return new AsyncExpiringLazy<AccessTokenResponse>(async data =>
                {
                    if (data.Result == null
                        || DateTime.UtcNow > data.ValidUntil.Subtract(TimeSpan.FromSeconds(30)))
                    {
                        var policy = Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(
                                    retryCount: options.Retry,
                                    sleepDurationProvider: (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, retryAttempt)));

                        var authClient = await policy.ExecuteAsync(async () =>
                        {
                            var auth = new AuthenticationClient();

                            await auth.TokenRefreshAsync(
                                options.RefreshToken,
                                options.ClientId,
                                options.ClientSecret,
                                $"{options.LoginUrl}{options.OAuthUri}");

                            return auth;
                        });

                        return new AsyncExpirationValue<AccessTokenResponse>
                        {
                            Result = authClient.AccessInfo,
                            ValidUntil = DateTimeOffset.UtcNow.Add(tokenExpiration)
                        };
                    }

                    return data;
                });
            });

            services.AddSingleton<Func<AsyncExpiringLazy<ForceClient>>>(sp => () =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<SalesforceConfiguration>>().Get(optionName);

                if (!TimeSpan.TryParse(options.TokenExpiration, out var tokenExpiration))
                {
                    tokenExpiration = TimeSpan.FromHours(1);
                }

                return new AsyncExpiringLazy<ForceClient>(async data =>
                {
                    if (data.Result == null
                        || DateTime.UtcNow > data.ValidUntil.Subtract(TimeSpan.FromSeconds(30)))
                    {
                        var policy = Policy
                                  .Handle<Exception>()
                                  .WaitAndRetryAsync(
                                      retryCount: options.Retry,
                                      sleepDurationProvider: (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, retryAttempt)));

                        var client = await policy.ExecuteAsync(async () =>
                        {
                            var authClient = new AuthenticationClient();

                            await authClient.TokenRefreshAsync(
                                options.RefreshToken,
                                options.ClientId,
                                options.ClientSecret,
                                $"{options.LoginUrl}{options.OAuthUri}");

                            return new ForceClient(
                                authClient.AccessInfo.InstanceUrl,
                                authClient.ApiVersion,
                                authClient.AccessInfo.AccessToken);
                        });

                        return new AsyncExpirationValue<ForceClient>
                        {
                            Result = client,
                            ValidUntil = DateTimeOffset.UtcNow.Add(tokenExpiration)
                        };
                    }

                    return data;
                });
            });

            return services;
        }
    }
}
