using MessageQueue.Core.Helper;
using System.Collections.Generic;

namespace MessageQueue.RabbitMq.Helper
{
    /// <summary>
    /// Contains configuration keys related to RabbitMq.
    /// </summary>
    internal static class RabbitMqConfigurationKeys
    {
        #region Keys
        public const string Port = "Port";
        public const string Password = "Password";
        public const string UserName = "UserName";
        public const string ExchangeName = "ExchangeName";
        public const string Acknowledgment = "Acknowledgment";
        public const string RoutingKey = "RoutingKey";
        public const string DurableExchange = "DurableExchange";
        public const string DurableQueue = "DurableQueue";
        public const string DurableMessage = "DurableMessage";
        public const string ConnectionTimeoutInMinutes = "ConnectionTimeoutInMinutes";
        public const string MaxConcurrentReceiveCallback = "MaxConcurrentReceiveCallback";
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(RabbitMqConfigurationKeys));
            #endregion
        }
        #endregion
    }
}
