using System;
using System.IO;
using System.Threading.Tasks;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.Core.Properties;
using MessageQueue.ServiceBus.Helper;
using MessageQueue.Log.Core.Abstract;
using Microsoft.ServiceBus.Messaging;
using MessageQueue.ServiceBus.Abstract;
using MessageQueue.Core.Abstract.Outbound;

namespace MessageQueue.ServiceBus.Concrete.Outbound
{
    /// <summary>
    /// Azure ServiceBus based implementation of IOutboundFaFMq.
    /// </summary>
    internal sealed class SbOutboundFaF<TMessage> : BaseServiceBus, IOutboundFaFMq<TMessage>
    {
        #region Constructors
        public SbOutboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, false, loggerObject);

                // Setting other fields.
                Address = sbConfiguration.Address;
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion

        #region IOutboundFaFMq Implementation
        public string Address { get; private set; }

        public void SendMessage(TMessage message)
        {
            try
            {
                #region Sending Message
                CheckConnection();

                queueClient.Send(new BrokeredMessage(new MemoryStream(MessageQueueCommonItems.SerializeToJsonBytes(message))));
                #endregion
            }
            catch (QueueException queueException)
            {
                #region Logging - Error
                logger.Fatal(queueException, queueException.Message);
                #endregion

                throw;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToSendMessage,
                    message: ErrorMessages.FailedToSendMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }

        public async Task SendMessageAsync(TMessage message)
        {
            try
            {
                #region Sending Message
                CheckConnection();

                await queueClient.SendAsync(new BrokeredMessage(new MemoryStream(MessageQueueCommonItems.SerializeToJsonBytes(message))));
                #endregion
            }
            catch (QueueException queueException)
            {
                #region Logging - Error
                logger.Fatal(queueException, queueException.Message);
                #endregion

                throw;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToSendMessage,
                    message: ErrorMessages.FailedToSendMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            #region Cleanup
            if (disposing)
            {
                queueClient.Close();
            }
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper method to check connection before sending message.
        /// </summary>
        private void CheckConnection()
        {
            #region Connection Check
            // If connection is closed, then re-create the client.
            if (queueClient.IsClosed)
            {
                lock (queueClient)
                {
                    // This is to avoid if multiple re-creation when threads are waiting for the lock to be released.
                    if (queueClient.IsClosed)
                    {
                        queueClient = queueClient.MessagingFactory.CreateQueueClient(sbConfiguration.QueueName);
                    }
                }
            }
            #endregion
        }
        #endregion
    }
}
