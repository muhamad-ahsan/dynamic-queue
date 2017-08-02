using NetMQ;
using System;
using System.Threading.Tasks;
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
    /// ZeroMq based implementation of IOutboundFaFMq.
    /// </summary>
    internal sealed class ZmqOutboundFaF<TMessage> : BaseZeroMq, IOutboundFaFMq<TMessage>
    {
        #region Private Methods
        private object lockForQueueOperation = new object();
        #endregion

        #region Constructors
        public ZmqOutboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, ZeroMqSocketType.Push, false, loggerObject);

                // Setting fields.
                Address = zmqConfiguration.Address;
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

        #region IOutboundFaFMq Implementation
        public string Address { get; }
        
        public void SendMessage(TMessage message)
        {
            try
            {
                #region Sending Message
                // We need to lock as the sockets are not multi-threaded in ZeroMq.
                lock (lockForQueueOperation)
                {
                    socket.SendFrame(MessageQueueCommonItems.SerializeToJson(message));
                }
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
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        
        public async Task SendMessageAsync(TMessage message)
        {
            try
            {
                #region Sending Message
                await Task.Run(() => SendMessage(message));
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
                lock (lockForQueueOperation)
                {
                    socket?.Dispose();
                }
            }
            #endregion
        }
        #endregion
    }
}
