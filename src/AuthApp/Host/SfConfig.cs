using System.ComponentModel.DataAnnotations;

namespace AuthApp.Host
{
    public class SfConfig
    {
        /// <summary>
        /// Salesforce Client Id
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Salesforece Secret Id
        /// </summary>
        [Required]
        public string ClientSecret { get; set; }

        /// <summary>
        /// i.e. https://login.salesforce.com
        /// </summary>
        [Required]
        [Url]
        public string LoginUrl { get; set; }

        /// <summary>
        /// Default set to /services/oauth2/token.
        /// </summary>
        public string OAuthTokenUri { get; set; } = "/services/oauth2/token";

        public string OAuthUri { get; set; } = "/services/oauth2/authorize";
    }
}
