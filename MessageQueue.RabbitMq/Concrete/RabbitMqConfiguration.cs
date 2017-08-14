using MessageQueue.Core.Abstract;

namespace MessageQueue.RabbitMq.Concrete
{
    /// <summary>
    /// Contains RabbitMq configuration.
    /// </summary>
    internal class RabbitMqConfiguration : MessageQueueConfiguration
    {
        #region Public Data Members
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        public bool DurableQueue { get; set; }
        public bool DurableExchange { get; set; }
        public bool DurableMessage { get; set; }
        public bool Acknowledgment { get; set; }
        public int ConnectionTimeoutInMinutes { get; set; }
        public ushort MaxConcurrentReceiveCallback { get; set; }
        #endregion
    }
}
