using System;
using RabbitMQ.Client;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.RabbitMq.Concrete
{
    /// <summary>
    /// RequestMessage implementation for RabbitMq.
    /// </summary>
    internal sealed class RmqRequestMessage<TRequest, TResponse> : RequestMessage<TRequest, TResponse>
    {
        #region Private Data Members
        private readonly IModel model;
        private readonly IQueueLogger logger;
        private readonly ulong deliveryTag;
        private readonly bool acknowledgment;
        private readonly string exchange, routingKey;
        private readonly IBasicProperties replyProperties;
        #endregion

        #region Constructors
        internal RmqRequestMessage(IModel model, IBasicProperties replyProperties, string exchange, string routingKey, bool acknowledgment, ulong deliveryTag, TRequest requestData, ref IQueueLogger logger)
        {
            #region Initialization
            RequestData = requestData;
            this.model = model;
            this.exchange = exchange;
            this.logger = logger;
            this.routingKey = routingKey;
            this.deliveryTag = deliveryTag;
            this.acknowledgment = acknowledgment;
            this.replyProperties = replyProperties;
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
                var responseBytes = MessageQueueCommonItems.SerializeToJsonBytes(response);

                // Sending response.
                model.BasicPublish(exchange, routingKey, replyProperties, responseBytes);

                // Acknowledging message.
                if (acknowledgment)
                {
                    model.BasicAck(deliveryTag, false);
                }
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
                    queueContext: CommonItems.RabbitMqName, 
                    context: new Dictionary<string, string>
                    {
                        [CommonContextKeys.ExchangeName] = exchange,
                        [CommonContextKeys.RoutingKey] = routingKey,
                        [ContextKeys.ReplyTo] = replyProperties.ReplyTo
                    },
                    logger: logger);
            }
        }
        #endregion
    }
}
