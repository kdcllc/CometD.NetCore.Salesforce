using System.Threading.Tasks;
using NetCoreForce.Client;

namespace CometD.NetCore.Salesforce.ForceClient
{
    /// <summary>
    /// A wrapper interface around <see cref="NetCoreForce.Client.AuthenticationClient"/>
    /// </summary>
    public interface IAuthenticationClientProxy
    {
        /// <summary>
        /// Returns <see cref="NetCoreForce.Client.AuthenticationClient"/>
        /// upon successful authentication with Salesforce.
        /// </summary>
        AuthenticationClient AuthenticationClient { get; }

        /// <summary>
        /// True/False on the Authentication success or failure.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Authenticate session with access token and refresh token via OAuth.
        /// </summary>
        /// <returns></returns>
        Task Authenticate();
    }
}
