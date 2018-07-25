using CometD.NetCore.Bayeux.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using TestApp.EventBus.Messages;
using TestApp.Services;

namespace TestApp
{
    public static class HostExtensions
    {
        public static IHostBuilder ConfigureHost(this IHostBuilder builder)
        {

            return builder.ConfigureServices((bulderContext, services) =>
            {
                services.AddLogging();
                services.AddHostedService<SalesforceEventBusHostedService>();

                services.AddTransient<IMessageListener,CustomMessageListener>();

                // Conjure up a RequestServices
                services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

                // Ensure object pooling is available everywhere.
                services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                // no need to create instance = services.AddStreamingClient();
                services.AddSalesforceEventBus();

            });
        }
    }
}
