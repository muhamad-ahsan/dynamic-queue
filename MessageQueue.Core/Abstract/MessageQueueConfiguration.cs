namespace MessageQueue.Core.Abstract
{
    /// <summary>
    /// Contains common configuration properties.
    /// </summary>
    public abstract class MessageQueueConfiguration
    {
        #region Public Data Members
        /// <summary>
        /// The address of the queue (host or server).
        /// </summary>
        public string Address { get; set; }
        #endregion
    }
}
