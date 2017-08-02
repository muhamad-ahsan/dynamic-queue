using System.Collections.Generic;

namespace MessageQueue.Core.Helper
{
    /// <summary>
    /// Contains configuration keys common to all implementations.
    /// </summary>
    public static class CommonConfigurationKeys
    {
        #region Keys
        public const string Address = "Address";
        public const string QueueName = "QueueName";
        public const string Implementation = "Implementation";
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(CommonConfigurationKeys));
            #endregion
        }
        #endregion
    }
}
