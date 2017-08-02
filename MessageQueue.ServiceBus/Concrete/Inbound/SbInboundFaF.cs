using System;
using System.IO;
using System.Threading.Tasks;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Properties;
using MessageQueue.ServiceBus.Helper;
using MessageQueue.Log.Core.Abstract;
using Microsoft.ServiceBus.Messaging;
using MessageQueue.ServiceBus.Abstract;
using MessageQueue.Core.Abstract.Inbound;

namespace MessageQueue.ServiceBus.Concrete.Inbound
{
    /// <summary>
    /// ServiceBus based implementation of IInboundFaFMq.
    /// </summary>
    internal sealed class SbInboundFaF<TMessage> : BaseServiceBus, IInboundFaFMq<TMessage>
    {
        #region Private Data Members
        private MessageReceiver messageReceiver;
        private volatile bool isReceivingMessages;
        private QueueClient acknowledgmentQueueClient;
        #endregion

        #region Constructors
        public SbInboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, true, loggerObject);
                
                // Setting other fields.
                Address = sbConfiguration.Address;

                // Creating queue client for acknowledgment.
                if (sbConfiguration.Acknowledgment)
                {
                    acknowledgmentQueueClient = queueClient.MessagingFactory.CreateQueueClient(sbConfiguration.QueueName);
                }
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

        #region IInboundFaFMq Implementation
        public string Address { get; }

        public event Action<TMessage, IMessageReceiveOptions> OnMessageReady;
        public event Func<TMessage, IMessageReceiveOptions, Task> OnMessageReadyAsync;

        public bool HasMessage()
        {
            try
            {
                // Creating new receiver just to avoid closed connection issue.
                var messageReceiverToPeek = queueClient.MessagingFactory.CreateMessageReceiver(sbConfiguration.QueueName, ReceiveMode.PeekLock);

                // Peeking
                var result = messageReceiverToPeek.Peek() != null;

                // Closing connection.
                messageReceiverToPeek.Close();

                return result;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueHasMessage,
                    message: ErrorMessages.FailedToCheckQueueHasMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        
        public void StartReceivingMessage()
        {
            try
            {
                lock (queueClient)
                {
                    if (!isReceivingMessages)
                    {
                        // Creating message receiver.
                        messageReceiver = queueClient.MessagingFactory.CreateMessageReceiver(sbConfiguration.QueueName, (sbConfiguration.Acknowledgment ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete));

                        // Registering event.
                        messageReceiver.OnMessageAsync(ReceiveReadyAsync, new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = sbConfiguration.MaxConcurrentReceiveCallback });

                        // Updating flag.
                        isReceivingMessages = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToStartReceivingMessage,
                    message: ErrorMessages.FailedToStartReceivingMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        
        public void StopReceivingMessage()
        {
            try
            {
                lock (queueClient)
                {
                    if (isReceivingMessages)
                    {
                        messageReceiver.Close();

                        // Updating flag.
                        isReceivingMessages = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToStopReceivingMessage,
                    message: ErrorMessages.FailedToStopReceivingMessage,
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
        /// Helper event handler.
        /// </summary>
        private async Task ReceiveReadyAsync(BrokeredMessage brokeredMessage)
        {
            try
            {
                if (brokeredMessage != null)
                {
                    // Converting from Json.
                    var convertedMessage = MessageQueueCommonItems.DeserializeFromJson<TMessage>(new StreamReader(brokeredMessage.GetBody<Stream>()).ReadToEnd());
                    var lockToken = Guid.Empty;

                    // Getting lock token if acknowledgment is configured.
                    if (sbConfiguration.Acknowledgment)
                    {
                        lockToken = brokeredMessage.LockToken;
                    }

                    var messageReceiveOptions = new SbMessageReceiveOptions(lockToken, sbConfiguration.QueueName, sbConfiguration.Acknowledgment, ref acknowledgmentQueueClient, ref logger);

                    // Calling handler (async is preferred over sync).
                    if (OnMessageReadyAsync != null)
                    {
                        await OnMessageReadyAsync.Invoke(convertedMessage, messageReceiveOptions);
                    }
                    else
                    {
                        OnMessageReady?.Invoke(convertedMessage, messageReceiveOptions);
                    }
                }
            }
            catch (QueueException queueException)
            {
                #region Logging - Error
                logger.Fatal(queueException, queueException.Message);
                #endregion
            }
            catch (Exception ex)
            {
                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToReceiveMessage,
                    message: ErrorMessages.FailedToReceiveMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion
    }
}
