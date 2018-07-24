using System;
using System.Threading.Tasks;
using NetCoreForce.Client;
using Polly;

namespace CometD.NetCore.Salesforce.ForceClient
{
    /// <summary>
    /// The <see cref="IAuthenticationClientProxy"/> proxy
    /// the functionality of <see cref="NetCoreForce.Client.ForceClient"/>
    /// library <see cref="NetCoreForce.Client.AuthenticationClient"/> class
    /// </summary>
    public class AuthenticationClientProxy : IAuthenticationClientProxy
    {
        private readonly SalesforceConfiguration _options;
        private readonly IAsyncPolicy _policy;
        private readonly AuthenticationClient _auth;


        /// <summary>
        /// Constructor <see cref="AuthenticationClientProxy"/> create instance of the class and authenticates the session.
        /// </summary>
        /// <param name="options"><see cref="SalesforceConfiguration"/></param>
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
        /// Returns instance of <see cref="NetCoreForce.Client.AuthenticationClient"/>
        /// </summary>
        public AuthenticationClient AuthenticationClient
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new ApplicationException($"{nameof(Authenticate)} must be called before fetching the {nameof(AuthenticationClient)}.");
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
            _auth.TokenRefreshAsync(_options.RefreshToken,
                                    _options.ClientId,
                                    _options.ClientSecret,
                                   $"{_options.LoginUrl}{_options.OAuthUri}")
            );
        }
    }
}
