namespace MessageQueue.Core.Helper
{
    /// <summary>
    /// Contains fully qualified class name with assembly name of different implementation of message queue interfaces.
    /// This is just a helper class to find easily different implementation details of queue interfaces.
    /// </summary>
    public static class InterfaceImplementationInfo
    {
        #region ZeroMq (NetMq)
        // Inbound
        public const string InboundFaFZeroMq = "MessageQueue.ZeroMq.Concrete.Inbound.ZmqInboundFaF`1, MessageQueue.ZeroMq";
        public const string InboundRaRZeroMq = "MessageQueue.ZeroMq.Concrete.Inbound.ZmqInboundRaR`2, MessageQueue.ZeroMq";

        // Outbound
        public const string OutboundFaFZeroMq = "MessageQueue.ZeroMq.Concrete.Outbound.ZmqOutboundFaF`1, MessageQueue.ZeroMq";
        public const string OutboundRaRZeroMq = "MessageQueue.ZeroMq.Concrete.Outbound.ZmqOutboundRaR`2, MessageQueue.ZeroMq";
        #endregion

        #region RabbitMq
        // Inbound
        public const string InboundFaFRabbitMq = "MessageQueue.RabbitMq.Concrete.Inbound.RmqInboundFaF`1, MessageQueue.RabbitMq";
        public const string InboundRaRRabbitMq = "MessageQueue.RabbitMq.Concrete.Inbound.RmqInboundRaR`2, MessageQueue.RabbitMq";

        // Outbound
        public const string OutboundFaFRabbitMq = "MessageQueue.RabbitMq.Concrete.Outbound.RmqOutboundFaF`1, MessageQueue.RabbitMq";
        public const string OutboundRaRRabbitMq = "MessageQueue.RabbitMq.Concrete.Outbound.RmqOutboundRaR`2, MessageQueue.RabbitMq";
        #endregion

        #region ServiceBus
        // Inbound
        public const string InboundFaFServiceBus = "MessageQueue.ServiceBus.Concrete.Inbound.SbInboundFaF`1, MessageQueue.ServiceBus";

        // Outbound
        public const string OutboundFaFServiceBus = "MessageQueue.ServiceBus.Concrete.Outbound.SbOutboundFaF`1, MessageQueue.ServiceBus";
        #endregion
    }
}
