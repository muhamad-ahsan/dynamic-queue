using System;

namespace MessageQueue.ServiceBus.Helper
{
    /// <summary>
    /// Contains default values.
    /// </summary>
    internal static class Defaults
    {
        #region Public Data Members
        public const short MaxDeliveryCount = 5;
        public const long MaxSizeInMegabytes = 1024; // This is the minimum as of 25-Jan-2017
        public static TimeSpan LockDurationInSeconds = TimeSpan.FromSeconds(300); // 5 minutes and this is the max value as of 25-Jan-2017
        public static TimeSpan MessageTimeToLive = TimeSpan.FromDays(30);
        public const bool Acknowledgment = true;
        public const bool EnablePartitioning = false;
        public const bool EnableDeadLettering = true;
        public const bool EnableBatchedOperations = false;
        public const bool RequiresDuplicateDetection = false;
        public const int MaxConcurrentReceiveCallback = 1;
        #endregion
    }
}
