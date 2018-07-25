using CometD.NetCore.Bayeux;
using CometD.NetCore.Bayeux.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TestApp.EventBus.Messages
{
    /// <summary>
    /// The <see cref="CustomMessageListener"/> implements <see cref="IMessageListener"/>
    /// </summary>
    public class CustomMessageListener : IMessageListener
    {
        private readonly ILogger<CustomMessageListener> _logger;

        /// <summary>
        /// Constructor for <see cref="CustomMessageListener"/>.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{CustomMessageListener}"/>.</param>
        public CustomMessageListener(ILogger<CustomMessageListener> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Receives salesforce message from Platform Event
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void OnMessage(IClientSessionChannel channel, IMessage message)
        {
            var msg = JsonConvert.DeserializeObject<CustomMessageEnvelope>(message.Json);

            _logger.LogDebug($"{nameof(CustomMessageListener)} payload: {message.Json}");

            var custName = msg.Data.Payload.CustomerName;
            var replayId = msg.Data.Event.ReplayId;

            _logger.LogDebug($"Customer Name: {custName} - ReplayId: {replayId}");
        }
    }
}
