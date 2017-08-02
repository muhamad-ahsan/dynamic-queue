using NetMQ;
using System;
using System.Threading.Tasks;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Concrete;
using MessageQueue.ZeroMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.ZeroMq.Abstract;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.Core.Abstract.Inbound;

namespace MessageQueue.ZeroMq.Concrete.Inbound
{
    /// <summary>
    /// ZeroMq based in-process implementation of IInboundFaFMq.
    /// </summary>
    internal sealed class ZmqIpInboundFaF<TMessage> : BaseZeroMq, IInboundFaFMq<TMessage>
    {
        #region Private Methods
        private volatile bool isReceivingMessages;
        private object lockForQueueOperation = new object();
        #endregion

        #region Constructors
        public ZmqIpInboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, ZeroMqSocketType.Pair, true, loggerObject);

                // Setting fields.
                Address = zmqConfiguration.Address;

                // Binding on receive event.
                socket.ReceiveReady += ReceiveReady;

                // Initializing poller.
                poller = new NetMQPoller();
                poller.RunAsync();
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration?.Address,
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
                return socket.HasIn;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueHasMessage,
                    message: ErrorMessages.FailedToCheckQueueHasMessage,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        
        public void StartReceivingMessage()
        {
            try
            {
                lock (poller)
                {
                    if (!isReceivingMessages)
                    {
                        poller.Add(socket);

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
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        
        public void StopReceivingMessage()
        {
            try
            {
                lock (poller)
                {
                    if (isReceivingMessages)
                    {
                        poller.Remove(socket);

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
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
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
                socket?.Dispose();
                poller?.Dispose();
            }
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper event handler.
        /// </summary>
        private async void ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                string receivedMessage;

                // We need to lock as the sockets are not multi-threaded in ZeroMq.
                lock (lockForQueueOperation)
                {
                    // Receiving message.
                    receivedMessage = e.Socket.ReceiveFrameString();
                }

                // Converting from Json.
                var convertedMessage = MessageQueueCommonItems.DeserializeFromJson<TMessage>(receivedMessage);
                var messageReceiveOptions = new ZmqMessageReceiveOptions(ref logger);

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
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion
    }
}
