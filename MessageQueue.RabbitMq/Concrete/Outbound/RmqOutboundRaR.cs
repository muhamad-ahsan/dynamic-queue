using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.RabbitMq.Abstract;
using MessageQueue.Core.Abstract.Outbound;

namespace MessageQueue.RabbitMq.Concrete.Outbound
{
    /// <summary>
    /// RabbitMq based implementation of IOutboundRaRMq.
    /// </summary>
    internal sealed class RmqOutboundRaR<TResponse, TRequest> : BaseRabbitMq, IOutboundRaRMq<TResponse, TRequest>
    {
        #region Private Data Members
        private List<string> correlationIds;
        private object lockForQueueOperation = new object();
        private string exchangeName, routingKey, replyQueueName;
        #endregion

        #region Constructors
        public RmqOutboundRaR(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, AmqpExchangeType.Direct, false, loggerObject);

                // Setting other fields.
                Address = rabbitMqConfiguration.Address;
                exchangeName = rabbitMqConfiguration.ExchangeName ?? string.Empty;
                routingKey = rabbitMqConfiguration.RoutingKey ?? rabbitMqConfiguration.QueueName;
                replyQueueName = model.QueueDeclare().QueueName;
                correlationIds = new List<string>();

                // Registering event.
                var consumer = new EventingBasicConsumer(model);
                consumer.Received += ResponseReady;

                // Binding with queue.
                model.BasicConsume(replyQueueName, true, consumer);
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                #region Adding Context Data
                var context = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(rabbitMqConfiguration.ExchangeName))
                {
                    context.Add(CommonContextKeys.ExchangeName, rabbitMqConfiguration.ExchangeName);
                }

                if (!string.IsNullOrEmpty(rabbitMqConfiguration.RoutingKey))
                {
                    context.Add(CommonContextKeys.RoutingKey, rabbitMqConfiguration.RoutingKey);
                }
                #endregion

                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    context: context,
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
                var correlationId = Guid.NewGuid().ToString("N");
                var messageProperties = model.CreateBasicProperties();
                messageProperties.ReplyTo = replyQueueName;
                messageProperties.CorrelationId = correlationId;

                // Storing correlation Id.
                lock (correlationIds)
                {
                    correlationIds.Add(correlationId);
                }

                // Sending message.
                lock (lockForQueueOperation)
                {
                    model.BasicPublish(exchangeName, routingKey, messageProperties, MessageQueueCommonItems.SerializeToJsonBytes(message));
                }
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
                #region Adding Context Data
                var context = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(rabbitMqConfiguration.ExchangeName))
                {
                    context.Add(CommonContextKeys.ExchangeName, rabbitMqConfiguration.ExchangeName);
                }

                if (!string.IsNullOrEmpty(routingKey))
                {
                    context.Add(CommonContextKeys.RoutingKey, routingKey);
                }
                #endregion

                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToSendMessage,
                    message: ErrorMessages.FailedToSendMessage,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    context: context,
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
                connection?.Dispose();
                model?.Dispose();
            }
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper event handler.
        /// </summary>
        private void ResponseReady(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                // Check if correlation Id is valid.
                if (correlationIds.Contains(e.BasicProperties.CorrelationId))
                {
                    lock (correlationIds)
                    {
                        correlationIds.Remove(e.BasicProperties.CorrelationId);
                    }

                    // Converting from Json bytes.
                    var convertedMessage = MessageQueueCommonItems.DeserializeFromJsonBytes<TResponse>(e.Body);

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
                #region Adding Context Data
                var context = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(rabbitMqConfiguration.ExchangeName))
                {
                    context.Add(CommonContextKeys.ExchangeName, rabbitMqConfiguration.ExchangeName);
                }

                if (!string.IsNullOrEmpty(routingKey))
                {
                    context.Add(CommonContextKeys.RoutingKey, routingKey);
                }

                if (!string.IsNullOrEmpty(replyQueueName))
                {
                    context.Add(CommonContextKeys.ReplyQueueName, replyQueueName);
                }
                #endregion

                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToReceiveResponseMessage,
                    message: ErrorMessages.FailedToReceiveResponseMessage,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    context: context,
                    logger: logger);
            }
        }
        #endregion
    }
}
