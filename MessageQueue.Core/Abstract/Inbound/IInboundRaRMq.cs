using System;

namespace MessageQueue.Core.Abstract.Inbound
{
    /// <summary>
    /// Interface which provides functionality to receive message from queue
    /// and response to client in request-response pattern.
    /// </summary>
    /// <typeparam name="TRequest">The expected type of the request</typeparam>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    public interface IInboundRaRMq<TRequest, TResponse> : IMessageQueue
    {
        #region Events
        /// <summary>
        /// Event that will be triggered when request message would be received from the queue.
        /// </summary>
        event Action<RequestMessage<TRequest, TResponse>> OnRequestReady;
        #endregion

        #region Methods
        /// <summary>
        /// Will start listening the queue for messages.
        /// </summary>
        void StartReceivingRequest();

        /// <summary>
        /// Will stop listening the queue for messages.
        /// </summary>
        void StopReceivingRequest();

        /// <summary>
        /// Will check if the queue has any message.
        /// NOTE: This method will check for the message regardless of queue listening status (start or stop).
        /// </summary>
        /// <returns>True if there is message; otherwise false.</returns>
        bool HasMessage();
        #endregion
    }
}
