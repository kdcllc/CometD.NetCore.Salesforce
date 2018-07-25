using System;
using CometD.NetCore.Salesforce.Messaging;
using Newtonsoft.Json;

namespace TestApp.EventBus.Messages
{
    /// <summary>
    /// The <see cref="CustomMessagePayload"/>
    /// <example>
    ///{
    ///  "data": {
    ///    "schema": "1qUPELmVz7qUv3ntwyN1eA", 
    ///    "payload": {
    ///      "CreatedDate": "2018-07-21T02:26:10.433Z", 
    ///      "CreatedById": "005f2000008xN7VAAU", 
    ///      "Customer_Name__c": "test"
    ///    }, 
    ///    "event": {
    ///      "replayId": 1
    ///    }
    ///  }, 
    ///  "channel": "/event/Custom_Event__e"
    ///}
    /// </example>
    /// </summary>
    public class CustomMessagePayload : MessagePayload
    {
        [JsonProperty("Customer_Name__c")]
        public string CustomerName { get; set; }
    }
}
