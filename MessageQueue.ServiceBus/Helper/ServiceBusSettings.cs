using System;

namespace MessageQueue.ServiceBus.Helper
{
    /// <summary>
    /// Contains ServiceBus settings.
    /// </summary>
    internal sealed class ServiceBusConfiguration
    {
        #region Public Data Members
        public string QueueName { get; set; }
        public bool Acknowledgment { get; set; }
        public string Address { get; set; }
        public string NamespaceAddress { get; set; }
        public short MaxDeliveryCount { get; set; }
        public long MaxSizeInMegabytes { get; set; }
        public bool EnableDeadLettering { get; set; }
        public bool EnablePartitioning { get; set; }
        public bool RequiresDuplicateDetection { get; set; }
        public bool EnableBatchedOperations { get; set; }
        public TimeSpan MessageTimeToLiveInMinutes { get; set; }
        public TimeSpan LockDurationInSeconds { get; set; }
        public ushort MaxConcurrentReceiveCallback { get; set; }
        #endregion
    }
}
