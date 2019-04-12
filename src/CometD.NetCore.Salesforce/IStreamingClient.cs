using System;

using CometD.NetCore.Bayeux.Client;

namespace CometD.NetCore.Salesforce
{
    /// <summary>
    /// The <see cref="IStreamingClient"/> a wrapper class around  <see cref="Bayeux"/> client.
    /// </summary>
    public interface IStreamingClient : IDisposable
    {
        /// <summary>
        /// Event handler that sends event if the client must reconnect.
        /// </summary>
        event EventHandler<bool> Reconnect;

        /// <summary>
        /// True/False if the <see cref="IStreamingClient"/> is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the Bayeux server.
        /// </summary>
        void Handshake();

        /// <summary>
        /// Connects to the Bayeux server.
        /// </summary>
        /// <param name="timeout"></param>
        void Handshake(int timeout);

        /// <summary>
        /// Disconnect Salesforce subscription to the platform events.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Disconnect Salesforce subscription to the platform events.
        /// </summary>
        /// <param name="timeout"></param>
        void Disconnect(int timeout);

        /// <summary>
        /// Subscribe to Salesforce Platform event.
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="listener"></param>
        /// <param name="replayId"></param>
        void SubscribeTopic(string topicName, IMessageListener listener, long replayId=-1);

        /// <summary>
        /// Unsubscribe from Salesforce Platform event.
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="listener"></param>
        /// <param name="replayId"></param>
        /// <returns></returns>
        bool UnsubscribeTopic(string topicName, IMessageListener listener = null, long replayId=-1);
    }
}
