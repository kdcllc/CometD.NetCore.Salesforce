namespace CometD.NetCore.Salesforce.Messaging
{
    /// <summary>
    /// Top level objects of the salesforce reply message.
    /// <example>
    /// "data": {}
    /// "channel": "/event/Custom_Event__e"
    /// </example>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class MessageEnvelope<TPayload> where TPayload : MessagePayload 
    {
        /// <summary>
        /// <see cref="MessageData{TPayload}"/> where TPayload is a generic message.
        /// </summary>
        public MessageData<TPayload> Data { get; set; }

        /// <summary>
        /// Channel or event information.
        /// <example>
        ///     "channel": "/event/Custom_Event__e"
        /// </example>
        /// </summary>
        public string Channel { get; set; }
    }
}
