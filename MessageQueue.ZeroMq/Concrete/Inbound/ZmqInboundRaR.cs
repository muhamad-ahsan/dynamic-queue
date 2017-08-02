using NetMQ;
using System;
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
    /// ZeroMq based implementation of IInboundRaRMq.
    /// </summary>
    internal sealed class ZmqInboundRaR<TRequest, TResponse> : BaseZeroMq, IInboundRaRMq<TRequest, TResponse>
    {
        #region Private Methods
        private volatile bool isReceivingMessages;
        #endregion

        #region Constructors
        public ZmqInboundRaR(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, ZeroMqSocketType.Router, true, loggerObject);

                // Setting fields.
                Address = zmqConfiguration.Address;

                // Binding on receive event.
                base.socket.ReceiveReady += ReceiveReady;

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

        #region IInboundRaRMq Implementation
        public string Address { get; }

        public event Action<RequestMessage<TRequest, TResponse>> OnRequestReady;
        
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

        public void StartReceivingRequest()
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
                    errorCode: QueueErrorCode.FailedToStartReceivingRequest,
                    message: ErrorMessages.FailedToStartReceivingRequest,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        
        public void StopReceivingRequest()
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
                    errorCode: QueueErrorCode.FailedToStopReceivingRequest,
                    message: ErrorMessages.FailedToStopReceivingRequest,
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
        private void ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                // Receiving client message.
                var clientRequest = e.Socket.ReceiveMultipartMessage();

                // Parsing client message.
                var clientAddress = clientRequest[0];
                var clientRequestData = MessageQueueCommonItems.DeserializeFromJson<TRequest>(clientRequest[2].ConvertToString());

                // Calling handler.
                OnRequestReady?.Invoke(new ZmqRequestMessage<TRequest, TResponse>(clientAddress, e.Socket, clientRequestData, ref logger));
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
                    errorCode: QueueErrorCode.FailedToReceiveRequestMessage,
                    message: ErrorMessages.FailedToReceiveRequestMessage,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion
    }
}
