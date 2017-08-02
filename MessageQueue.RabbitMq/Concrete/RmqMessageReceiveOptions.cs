using System;
using RabbitMQ.Client;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.RabbitMq.Concrete
{
    /// <summary>
    /// IMessageReceiveOptions implementation for RabbitMq.
    /// </summary>
    internal sealed class RmqMessageReceiveOptions : IMessageReceiveOptions
    {
        #region Private Data Members
        private IModel model;
        private IQueueLogger logger;
        private string queueName;
        private ulong deliveryTag;
        #endregion

        #region Constructors
        public RmqMessageReceiveOptions(IModel model, ulong deliveryTag, string queueName, bool isAcknowledgmentConfigured, ref IQueueLogger logger)
        {
            #region Initialization
            this.model = model;
            this.logger = logger;
            this.queueName = queueName;
            this.deliveryTag = deliveryTag;
            IsAcknowledgmentConfigured = isAcknowledgmentConfigured;
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
                    model.BasicAck(deliveryTag, false);
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue, 
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue, 
                        innerException: null, 
                        queueContext: CommonItems.RabbitMqName, 
                        queueName: queueName, 
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
                    queueContext: CommonItems.RabbitMqName, 
                    queueName: queueName, 
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
                    model.BasicNack(deliveryTag, false, true);
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue,
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue,
                        innerException: null,
                        queueContext: CommonItems.RabbitMqName,
                        queueName: queueName,
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
                    queueContext: CommonItems.RabbitMqName,
                    queueName: queueName,
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
