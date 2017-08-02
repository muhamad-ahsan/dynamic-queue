using System.Threading.Tasks;

namespace MessageQueue.Core.Abstract.Outbound
{
    /// <summary>
    /// Interface which provides functionality to send message into queue
    /// in fire and forget pattern (Push-Pull, Producer-Consumer).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message</typeparam>
    public interface IOutboundFaFMq<TMessage> : IMessageQueue
    {
        #region Methods
        /// <summary>
        /// Send message.
        /// </summary>
        /// <param name="message">The message</param>
        void SendMessage(TMessage message);

        /// <summary>
        /// Sends message asynchronously.
        /// </summary>
        /// <param name="message">The message</param>
        Task SendMessageAsync(TMessage message);
        #endregion
    }
}
