using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetCoreForce.Client.Models;

namespace CometD.NetCore.Salesforce.Resilience
{
    /// <summary>
    ///  Resilient Library for <see cref="ForceClient"/>.
    /// </summary>
    public interface IResilientForceClient
    {
        /// <summary>
        ///     Get a basic SOQL COUNT() query result
        ///     The query must start with SELECT COUNT() FROM, with no named field in the count
        ///     clause. COUNT() must be the only element in the SELECT list.
        /// </summary>
        /// <param name="queryString"> SOQL query string starting with SELECT COUNT() FROM.</param>
        /// <param name="queryAll">True if deleted records are to be included. The default is false.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The System.Threading.Tasks.Task`1 returning the count.</returns>
        Task<int> CountQueryAsync(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///  Creates a new record.
        /// </summary>
        /// <typeparam name="T">The type for the SObject name, e.g. "Account".</typeparam>
        /// <param name="sObjectTypeName"></param>
        /// <param name="sObject">The SObject name, e.g. "Account".</param>
        /// <param name="customHeaders">  Custom headers to include in request (Optional). await The HeaderFormatter helper
        ///                               class can be used to generate the custom header as needed.
        /// </param>
        /// <param name="cancellationToken">The Cancellation Token.</param>
        /// <returns>CreateResponse object, includes new object's ID.</returns>
        Task<CreateResponse> CreateRecordAsync<T>(
           string sObjectTypeName,
           T sObject,
           Dictionary<string, string>? customHeaders = null,
           CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete record.
        /// </summary>
        /// <param name="sObjectTypeName">SObject name, e.g. "Account".</param>
        /// <param name="objectId"> Id of Object to update.</param>
        /// <param name="cancellationToken"></param>
        /// <returns> void, API returns 204/NoContent.</returns>
        Task DeleteRecordAsync(
            string sObjectTypeName,
            string objectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a List of Objects
        /// Use the Describe Global resource to list the objects available in your org and
        /// available to the logged-in user. This resource also returns the org encoding,
        ///  as well as maximum batch size permitted in queries.        ///. </summary>
        /// <param name="cancellationToken"></param>
        /// <returns> Returns DescribeGlobal object with a SObjectDescribe collection.</returns>
        Task<DescribeGlobal> DescribeGlobalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// List summary information about each REST API version currently available, including
        /// the version, label, and a link to each version's root. You do not need authentication
        /// to retrieve the list of versions.
        /// </summary>
        /// <param name="currentInstanceUrl">
        /// Current instance URL. If the client has been initialized, the parameter is optional
        /// and the client's current instance URL will be used.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<SalesforceVersion>> GetAvailableRestApiVersionsAsync(
            string? currentInstanceUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve(basic) metadata for an object.
        /// Use the SObject Basic Information resource to retrieve metadata for an object.
        /// </summary>
        /// <param name="objectTypeName">SObject name, e.g. Account.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SObjectBasicInfo> GetObjectBasicInfoAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get SObject by ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName"> SObject name, e.g. "Account".</param>
        /// <param name="objectId"> SObject ID.</param>
        /// <param name="fields">(optional) List of fields to retrieve, if not supplied, all fields are retrieved.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetObjectByIdAsync<T>(
            string sObjectTypeName,
            string objectId,
            List<string>? fields = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get field and other metadata for an object.
        /// Use the SObject Describe resource to retrieve all the metadata for an object,
        /// including information about each field, URLs, and child relationships.
        /// </summary>
        /// <param name="objectTypeName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns SObjectMetadataAll with full object meta including field metadata.</returns>
        Task<SObjectDescribeFull> GetObjectDescribeAsync(
            string objectTypeName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists information about limits in your org.
        /// This resource is available in REST API version 29.0 and later for API users with
        /// the View Setup and Configuration permission.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OrganizationLimits> GetOrganizationLimitsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current user's info via Identity URL
        ///   https://developer.salesforce.com/docs/atlas.en-us.mobile_sdk.meta/mobile_sdk/oauth_using_identity_urls.htm.
        /// </summary>
        /// <param name="identityUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>UserInfo.</returns>
        Task<UserInfo> GetUserInfoAsync(
            string identityUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or Updates a records based on external id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName">SObject name, e.g. "Account".</param>
        /// <param name="fieldName">External ID field name.</param>
        /// <param name="fieldValue">External ID field value.</param>
        /// <param name="sObject">Object to update.</param>
        /// <param name="customHeaders">Custom headers to include in request (Optional). await The HeaderFormatter helper class
        /// can be used to generate the custom header as needed.</param>
        /// <param name="cancellationToken">.</param>
        /// <returns></returns>
        Task<CreateResponse> InsertOrUpdateRecordAsync<T>(
            string sObjectTypeName,
            string fieldName,
            string fieldValue,
            T sObject,
            Dictionary<string, string>? customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve records using a SOQL query.
        /// Will automatically retrieve the complete result set if split into batches. If
        /// you want to limit results, use the LIMIT operator in your query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryString">SOQL query string, without any URL escaping/encoding.</param>
        /// <param name="queryAll">True if deleted records are to be included.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///  Retrieve a single record using a SOQL query.
        ///  Will throw an exception if multiple rows are retrieved by the query - if you
        ///  are note sure of a single result, use Query{T} instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryString">SOQL query string, without any URL escaping/encoding.</param>
        /// <param name="queryAll">True if deleted records are to be included.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>result object.</returns>
        Task<T> QuerySingleAsync<T>(
            string queryString,
            bool queryAll = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///  Executes a SOSL search, returning a type T, e.g. when using "RETURNING Account"
        ///  in the SOSL query.
        ///  Not properly matching the return type T and the RETURNING clause of the SOSL
        ///  query may return unexpected results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchString"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>SearchResult{T}.</returns>
        Task<SearchResult<T>> SearchAsync<T>(
            string searchString,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///  Does a basic test of the client's connection to the current Salesforce instance,
        ///  and that the API is responding to requests.
        ///  This does not validate authentication.
        ///  Makes a call to the Versions resource, since it requires no authentication or
        ///  permissions.
        /// </summary>
        /// <param name="currentInstanceUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns> True or false. Does not throw exceptions, only false in case of any errors.</returns>
        Task<bool> TestConnectionAsync(
            string? currentInstanceUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName">SObject name, e.g. "Account".</param>
        /// <param name="objectId">Id of Object to update.</param>
        /// <param name="sObject">Object to update.</param>
        /// <param name="customHeaders">
        /// Custom headers to include in request (Optional). await The HeaderFormatter helper
        /// class can be used to generate the custom header as needed.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateRecordAsync<T>(
            string sObjectTypeName,
            string objectId,
            T sObject,
            Dictionary<string, string>? customHeaders = null,
            CancellationToken cancellationToken = default);
    }
}
