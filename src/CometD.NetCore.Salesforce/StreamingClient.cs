using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Extension;
using CometD.NetCore.Client.Transport;
using CometD.NetCore.Salesforce.ForceClient;
using Microsoft.Extensions.Logging;
using NetCoreForce.Client.Models;

namespace CometD.NetCore.Salesforce
{
    /// <summary>
    /// CometD implementation of <see cref="IStreamingClient"/>.
    /// </summary>
    public class StreamingClient : IStreamingClient
    {
        private AccessTokenResponse _tokenInfo;

        private BayeuxClient _bayeuxClient = null;
        private ErrorExtension _errorExtension;
        private LongPollingTransport _clientTransport;

        private readonly ILogger<StreamingClient> _logger;
        private readonly IAuthenticationClientProxy _authenticationClient;
        private readonly SalesforceConfiguration _options;

        // long polling duration
        private const int ReadTimeOut = 120 * 1000;

        ///<inheritdoc/>
        public event EventHandler<bool> Reconnect;

        /// <summary>
        /// Constructor <see cref="StreamingClient"/> creates instance of the class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticationClient"></param>
        /// <param name="options"></param>
        public StreamingClient(
            ILogger<StreamingClient> logger,
            IAuthenticationClientProxy authenticationClient,
            SalesforceConfiguration options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _authenticationClient = authenticationClient ?? throw new ArgumentNullException(nameof(authenticationClient));

            InitBayeuxClient();
        }

        ///<inheritdoc/>
        public bool IsConnected => _bayeuxClient.Connected;

        ///<inheritdoc/>
        public void Handshake()
        {
            Handshake(1000);
        }

        ///<inheritdoc/>
        public void Handshake(int timeout)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot connect when disposed");
            }

            _logger.LogDebug("Handshaking...");
            _bayeuxClient.Handshake();
            _bayeuxClient.WaitFor(timeout, new[] { BayeuxClient.State.CONNECTED });
            _logger.LogDebug("Connected");
        }

        ///<inheritdoc/>
        public void SubscribeTopic(string topicName, IMessageListener listener, long replayId=-1)
        {
            #region ArgumentNullExeption
            if (null == topicName || (topicName = topicName.Trim()).Length == 0)
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            if (null == listener)
            {
                throw new ArgumentNullException(nameof(listener));
            }
            #endregion

            var channel = _bayeuxClient.GetChannel(topicName,replayId);
            channel?.Subscribe(listener);
        }

        ///<inheritdoc/>
        public bool UnsubscribeTopic(string topicName, IMessageListener listener = null, long replayId=-1)
        {
            if (null == topicName || (topicName = topicName.Trim()).Length == 0)
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            var channel = _bayeuxClient.GetChannel(topicName, replayId);
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

        ///<inheritdoc/>
        public void Disconnect()
        {
            Disconnect(1000);
        }

        ///<inheritdoc/>
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

            _errorExtension.ConnectionError -= ErrorExtension_ConnectionError;
            _errorExtension.ConnectionException -= ErrorExtension_ConnectionException;
            _errorExtension.ConnectionMessage -= ErrorExtension_ConnectionMessage;

            _logger.LogDebug("Disconnected...");
        }

        /// <summary>
        /// This refreshes the session for the user.
        /// </summary>
        private void Reauthenticate()
        {
            _authenticationClient.Authenticate().Wait();
            _logger.LogDebug("Authentication Successful!");
        }

        private void InitBayeuxClient()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot create connection when disposed");
            }

            if (!_authenticationClient.IsAuthenticated)
            {
                Reauthenticate();
            }

            _tokenInfo = _authenticationClient.AuthenticationClient.AccessInfo;

            // Salesforce socket timeout during connection(CometD session) = 110 seconds
            IDictionary<string, object> options = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {ClientTransport.TIMEOUT_OPTION, _options.ReadTimeOut ?? ReadTimeOut },
                {ClientTransport.MAX_NETWORK_DELAY_OPTION, 120000 }
            };

            var headers = new NameValueCollection { { HttpRequestHeader.Authorization.ToString(), $"OAuth {_tokenInfo.AccessToken}" } };

            _clientTransport = new LongPollingTransport(options, headers);

            // only need the scheme and host, strip out the rest
            var serverUri = new Uri(_tokenInfo.InstanceUrl);
            var endpoint = $"{serverUri.Scheme}://{serverUri.Host}{_options.CometDUri}";

            _bayeuxClient = new BayeuxClient(endpoint, _clientTransport);

            // adds logging and also raises an event to process reconnection to the server.
            _errorExtension = new ErrorExtension();
            _errorExtension.ConnectionError += ErrorExtension_ConnectionError;
            _errorExtension.ConnectionException += ErrorExtension_ConnectionException;
            _errorExtension.ConnectionMessage += ErrorExtension_ConnectionMessage;
            _bayeuxClient.AddExtension(_errorExtension);

            _replayIdExtension = new ReplayExtension();
            _bayeuxClient.AddExtension(_replayIdExtension);
        }

        private void ErrorExtension_ConnectionMessage(object sender, string meaage)
        {
            _logger.LogDebug(meaage);
        }

        private void ErrorExtension_ConnectionException(object sender, Exception ex)
        {
            // ongoing time out issue not to be considered as error in the log.
            if (ex?.Message == "The operation has timed out.")
            {
                _logger.LogDebug(ex.Message);
            }
            else
            {
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Salesforce disconnects at random the client and the client must be re-connected and also re-subscribed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void ErrorExtension_ConnectionError(object sender, string message)
        {
            // authentication failure
            if (message.ToLower() == "403::Handshake denied" ||
                message.ToLower() == "403:denied_by_security_policy:create_denied" ||
                message.ToLower() == "403::unknown client"
                )
            {
                _logger.LogError($"Handled CometD Exception: {message}");
                _logger.LogDebug($"Re-authenticating BayeuxClient...");
                // 1. Disconnect
                Disconnect();
                _logger.LogDebug($"Disconnecting {nameof(BayeuxClient)}...");
                // 2. try (x) times to re-authenticate
                Reauthenticate();
                _logger.LogDebug($"Re-authenticating {nameof(BayeuxClient)}...");
                // 3. populate a new transport with new security headers.
                InitBayeuxClient();
                _logger.LogDebug($"Re-Init {nameof(BayeuxClient)}...");
                // invoke event
                Reconnect?.Invoke(this, true);
            }
            else
            {
                _logger.LogError($"{nameof(StreamingClient)} failed with the following message: {message}");
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;
        private ReplayExtension _replayIdExtension;

        /// <summary>
        /// Disposing of the resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing of the resources
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

        /// <summary>
        /// Destructor
        /// </summary>
        ~StreamingClient()
        {
            Dispose(false);
        }

        #endregion
    }
}
