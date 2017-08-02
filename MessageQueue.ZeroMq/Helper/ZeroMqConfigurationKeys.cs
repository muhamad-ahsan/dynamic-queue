using MessageQueue.Core.Helper;
using System.Collections.Generic;

namespace MessageQueue.ZeroMq.Helper
{
    /// <summary>
    /// Contains configuration keys related to ZeroMQ.
    /// </summary>
    internal static class ZeroMqConfigurationKeys
    {
        #region Keys
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(ZeroMqConfigurationKeys));
            #endregion
        }
        #endregion
    }
}
