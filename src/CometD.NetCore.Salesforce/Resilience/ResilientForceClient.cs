using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreForce.Client;
using NetCoreForce.Client.Models;
using Polly;
using Polly.Wrap;

namespace CometD.NetCore.Salesforce.Resilience
{
    public class ResilientForceClient : IResilientForceClient
    {
        private readonly AsyncExpiringLazy<NetCoreForce.Client.ForceClient> _forceClient;
        private readonly SalesforceConfiguration _options;
        private readonly ILogger<ResilientForceClient> _logger;
        private readonly AsyncPolicyWrap _policy;

        private const string PolicyContextMethod = nameof(PolicyContextMethod);

        /// <summary>
        /// Constructor for <see cref="ResilientForceClient"/>
        /// </summary>
        /// <param name="forceClient"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ResilientForceClient(
            Func<AsyncExpiringLazy<NetCoreForce.Client.ForceClient>> forceClient,
            IOptions<SalesforceConfiguration> options,
            ILogger<ResilientForceClient> logger)
        {
            if (forceClient == null)
            {
                throw new ArgumentNullException(nameof(forceClient));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _forceClient = forceClient();
            _options = options.Value;

            _policy = Policy.WrapAsync(GetAuthenticationRetryPolicy(), GetWaitAndRetryPolicy());
        }

        ///<inheritdoc/>
        public async Task<int> CountQueryAsync(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [PolicyContextMethod] = nameof(CountQueryAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.CountQuery(queryString, queryAll);
                },
                mContext,
                cancellationToken);
        }

        ///<inheritdoc/>
        public async Task<CreateResponse> CreateRecordAsync<T>(
            string sObjectTypeName,
            T sObject,
            Dictionary<string, string> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [PolicyContextMethod] = nameof(CreateRecordAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.CreateRecord<T>(
                        sObjectTypeName,
                        sObject,
                        customHeaders);
                },
                mContext,
                cancellationToken);
        }

        ///<inheritdoc/>
        public Task DeleteRecord(
            string sObjectTypeName,
            string objectId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<DescribeGlobal> DescribeGlobalAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<List<SalesforceVersion>> GetAvailableRestApiVersionsAsync(
            string currentInstanceUrl = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<SObjectBasicInfo> GetObjectBasicInfoAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<T> GetObjectByIdAsync<T>(
            string sObjectTypeName,
            string objectId,
            List<string> fields = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<SObjectDescribeFull> GetObjectDescribeAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<OrganizationLimits> GetOrganizationLimitsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<UserInfo> GetUserInfoAsync(
            string identityUrl,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<CreateResponse> InsertOrUpdateRecordAsync<T>(
            string sObjectTypeName,
            string fieldName,
            string fieldValue,
            T sObject,
            Dictionary<string, string> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<List<T>> QueryAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public IAsyncEnumerable<T> QueryAsync<T>(
            string queryString,
            bool queryAll = false,
            int? batchSize = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public IAsyncEnumerator<T> QueryAsyncEnumerator<T>(
            string queryString,
            bool queryAll = false,
            int? batchSize = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<T> QuerySingleAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<SearchResult<T>> SearchAsync<T>(
            string searchString,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public bool TestConnection(
            string currentInstanceUrl = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task UpdateRecordAsync<T>(
            string sObjectTypeName,
            string objectId,
            T sObject,
            Dictionary<string, string> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private IAsyncPolicy GetWaitAndRetryPolicy()
        {
            var jitterer = new Random();

            return Policy
                .Handle<Exception>(x => !(x is ForceApiException))
                .WaitAndRetryAsync(
                      retryCount: _options.Retry,    // exponential back-off plus some jitter
                      sleepDurationProvider: (retryAttempt, context) =>
                      {
                          return TimeSpan.FromSeconds(Math.Pow(_options.BackoffPower, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 100));
                      },
                      onRetry: (ex, span, context) =>
                      {
                          var methodName = context[PolicyContextMethod] ?? "MethodNotSpecified";

                          _logger.LogWarning(
                            "{Method} wait {Seconds} to execute with exception: {Message} for named policy: {Policy}",
                            methodName,
                            span.TotalSeconds,
                            ex.Message,
                            context.PolicyKey);
                      }
                  )
                 .WithPolicyKey($"{nameof(ResilientForceClient)}WaitAndRetryAsync");
        }

        private IAsyncPolicy GetAuthenticationRetryPolicy()
        {
            return Policy
                .Handle<ForceApiException>(x => x.Message.Contains("ErrorCode INVALID_SESSION_ID"))
                .RetryAsync(
                    retryCount: _options.Retry,
                    onRetry: (ex, count, context) =>
                    {
                        var methodName = context[PolicyContextMethod] ?? "MethodNotSpecified";

                        _logger.LogWarning(
                            "{Method} attempting to re-authenticate Retry {Count} of {Total} for named policy {PolicyKey}, due to {Message}.",
                            methodName,
                            count,
                            _options.Retry,
                            context.PolicyKey,
                            ex.Message);
                        _forceClient.Invalidate();
                    })
                .WithPolicyKey($"{nameof(ResilientForceClient)}RetryAsync");
        }
    }
}
