using NetMQ;
using System;
using NetMQ.Sockets;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.ZeroMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.ZeroMq.Abstract
{
    /// <summary>
    /// Base class for all ZeroMq based implementation of interfaces.
    /// </summary>
    internal abstract class BaseZeroMq
    {
        #region Protected Properties
        protected IQueueLogger logger;
        protected NetMQPoller poller;
        protected NetMQSocket socket;
        protected ZeroMqConfiguration zmqConfiguration;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Common initialization code.
        /// </summary>
        protected virtual void Initialize(Dictionary<string, string> configuration, ZeroMqSocketType socketType, bool isInbound, IQueueLogger loggerObject = null)
        {
            try
            {
                #region Logger Initialization
                logger = loggerObject;
                #endregion

                #region Parameters Collection
                zmqConfiguration = CommonItems.CollectZmqConfiguration(ref configuration, isInbound, ref logger);
                #endregion

                #region Creating Socket
                InitializeZeroMqSocket(socketType, isInbound);
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper method to create and initialize different types of ZeroMq sockets.
        /// </summary>
        private void InitializeZeroMqSocket(ZeroMqSocketType socketType, bool isInbound)
        {
            try
            {
                #region Initialization
                switch (socketType)
                {
                    case ZeroMqSocketType.Pair:
                    {
                        socket = new PairSocket(zmqConfiguration.Address);
                        break;
                    }

                    case ZeroMqSocketType.Pull:
                    {
                        socket = new PullSocket(zmqConfiguration.Address);
                        break;
                    }

                    case ZeroMqSocketType.Push:
                    {
                        socket = new PushSocket(zmqConfiguration.Address);
                        break;
                    }
                    
                    case ZeroMqSocketType.Dealer:
                    {
                        socket = new DealerSocket(zmqConfiguration.Address);
                        break;
                    }
                    
                    case ZeroMqSocketType.Router:
                    {
                        socket = new RouterSocket(zmqConfiguration.Address);
                        break;
                    }

                    default:
                        throw MessageQueueCommonItems.PrepareAndLogQueueException(
                            errorCode: QueueErrorCode.InvalidZeroMqSocketType,
                            message: ErrorMessages.InvalidZeroMqSocketType,
                            innerException: null,
                            queueContext: CommonItems.ZeroMqName,
                            address: zmqConfiguration.Address,
                            logger: logger);
                }

                // Setting options.
                socket.Options.Linger = Defaults.ZeroMqLinger;

                if (isInbound)
                {
                    socket.Options.ReceiveHighWatermark = Defaults.ZeroMqReceiveHighWatermark;
                }
                else
                {
                    socket.Options.SendHighWatermark = Defaults.ZeroMqSendHighWatermark;
                }
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCreateZeroMqSocket,
                    message: ErrorMessages.FailedToCreateZeroMqSocket,
                    innerException: ex,
                    queueContext: CommonItems.ZeroMqName,
                    address: zmqConfiguration?.Address,
                    logger: logger);
            }
        }
        #endregion
    }
}
