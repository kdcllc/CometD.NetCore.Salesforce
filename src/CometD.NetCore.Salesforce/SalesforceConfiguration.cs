namespace CometD.NetCore.Salesforce
{
    /// <summary>
    /// Represents the config settings in appsettings.json
    /// </summary>
    public sealed class SalesforceConfiguration
    {
        /// <summary>
        /// Consumer Id of the Salesforce Connected App
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Consumer secret of the Salesforce Connected App
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Url to login to Salesforce
        ///  https://test.salesforce.com/services/oauth2/authorize or https://login.salesforce.com/services/oauth2/authorize
        /// </summary>
        public string LoginUrl { get; set; }

        /// <summary>
        /// Url of the Salesforce organization
        /// </summary>
        public string OrganizationUrl { get; set; }

        /// <summary>
        /// Path of the endpoint to publish platform events
        /// </summary>
        public string PublishEndpoint { get; set; }

        /// <summary>
        /// Oauth refresh token of the Salesforce Connected App
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Uri to connect to CometD
        /// </summary>
        public string CometDUri { get; set; }

        /// <summary>
        /// Topic or Event Uri
        /// </summary>
        public string EventOrTopicUri { get; set; }

        /// <summary>
        /// Salesforce uri for oauth authentication.
        /// </summary>
        public string OAuthUri { get; set; }

        /// <summary>
        /// Retry for the connections and authentications
        /// </summary>
        public int Retry { get; set; }

        /// <summary>
        /// The event that gets raised when a request for proposal is approved and the Deal is in working status
        /// </summary>
        public string CustomEvent { get; set; }

        /// <summary>
        /// Salesforce ReplayId for specific message.
        /// </summary>
        public int ReplayId { get; set; }

        /// <summary>
        /// Long polling duration. Default  120 * 1000.
        /// </summary>
        public long? ReadTimeOut { get; set; }
    }
}
