using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetCoreForce.Client;
using NetCoreForce.Client.Models;
using Polly;

namespace CometD.NetCore.Salesforce.ForceClient
{
    /// <summary>
    /// The <see cref="IForceClientProxy"/> proxy functionality around <see cref="NetCoreForce.Client.ForceClient"/>.
    /// <see cref="!:https://github.com/anthonyreilly/NetCoreForce/blob/master/src/NetCoreForce.Client/ForceClient.cs"/>
    /// </summary>
    public class ForceClientProxy : IForceClientProxy
    {
        private readonly SalesforceConfiguration _options;
        private readonly IAuthenticationClientProxy _authenticationClient;
        private readonly ILogger _logger;
        private NetCoreForce.Client.ForceClient _forceClient;
        private readonly IAsyncPolicy _policy;

        /// <summary>
        ///  Constructor for <see cref="AuthenticationClientProxy"/>
        ///  that create an instance of <see cref="NetCoreForce.Client.ForceClient"/>.
        /// </summary>
        /// <param name="authenticationClient">Instance of <see cref="AuthenticationClientProxy"/> that creates instance of <see cref="AuthenticationClient"/>.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{IForceClientProxy}"/>.</param>
        /// <param name="options">Options based on <see cref="SalesforceConfiguration"/></param>
        public ForceClientProxy(IAuthenticationClientProxy authenticationClient,
            ILogger<ForceClientProxy> logger,
            SalesforceConfiguration options)
        {
            #region ArgumentException and ArgumentNullException

            _options = options ?? throw new ArgumentNullException(nameof(options));

            _authenticationClient = authenticationClient ??
                                    throw new ArgumentNullException(nameof(authenticationClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            #endregion

            // creates an instance of the forceclient register as singleton
            _forceClient = new NetCoreForce.Client.ForceClient(
                _authenticationClient.AuthenticationClient.AccessInfo.InstanceUrl,
                _authenticationClient.AuthenticationClient.ApiVersion,
                _authenticationClient.AuthenticationClient.AccessInfo.AccessToken
            );

            // create retry authentication policy
            _policy = CreateAuthenticationWaitAndRetry(_options.Retry,
                                nameof(ForceClientProxy), OnWaitAndRetry);
        }

       
        #region Private

        private async Task RefreshAuthorization()
        {
            await _authenticationClient.Authenticate();

            _forceClient = new NetCoreForce.Client.ForceClient(
                _authenticationClient.AuthenticationClient.AccessInfo.InstanceUrl,
                _authenticationClient.AuthenticationClient.ApiVersion,
                _authenticationClient.AuthenticationClient.AccessInfo.AccessToken
            );

            _logger.LogDebug($"Salesforce Authentication Successful!");
        }

        private async Task OnWaitAndRetry(Exception ex, int count, Context context)
        {
            _logger.LogWarning($"Trying to {nameof(RefreshAuthorization)}");
            _logger.LogWarning($"Retry {context.Count}:{count} of {context.PolicyKey}, due to {ex.Message}.");
            await RefreshAuthorization();
        }

        private IAsyncPolicy CreateAuthenticationWaitAndRetry(int retryAuthorization,
           string name,
           Func<Exception, int, Context, Task> retryHook)
        {
            return Policy
                    .Handle<ForceApiException>(x => x.Message.Contains("ErrorCode INVALID_SESSION_ID"))
                    .RetryAsync(retryCount: retryAuthorization,
                    onRetryAsync: retryHook)
                    .WithPolicyKey($"{name}Retry");
        }
        #endregion

        /// <summary>
        /// Retrieves a Salesforce object by its Id
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="sObjectTypeName">Salesforce object type name</param>
        /// <param name="objectId">Id of the object to retrieve</param>
        /// <param name="token">Token to cancel operation</param>
        /// <param name="fields">List of fields to return.</param>
        /// <returns>The retrieved object from salesforce.</returns>
        public async Task<T> GetObjectById<T>(string sObjectTypeName, string objectId, CancellationToken token,
            List<string> fields = null)
        {
            return await _policy.ExecuteAsync(ctx => _forceClient.GetObjectById<T>(sObjectTypeName, objectId, fields), token);
        }

        /// <summary>
        /// Creates a record in the Salesforce instance.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="sObjectTypeName">Salesforce object type name</param>
        /// <param name="instance">Object to create</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>A <see cref="CreateResponse"/> representing the operation result.</returns>
        public async Task<CreateResponse> CreateRecord<T>(string sObjectTypeName, T instance,
            Dictionary<string, string> headers = null)
        {
            return await _policy.ExecuteAsync(() => _forceClient.CreateRecord(sObjectTypeName, instance, headers));
        }

        /// <summary>
        /// Creates a record in the Salesforce organization
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="sObjectTypeName">Salesforce object type name</param>
        /// <param name="instance">Object to create</param>
        /// <param name="token">Token to cancel operation</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>A <see cref="CreateResponse"/> representing the operation result.</returns>
        public async Task<CreateResponse> CreateRecord<T>(string sObjectTypeName, T instance,
            CancellationToken token, Dictionary<string, string> headers = null)
        {
            return await _policy.ExecuteAsync(ctx => _forceClient.CreateRecord(sObjectTypeName, instance, headers), token);
        }
    }
}
