using System;
using System.Threading.Tasks;

namespace MessageQueue.Core.Abstract.Inbound
{
    /// <summary>
    /// Interface which provides functionality to receive message from queue
    /// in fire and forget pattern (Push-Pull, Producer-Consumer).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message</typeparam>
    public interface IInboundFaFMq<TMessage> : IMessageQueue
    {
        #region Events
        /// <summary>
        /// Event that will be triggered when message would be received from the queue.
        /// </summary>
        event Action<TMessage, IMessageReceiveOptions> OnMessageReady;

        /// <summary>
        /// Asynchronous event that will be triggered when message would be received from the queue.
        /// NOTE: If handler is registered with both events (sync and asyns) then async will be triggered only.
        /// </summary>
        event Func<TMessage, IMessageReceiveOptions, Task> OnMessageReadyAsync;
        #endregion

        #region Methods
        /// <summary>
        /// Will start listening the queue for messages.
        /// </summary>
        void StartReceivingMessage();

        /// <summary>
        /// Will stop listening the queue for messages.
        /// </summary>
        void StopReceivingMessage();

        /// <summary>
        /// Will check if the queue has any message.
        /// NOTE: This method will check for the message regardless of queue listening status (start or stop).
        /// </summary>
        /// <returns>True if there is message; otherwise false.</returns>
        bool HasMessage();
        #endregion
    }
}
