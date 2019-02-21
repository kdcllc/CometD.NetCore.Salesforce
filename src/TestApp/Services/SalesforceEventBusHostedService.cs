using System.Threading;
using System.Threading.Tasks;
using CometD.NetCore.Salesforce;
using Bet.BuildingBlocks.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestApp.EventBus.Messages;

namespace TestApp.Services
{
    /// <summary>
    /// Provides with LifetimeEventsHostedService
    /// </summary>
    internal class SalesforceEventBusHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IApplicationLifetime _appLifetime;
        private readonly SalesforceConfiguration _options;
        private readonly IEventBus _eventBus;

        public SalesforceEventBusHostedService(
            ILogger<SalesforceEventBusHostedService> logger,
            IApplicationLifetime appLifetime,
            SalesforceConfiguration options,
            IEventBus eventBus)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _options = options;
            _eventBus = eventBus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            _logger.LogInformation("StartAsync has been called.");

            await _eventBus.Subscribe<CustomMessageListener>(
               new PlatformEvent() { Name = _options.CustomEvent, ReplayId= _options.ReplayId });

           //return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync has been called.");

            await _eventBus.Unsubscribe<CustomMessageListener>(
                new PlatformEvent() { Name = _options.CustomEvent, ReplayId=_options.ReplayId});
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");

            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}
