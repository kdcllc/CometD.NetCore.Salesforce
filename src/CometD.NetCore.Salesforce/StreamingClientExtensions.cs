using CometD.NetCore.Salesforce;
using CometD.NetCore.Salesforce.ForceClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An extension method for <see cref="CometD.NetCore.Salesforce"/>.
    /// </summary>
    public static class StreamingClientExtensions
    {
        /// <summary>
        /// An Extension method to add <see cref="StreamingClient"/> dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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
