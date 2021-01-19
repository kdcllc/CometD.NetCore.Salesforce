using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;

using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Extension;
using CometD.NetCore.Client.Transport;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCoreForce.Client.Models;

namespace CometD.NetCore.Salesforce
{
    public class ResilientStreamingClient : IStreamingClient
    {
        private readonly ILogger<ResilientStreamingClient> _logger;
        private readonly SalesforceConfiguration _options;
        private readonly AsyncExpiringLazy<AccessTokenResponse> _tokenResponse;

        // long polling duration
        private readonly int _readTimeOut = 120 * 1000;

        private BayeuxClient? _bayeuxClient = null;
        private bool _isDisposed = false;
        private ErrorExtension? _errorExtension;
        private LongPollingTransport? _clientTransport;
        private ReplayExtension? _replayIdExtension;

        public Action<int> InvalidReplayIdStrategy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientStreamingClient"/> class.
        /// </summary>
        /// <param name="tokenResponse"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ResilientStreamingClient(
            Func<AsyncExpiringLazy<AccessTokenResponse>> tokenResponse,
            IOptions<SalesforceConfiguration> options,
            ILogger<ResilientStreamingClient> logger)
        {
            _logger = logger;
            _options = options.Value;
            _tokenResponse = tokenResponse();

            CreateBayeuxClient();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ResilientStreamingClient"/> class.
        /// </summary>
        ~ResilientStreamingClient()
        {
            Dispose(false);
        }

        public event EventHandler<bool>? Reconnect;

        public bool IsConnected
        {
            get
            {
                if (_bayeuxClient == null)
                {
                    return false;
                }

                return _bayeuxClient.Connected;
            }
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            Disconnect(1000);
        }

        /// <inheritdoc/>
        public void Disconnect(int timeout)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot disconnect when disposed");
            }

            _bayeuxClient?.ResetSubscriptions();

            _logger.LogDebug("Disconnecting...");
            _bayeuxClient?.Disconnect();
            _bayeuxClient?.WaitFor(timeout, new[] { BayeuxClient.State.DISCONNECTED });

            if (_errorExtension != null)
            {
                _errorExtension.ConnectionError -= ErrorExtension_ConnectionError;
                _errorExtension.ConnectionException -= ErrorExtension_ConnectionException;
                _errorExtension.ConnectionMessage -= ErrorExtension_ConnectionMessage;
            }

            _logger.LogDebug("Disconnected...");
        }

        /// <summary>
        /// The Handshake uses the default value of 1000 ms.
        /// </summary>
        public void Handshake()
        {
            Handshake(1000);
        }

        /// <inheritdoc/>
        public void Handshake(int timeout)
        {
            if (_bayeuxClient == null)
            {
                return;
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot connect when disposed");
            }

            _logger.LogDebug("Handshaking...");
            _bayeuxClient.Handshake();
            _bayeuxClient.WaitFor(timeout, new[] { BayeuxClient.State.CONNECTED });
            _logger.LogDebug("Connected");
        }

        /// <inheritdoc/>
        public void SubscribeTopic(
            string topicName,
            IMessageListener listener,
            long replayId = -1)
        {
            if (topicName == null || (topicName = topicName.Trim()).Length == 0)
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            var channel = _bayeuxClient?.GetChannel(topicName, replayId);
            channel?.Subscribe(listener);
        }

        /// <inheritdoc/>
        public bool UnsubscribeTopic(
            string topicName,
            IMessageListener? listener = null,
            long replayId = -1)
        {
            if (topicName == null || (topicName = topicName.Trim()).Length == 0)
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            var channel = _bayeuxClient?.GetChannel(topicName, replayId);
            if (channel != null)
            {
                if (listener != null)
                {
                    channel.Unsubscribe(listener);
                }
                else
                {
                    channel.Unsubscribe();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Disposing of the resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing of the resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                Disconnect();
                _isDisposed = true;
            }
        }

        private void CreateBayeuxClient()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot create connection when disposed");
            }

            _logger.LogDebug("Creating {name} ...", nameof(BayeuxClient));

            var accessToken = _tokenResponse.Value().Result;

            // only need the scheme and host, strip out the rest
            var serverUri = new Uri(accessToken.InstanceUrl);
            var endpoint = $"{serverUri.Scheme}://{serverUri.Host}{_options.CometDUri}";

            var headers = new NameValueCollection { { nameof(HttpRequestHeader.Authorization), $"OAuth {accessToken.AccessToken}" } };

            // Salesforce socket timeout during connection(CometD session) = 110 seconds
            var options = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { ClientTransport.TIMEOUT_OPTION, _options.ReadTimeOut ?? _readTimeOut },
                { ClientTransport.MAX_NETWORK_DELAY_OPTION, _options.ReadTimeOut ?? _readTimeOut }
            };

            _clientTransport = new LongPollingTransport(options, headers);

            _bayeuxClient = new BayeuxClient(endpoint, _clientTransport);

            // adds logging and also raises an event to process reconnection to the server.
            _errorExtension = new ErrorExtension();
            _errorExtension.ConnectionError += ErrorExtension_ConnectionError;
            _errorExtension.ConnectionException += ErrorExtension_ConnectionException;
            _errorExtension.ConnectionMessage += ErrorExtension_ConnectionMessage;
            _bayeuxClient.AddExtension(_errorExtension);

            _replayIdExtension = new ReplayExtension();
            _bayeuxClient.AddExtension(_replayIdExtension);

            _logger.LogDebug("{name} was created...", nameof(BayeuxClient));
        }

        private void ErrorExtension_ConnectionError(
            object sender,
            string e)
        {
            // authentication failure
            if (string.Equals(e, "403::Handshake denied", StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, "403:denied_by_security_policy:create_denied", StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, "403::unknown client", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Handled CometD Exception: {message}", e);

                // 1. Disconnect existing client.
                Disconnect();

                // 2. Invalidate the access token.
                _tokenResponse.Invalidate();

                _logger.LogDebug("Invalidate token for {name} ...", nameof(BayeuxClient));

                // 3. Recreate BayeuxClient and populate it with a new transport with new security headers.
                CreateBayeuxClient();

                // 4. Invoke the Reconnect Event
                Reconnect?.Invoke(this, true);
            }
            else if (e.Contains("you provided was invalid"))
            {
                var start = e.IndexOf('{');
                var end = e.IndexOf('}');
                var replayIdString = e.Substring(start + 1, end - (start + 1));

                var success = int.TryParse(replayIdString, out var replayId);

                if (success)
                {
                    InvalidReplayIdStrategy(replayId);
                }
            }
            else
            {
                _logger.LogError("{name} failed with the following message: {message}", nameof(ResilientStreamingClient), e);
            }
        }

        private void ErrorExtension_ConnectionException(
            object sender,
            Exception ex)
        {
            // ongoing time out issue not to be considered as error in the log.
            if (ex?.Message == "The operation has timed out.")
            {
                _logger.LogDebug(ex.Message);
            }
            else if (ex != null)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private void ErrorExtension_ConnectionMessage(
            object sender,
            string meaage)
        {
            _logger.LogDebug(meaage);
        }
    }
}
