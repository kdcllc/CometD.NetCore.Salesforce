using System;

namespace CometD.NetCore.Salesforce.Messaging
{
    /// <summary>
    /// The <see cref="MessagePayload"/> payload base class for the other salesforce events.
    /// </summary>
    public class MessagePayload
    {
        /// <summary>
        /// The <see cref="DateTimeOffset"/> payload creation timestamp.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The <see cref="MessagePayload"/> user id.
        /// </summary>
        public string CreatedById { get; set; }
    }
}
