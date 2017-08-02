using NetMQ;
using System;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Abstract;
using MessageQueue.ZeroMq.Helper;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.ZeroMq.Concrete
{
    /// <summary>
    /// RequestMessage implementation for ZeroMq.
    /// </summary>
    internal sealed class ZmqRequestMessage<TRequest, TResponse> : RequestMessage<TRequest, TResponse>
    {
        #region Private Data Members
        private IQueueLogger logger;
        private NetMQFrame clientAddress;
        private NetMQSocket responseChannel;
        #endregion

        #region Constructors
        internal ZmqRequestMessage(NetMQFrame clientAddress, NetMQSocket responseChannel, TRequest requestData, ref IQueueLogger loggerObject)
        {
            #region Initialization
            logger = loggerObject;
            RequestData = requestData;
            this.clientAddress = clientAddress;
            this.responseChannel = responseChannel;
            #endregion
        }
        #endregion

        #region RequestMessage Implementation
        public override void Response(TResponse response)
        {
            try
            {
                #region Sending Response
                // Preparing response.
                var messageToClient = new NetMQMessage();
                messageToClient.Append(clientAddress);
                messageToClient.AppendEmptyFrame();
                messageToClient.Append(MessageQueueCommonItems.SerializeToJson(response));

                // Sending response.
                responseChannel.SendMultipartMessage(messageToClient);
                #endregion
            }
            catch (QueueException queueException)
            {
                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToSendResponseMessage,
                    message: ErrorMessages.FailedToSendResponseMessage,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    logger: logger);
            }
        }
        #endregion
    }
}
