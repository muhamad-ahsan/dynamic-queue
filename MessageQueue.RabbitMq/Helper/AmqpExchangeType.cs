namespace MessageQueue.RabbitMq.Helper
{
    /// <summary>
    /// Represents AMQP exchange types.
    /// </summary>
    internal enum AmqpExchangeType : ushort
    {
        Direct,
        Fanout,
        Topic,
        Header
    }
}
