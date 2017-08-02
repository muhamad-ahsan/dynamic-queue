using MessageQueue.Core.Helper;
using System.Collections.Generic;

namespace MessageQueue.ServiceBus.Helper
{
    /// <summary>
    /// Contains configuration keys related to ServiceBus.
    /// </summary>
    internal static class ServiceBusConfigurationKeys
    {
        #region Keys
        public const string EnableDeadLettering = "EnableDeadLettering";
        public const string MessageTimeToLiveInMinutes = "MessageTimeToLiveInMinutes";
        public const string Acknowledgment = "Acknowledgment";
        public const string LockDurationInSeconds = "LockDurationInSeconds";
        public const string MaxDeliveryCount = "MaxDeliveryCount";
        public const string MaxSizeInMegabytes = "MaxSizeInMegabytes";
        public const string EnablePartitioning = "EnablePartitioning";
        public const string EnableBatchedOperations = "EnableBatchedOperations";
        public const string RequiresDuplicateDetection = "RequiresDuplicateDetection";
        public const string NamespaceAddress = "NamespaceAddress";
        public const string MaxConcurrentReceiveCallback = "MaxConcurrentReceiveCallback";
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns all the keys available in the class.
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            #region Return
            return MessageQueueCommonItems.GetAllStringConstants(typeof(ServiceBusConfigurationKeys));
            #endregion
        }
        #endregion
    }
}
