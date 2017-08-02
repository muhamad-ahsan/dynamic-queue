namespace MessageQueue.ZeroMq.Helper
{
    /// <summary>
    /// Represents ZeroMq socket types.
    /// </summary>
    internal enum ZeroMqSocketType : ushort
    {
        Pair,
        Pull,
        Push,
        Dealer,
        Router
    }
}
