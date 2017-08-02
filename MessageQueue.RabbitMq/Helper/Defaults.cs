namespace MessageQueue.RabbitMq.Helper
{
    /// <summary>
    /// Contains default values.
    /// </summary>
    internal static class Defaults
    {
        #region Public Data Members
        public const int RabbitMqConnectionTimeoutInMinutes = 3;
        public const int RabbitMqNetworkRecoveryIntervalInSeconds = 30;
        public const int MaxConcurrentReceiveCallback = 1;
        #endregion
    }
}
