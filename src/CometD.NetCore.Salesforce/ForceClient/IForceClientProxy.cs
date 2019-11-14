using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CometD.NetCore.Salesforce.Resilience;

using NetCoreForce.Client.Models;

namespace CometD.NetCore.Salesforce.ForceClient
{
    /// <summary>
    /// A Proxy interface around <see cref="NetCoreForce.Client.ForceClient"/>.
    /// </summary>
    [Obsolete("Use " + nameof(IResilientForceClient) + "instead.")]
    public interface IForceClientProxy
    {
        /// <summary>
        /// Returns <see cref="CreateResponse"/> on record creation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName"></param>
        /// <param name="instance"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task<CreateResponse> CreateRecord<T>(string sObjectTypeName, T instance, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Return <see cref="CreateResponse"/> on record creation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName"></param>
        /// <param name="instance"></param>
        /// <param name="token"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task<CreateResponse> CreateRecord<T>(string sObjectTypeName, T instance, CancellationToken token, Dictionary<string, string>? headers = null);

        /// <summary>
        /// Returns <see cref="Task{TResult}"/> from the Salesforce call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sObjectTypeName"></param>
        /// <param name="objectId"></param>
        /// <param name="token"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        Task<T> GetObjectById<T>(string sObjectTypeName, string objectId, CancellationToken token, List<string>? fields = null);
    }
}
