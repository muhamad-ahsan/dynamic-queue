using System;

namespace MessageQueue.Core.Abstract.Outbound
{
    /// <summary>
    /// Interface which provides functionality to send message into queue
    /// in request-response pattern.
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response</typeparam>
    /// <typeparam name="TRequest">The type of the request message</typeparam>
    public interface IOutboundRaRMq<TResponse, TRequest> : IMessageQueue
    {
        #region Events
        /// <summary>
        /// Event that will be triggered when response message would be received from the queue.
        /// </summary>
        event Action<TResponse> OnResponseReady;
        #endregion

        #region Methods
        /// <summary>
        /// Send request message.
        /// </summary>
        /// <param name="message">The message</param>
        void SendRequest(TRequest message);
        #endregion
    }
}
