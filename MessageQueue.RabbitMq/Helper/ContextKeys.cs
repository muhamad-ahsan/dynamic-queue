using MessageQueue.Core.Helper;
using System.Collections.Generic;

namespace MessageQueue.RabbitMq.Helper
{
    /// <summary>
    /// Contains context keys used to add contextual data in exception.
    /// </summary>
    internal static class ContextKeys
    {
        #region Keys
        public const string ReplyTo = "ReplyTo";
        public const string ReplyCode = "ReplyCode";
        public const string ReplyText = "ReplyText";
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(ContextKeys));
            #endregion
        }
        #endregion
    }
}
