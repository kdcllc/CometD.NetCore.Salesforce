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
    }
}
