using System;
using System.Threading;

using CometD.NetCore.Salesforce;
using CometD.NetCore.Salesforce.ForceClient;
using CometD.NetCore.Salesforce.Resilience;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCoreForce.Client;
using NetCoreForce.Client.Models;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An extension method for <see cref="CometD.NetCore.Salesforce"/>.
    /// </summary>
    public static class StreamingClientExtensions
    {
        /// <summary>
        /// Adds ForecClient Resilient version of it with Refresh Token Authentication.
        /// <see cref="!:https://help.salesforce.com/articleView?id=remoteaccess_oauth_refresh_token_flow.htm%26type%3D5"/>
        /// Can be used in the code with <see cref="IResilientForceClient"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IServiceCollection AddResilientStreamingClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "Salesforce")
        {
            services.Configure<SalesforceConfiguration>(configuration.GetSection(sectionName));

            services.AddSingleton<IResilientForceClient, ResilientForceClient>();

            services.AddSingleton<Func<AsyncExpiringLazy<AccessTokenResponse>>>(sp => () =>
            {
                var options = sp.GetRequiredService<IOptions<SalesforceConfiguration>>().Value;

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
                var options = sp.GetRequiredService<IOptions<SalesforceConfiguration>>().Value;

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

            services.AddSingleton<IStreamingClient, ResilientStreamingClient>();

            return services;
        }

        /// <summary>
        /// An Extension method to add <see cref="StreamingClient"/> dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        [Obsolete("Use " + nameof(AddResilientStreamingClient) + "extension method instead.")]
        public static IServiceCollection AddStreamingClient(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var salesforceConfig = new SalesforceConfiguration();
                var config = sp.GetRequiredService<IConfiguration>();

                config.Bind("Salesforce", salesforceConfig);

                return salesforceConfig;
            });

            services.AddSingleton<IAuthenticationClientProxy, AuthenticationClientProxy>();
            services.AddSingleton<IForceClientProxy, ForceClientProxy>();
            services.AddSingleton<IStreamingClient, StreamingClient>();

            return services;
        }
    }
}
