using System.Collections.Generic;

namespace MessageQueue.Core.Helper
{
    /// <summary>
    /// Contains context keys used to add contextual data in exception, common to all implementations.
    /// </summary>
    public static class CommonContextKeys
    {
        #region Keys
        public const string Address = "Address";
        public const string QueueName = "QueueName";
        public const string RoutingKey = "RoutingKey";
        public const string ExchangeName = "ExchangeName";
        public const string QueueContext = "QueueContext";
        public const string ParameterName = "ParameterName";
        public const string ReplyQueueName = "ReplyQueueName";
        public const string NotSupportedParameters = "NotSupportedParameter(s)";
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(CommonContextKeys));
            #endregion
        }
        #endregion
    }
}
