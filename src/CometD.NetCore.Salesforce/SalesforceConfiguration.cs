using NetCoreForce.Client;

namespace CometD.NetCore.Salesforce
{
    /// <summary>
    /// Represents the configuration settings in appsettings.json.
    /// </summary>
    public sealed class SalesforceConfiguration
    {
        /// <summary>
        /// Consumer Id of the Salesforce Connected App.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Consumer secret of the Salesforce Connected App.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Username to login to Salesforce
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password to login to Salesforce
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// API Token for user identification to Salesforce API
        /// </summary>
        public string UserPasswordToken { get; set; } = string.Empty;

        /// <summary>
        /// Url to login to Salesforce
        ///  https://test.salesforce.com/services/oauth2/authorize
        ///  or https://login.salesforce.com/services/oauth2/authorize.
        /// </summary>
        public string LoginUrl { get; set; } = string.Empty;

        /// <summary>
        /// Url of the Salesforce organization.
        /// </summary>
        public string OrganizationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Path of the endpoint to publish platform events.
        /// </summary>
        public string PublishEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// OAuth refresh token of the Salesforce Connected App.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Uri to connect to CometD.
        /// </summary>
        public string CometDUri { get; set; } = string.Empty;

        /// <summary>
        /// Topic or Event Uri.
        /// </summary>
        public string EventOrTopicUri { get; set; } = string.Empty;

        /// <summary>
        /// Salesforce uri for OAuth authentication.
        /// </summary>
        public string OAuthUri { get; set; } = string.Empty;

        /// <summary>
        /// Retry for the connections and authentications.
        /// </summary>
        public int Retry { get; set; }

        /// <summary>
        /// The number to be used for BackoffPower for policy. The default is 2.
        /// </summary>
        public int BackoffPower { get; set; } = 2;

        /// <summary>
        /// The expiration for <see cref="ForceClient"/> token. The default is 1:00 hour.
        /// </summary>
        public string TokenExpiration { get; set; } = "01:00:00";

        /// <summary>
        /// The event that gets raised when a request for proposal is approved and the Deal is in working status.
        /// </summary>
        public string CustomEvent { get; set; } = string.Empty;

        /// <summary>
        /// Salesforce ReplayId for specific message.
        /// </summary>
        public int ReplayId { get; set; }

        /// <summary>
        /// Long polling duration. Default  120 * 1000.
        /// </summary>
        public long? ReadTimeOut { get; set; }

        /// <summary>
        /// The type of the Salesforce authentication token. Default "OAuth".
        /// </summary>
        public string TokenType { get; set; } = "OAuth";
    }
}
