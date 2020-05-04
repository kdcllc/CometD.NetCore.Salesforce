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
        private readonly AsyncExpiringLazy<ForceClient> _forceClient;
        private readonly SalesforceConfiguration _options;
        private readonly ILogger<ResilientForceClient> _logger;
        private readonly AsyncPolicyWrap _policy;

        private readonly string _policyContextMethod = nameof(_policyContextMethod);

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientForceClient"/> class.
        /// </summary>
        /// <param name="forceClient"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ResilientForceClient(
            Func<AsyncExpiringLazy<ForceClient>> forceClient,
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

        /// <inheritdoc/>
        public async Task<int> CountQueryAsync(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(CountQueryAsync)
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

        /// <inheritdoc/>
        public async Task<CreateResponse> CreateRecordAsync<T>(
            string sObjectTypeName,
            T sObject,
            Dictionary<string, string>? customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(CreateRecordAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.CreateRecord(
                        sObjectTypeName,
                        sObject,
                        customHeaders);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public Task DeleteRecordAsync(
            string sObjectTypeName,
            string objectId,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(DeleteRecordAsync)
            };

            return _policy.ExecuteAsync(
                (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return client.DeleteRecord(sObjectTypeName, objectId);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<DescribeGlobal> DescribeGlobalAsync(CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(DescribeGlobalAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.DescribeGlobal();
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<SalesforceVersion>> GetAvailableRestApiVersionsAsync(
            string? currentInstanceUrl = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetAvailableRestApiVersionsAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetAvailableRestApiVersions(currentInstanceUrl);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<SObjectBasicInfo> GetObjectBasicInfoAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetObjectBasicInfoAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetObjectBasicInfo(objectTypeName);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<T> GetObjectByIdAsync<T>(
            string sObjectTypeName,
            string objectId,
            List<string>? fields = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetObjectByIdAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetObjectById<T>(
                        sObjectTypeName,
                        objectId,
                        fields);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<SObjectDescribeFull> GetObjectDescribeAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetObjectDescribeAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetObjectDescribe(objectTypeName);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<OrganizationLimits> GetOrganizationLimitsAsync(CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetOrganizationLimitsAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetOrganizationLimits();
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<UserInfo> GetUserInfoAsync(
            string identityUrl,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(GetUserInfoAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.GetUserInfo(identityUrl);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<CreateResponse> InsertOrUpdateRecordAsync<T>(
            string sObjectTypeName,
            string fieldName,
            string fieldValue,
            T sObject,
            Dictionary<string, string>? customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(InsertOrUpdateRecordAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.InsertOrUpdateRecord(
                        sObjectTypeName,
                        fieldName,
                        fieldValue,
                        sObject,
                        customHeaders);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<T>> QueryAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(QueryAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.Query<T>(
                        queryString,
                        queryAll);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<T> QuerySingleAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(QuerySingleAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.QuerySingle<T>(
                        queryString,
                        queryAll);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<SearchResult<T>> SearchAsync<T>(
            string searchString,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(SearchAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await client.Search<T>(searchString);
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> TestConnectionAsync(
            string? currentInstanceUrl = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(TestConnectionAsync)
            };

            return await _policy.ExecuteAsync(
                async (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return await Task.FromResult(client.TestConnection(currentInstanceUrl));
                },
                mContext,
                cancellationToken);
        }

        /// <inheritdoc/>
        public Task UpdateRecordAsync<T>(
            string sObjectTypeName,
            string objectId,
            T sObject,
            Dictionary<string, string>? customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var mContext = new Context
            {
                [_policyContextMethod] = nameof(UpdateRecordAsync)
            };

            return _policy.ExecuteAsync(
                (context, token) =>
                {
                    var client = _forceClient.Value().Result;
                    return client.UpdateRecord(
                        sObjectTypeName,
                        objectId,
                        sObject,
                        customHeaders);
                },
                mContext,
                cancellationToken);
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
                          var methodName = context[_policyContextMethod] ?? "MethodNotSpecified";

                          _logger.LogWarning(
                            "{Method} wait {Seconds} to execute with exception: {Message} for named policy: {Policy}",
                            methodName,
                            span.TotalSeconds,
                            ex.Message,
                            context.PolicyKey);
                      })
                 .WithPolicyKey($"{nameof(ResilientForceClient)}WaitAndRetryAsync");
        }

        private IAsyncPolicy GetAuthenticationRetryPolicy()
        {
            return Policy
                .Handle<ForceApiException>(x => x.Message.Contains("ErrorCode INVALID_SESSION_ID"))
                .RetryAsync(
                    retryCount: _options.Retry,
                    onRetryAsync: async (ex, count, context) =>
                    {
                        var methodName = context[_policyContextMethod] ?? "MethodNotSpecified";

                        _logger.LogWarning(
                            "{Method} attempting to re-authenticate Retry {Count} of {Total} for named policy {PolicyKey}, due to {Message}.",
                            methodName,
                            count,
                            _options.Retry,
                            context.PolicyKey,
                            ex.Message);

                        await _forceClient.Invalidate();
                    })
                .WithPolicyKey($"{nameof(ResilientForceClient)}RetryAsync");
        }
    }
}
