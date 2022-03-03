namespace CometD.NetCore.Salesforce.Messaging
{
    /// <summary>
    /// Child element of <see cref="MessageData{TPayload}"/>.
    /// </summary>
    public class MessageEvent
    {
        /// <summary>
        /// Contains Salesforce replay id.
        /// <example>
        ///     "replayId": 10
        /// </example>
        /// </summary>
        public int ReplayId { get; set; }
        
        /// <summary>
        /// Contains Salesforce event Uuid.
        /// <example>
        ///     "EventUuid": "e981b488-81f3-4fcc-bd6f-f7033c9d7ac3"
        /// </example>
        /// </summary>
        public string EventUuid { get; set; }
    }
}
