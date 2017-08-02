using NetMQ;
using System;
using System.Text;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.ZeroMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.ZeroMq.Abstract;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.Core.Abstract.Outbound;

namespace MessageQueue.ZeroMq.Concrete.Outbound
{
    /// <summary>
    /// ZeroMq based implementation of IOutboundRaRMq.
    /// </summary>
    internal sealed class ZmqOutboundRaR<TResponse, TRequest> : BaseZeroMq, IOutboundRaRMq<TResponse, TRequest>
    {
        #region Private Data Members
        private readonly string socketIdentity;
        #endregion

        #region Constructors
        public ZmqOutboundRaR(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, ZeroMqSocketType.Dealer, false, loggerObject);

                // Setting fields.
                Address = zmqConfiguration.Address;

                // Need to set some unique identifier for connection identity (ZeroMq requirement).
                socketIdentity = Guid.NewGuid().ToString("N");
                socket.Options.Identity = Encoding.Unicode.GetBytes(socketIdentity);

                // Binding on receive event.
                socket.ReceiveReady += ResponseReady;

                // Initializing poller and starting it.
                poller = new NetMQPoller { socket };
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

        #region IOutboundRaRMq Implementation
        public string Address { get; }

        public event Action<TResponse> OnResponseReady;
        
        public void SendRequest(TRequest message)
        {
            #region Sending Message
            try
            {
                // Preparing message.
                var messageToServer = new NetMQMessage();
                messageToServer.AppendEmptyFrame();
                messageToServer.Append(MessageQueueCommonItems.SerializeToJson(message));

                // Sending message.
                socket.SendMultipartMessage(messageToServer);
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
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
            #endregion
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
        private void ResponseReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                // Receiving message.
                // Server will send empty frame followed by actual data frame so we need to skip the first frame.
                var receivedMessage = e.Socket.ReceiveMultipartStrings();

                if (receivedMessage.Count > 1)
                {
                    // Converting from Json.
                    var convertedMessage = MessageQueueCommonItems.DeserializeFromJson<TResponse>(receivedMessage[1]);

                    // Calling handler.
                    OnResponseReady?.Invoke(convertedMessage);
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
                    errorCode: QueueErrorCode.FailedToReceiveResponseMessage,
                    message: ErrorMessages.FailedToReceiveResponseMessage,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion
    }
}
