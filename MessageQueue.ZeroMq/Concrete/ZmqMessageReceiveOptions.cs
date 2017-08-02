using System;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Concrete;
using MessageQueue.ZeroMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.ZeroMq.Concrete
{
    /// <summary>
    /// IMessageReceiveOptions implementation for ZeroMq.
    /// </summary>
    internal sealed class ZmqMessageReceiveOptions : IMessageReceiveOptions
    {
        #region Private Data Members
        private IQueueLogger logger;
        #endregion

        #region Constructors
        public ZmqMessageReceiveOptions(ref IQueueLogger loggerObject)
        {
            #region Initialization
            logger = loggerObject;
            IsAcknowledgmentConfigured = false; // Acknowledgment is not supported yet in ZeroMq.
            #endregion
        }
        #endregion

        #region IMessageReceiveOptions Implementation
        // Properties
        public bool IsAcknowledgmentConfigured { get; }

        // Methods
        public void Acknowledge(bool ignoreError = true)
        {
            try
            {
                #region Acknowledging Message
                if (IsAcknowledgmentConfigured)
                {
                    // Not supported feature yet.
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue,
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue,
                        innerException: null,
                        queueContext: CommonItems.ZeroMqName,
                        logger: logger);
                }
                #endregion
            }
            catch (QueueException)
            {
                if (ignoreError == false)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var queueException = MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToAcknowledgeMessage,
                    message: ErrorMessages.FailedToAcknowledgeMessage,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    logger: logger);

                if (ignoreError == false)
                {
                    throw queueException;
                }
            }
        }
        
        public void AbandonAcknowledgment(bool ignoreError = true)
        {
            try
            {
                #region Abandoning Acknowledgment
                if (IsAcknowledgmentConfigured)
                {
                    // Not supported feature yet.
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue,
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue,
                        innerException: null,
                        queueContext: CommonItems.ZeroMqName,
                        logger: logger);
                }
                #endregion
            }
            catch (QueueException)
            {
                if (ignoreError == false)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var queueException = MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToAbandonMessageAcknowledgment,
                    message: ErrorMessages.FailedToAbandonMessageAcknowledgment,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    logger: logger);

                if (ignoreError == false)
                {
                    throw queueException;
                }
            }
        }
        #endregion
    }
}
