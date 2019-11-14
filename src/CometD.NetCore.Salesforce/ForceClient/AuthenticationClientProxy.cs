using System;
using System.Threading.Tasks;

using NetCoreForce.Client;

using Polly;

namespace CometD.NetCore.Salesforce.ForceClient
{
    /// <summary>
    /// The <see cref="IAuthenticationClientProxy"/> proxy
    /// the functionality of <see cref="NetCoreForce.Client.ForceClient"/>
    /// library <see cref="NetCoreForce.Client.AuthenticationClient"/> class.
    /// </summary>
    [Obsolete("Use " + nameof(ResilientStreamingClient) + "class instead.")]

    public class AuthenticationClientProxy : IAuthenticationClientProxy
    {
        private readonly SalesforceConfiguration _options;
        private readonly IAsyncPolicy _policy;
        private readonly AuthenticationClient _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationClientProxy"/> class and authenticates the session.
        /// </summary>
        /// <param name="options"><see cref="SalesforceConfiguration"/>.</param>
        public AuthenticationClientProxy(SalesforceConfiguration options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _policy = Policy
                   .Handle<Exception>()
                   .WaitAndRetryAsync(5, retryAttempt =>
                   TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            _auth = new AuthenticationClient();

            // sync call to authenticate on the creation of the instance of this object.
            Authenticate().Wait();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AuthenticationClientProxy"/> class.
        /// </summary>
        ~AuthenticationClientProxy()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns instance of <see cref="NetCoreForce.Client.AuthenticationClient"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If client is not authenticated.</exception>
        public AuthenticationClient AuthenticationClient
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException($"{nameof(Authenticate)} must be called before fetching the {nameof(AuthenticationClient)}.");
                }

                return _auth;
            }
        }

        /// <summary>
        /// Returns true/false if Authentication was successful.
        /// </summary>
        public bool IsAuthenticated => _auth?.AccessInfo != null;

        /// <summary>
        /// Authenticates and Obtain a new access token using a refresh token.
        /// </summary>
        /// <returns></returns>
        public Task Authenticate()
        {
            return _policy.ExecuteAsync(() =>
            _auth.TokenRefreshAsync(
                _options.RefreshToken,
                _options.ClientId,
                _options.ClientSecret,
                $"{_options.LoginUrl}{_options.OAuthUri}"));
        }

        /// <summary>
        /// Disposing of the resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _auth?.Dispose();
            }
        }
    }
}
