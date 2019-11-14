namespace CometD.NetCore.Salesforce.Messaging
{
    /// <summary>
    /// Data from the Salesforce platform event reply.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class MessageData<TPayload> where TPayload : MessagePayload
    {
        /// <summary>
        /// Unique id of the data.
        /// <example>"schema": "1qUPELmVz7qUv3ntwyN1eA"</example>
        /// </summary>
        public string Schema { get; set; } = string.Empty;

        /// <summary>
        /// Generic type of the payload.
        /// <example>"payload": {}</example>
        /// </summary>
        public TPayload? Payload { get; set; }

        /// <summary>
        /// Contains Message event.
        /// <example>
        /// "event": {
        ///  "replayId": 10
        ///  }
        ///  </example>
        /// </summary>
        public MessageEvent? Event { get; set; }
    }
}
