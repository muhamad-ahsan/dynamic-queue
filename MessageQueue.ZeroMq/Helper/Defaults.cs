using System;

namespace MessageQueue.ZeroMq.Helper
{
    /// <summary>
    /// Contains default values.
    /// </summary>
    internal static class Defaults
    {
        #region Public Data Members
        public static TimeSpan ZeroMqLinger = new TimeSpan(0, 0, -1); // Need to set to avoid messages lost.
        public const int ZeroMqSendHighWatermark = 10000;
        public const int ZeroMqReceiveHighWatermark = 10000;
        #endregion
    }
}
