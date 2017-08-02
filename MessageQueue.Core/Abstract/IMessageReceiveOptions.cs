namespace MessageQueue.Core.Abstract
{
    /// <summary>
    /// Generic interface which provides different options on message receive.
    /// </summary>
    public interface IMessageReceiveOptions
    {
        #region Properties
        /// <summary>
        /// Returns true if message acknowledgment is configured; false otherwise.
        /// </summary>
        bool IsAcknowledgmentConfigured { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Acknowledges message.
        /// If acknowledgment is not configured, it will throw exception
        /// NOTE: Any exception will be swallowed if ignoreError is set to true (default). 
        /// </summary>
        void Acknowledge(bool ignoreError = true);

        /// <summary>
        /// When acknowledgment is configured and processing is failed due to any reason,
        /// calling this method will abandon the lock and message will be released from lock.
        /// If acknowledgment is not configured, it will throw exception
        /// NOTE: Any exception will be swallowed if ignoreError is set to true (default). 
        /// </summary>
        void AbandonAcknowledgment(bool ignoreError = true);
        #endregion
    }
}
