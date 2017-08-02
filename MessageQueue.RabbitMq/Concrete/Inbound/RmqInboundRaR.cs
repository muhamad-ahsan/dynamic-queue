using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.Core.Abstract;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.RabbitMq.Abstract;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.Core.Abstract.Inbound;

namespace MessageQueue.RabbitMq.Concrete.Inbound
{
    /// <summary>
    /// RabbitMq based implementation of IInboundRaRMq.
    /// </summary>
    internal sealed class RmqInboundRaR<TRequest, TResponse> : BaseRabbitMq, IInboundRaRMq<TRequest, TResponse>
    {
        #region Private Methods
        private EventingBasicConsumer consumer;
        private volatile bool isReceivingMessages;
        #endregion

        #region Constructors
        public RmqInboundRaR(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, AmqpExchangeType.Direct, true, loggerObject);

                // Setting consumer object.
                consumer = new EventingBasicConsumer(model);
                consumer.Received += ReceiveReady;
                consumer.ConsumerTag = Guid.NewGuid().ToString("D");

                // Setting other fields.
                Address = rabbitMqConfiguration.Address;
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                #region Adding Context Data
                var context = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(rabbitMqConfiguration?.ExchangeName))
                {
                    context.Add(CommonContextKeys.ExchangeName, rabbitMqConfiguration?.ExchangeName);
                }

                if (!string.IsNullOrEmpty(rabbitMqConfiguration?.RoutingKey))
                {
                    context.Add(CommonContextKeys.RoutingKey, rabbitMqConfiguration?.RoutingKey);
                }
                #endregion

                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration?.QueueName,
                    address: rabbitMqConfiguration?.Address,
                    context: context,
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
                return model.MessageCount(rabbitMqConfiguration.QueueName) > 0;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueHasMessage,
                    message: ErrorMessages.FailedToCheckQueueHasMessage,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    logger: logger);
            }
        }

        public void StartReceivingRequest()
        {
            try
            {
                lock (consumer)
                {
                    if (!isReceivingMessages)
                    {
                        // Binding consumer with queue to start receiving messages.
                        model.BasicConsume(rabbitMqConfiguration.QueueName, !rabbitMqConfiguration.Acknowledgment, consumer);

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
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    logger: logger);
            }
        }

        public void StopReceivingRequest()
        {
            try
            {
                lock (consumer)
                {
                    if (isReceivingMessages)
                    {
                        model.BasicCancel(consumer.ConsumerTag);

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
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
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
        private void ReceiveReady(object sender, BasicDeliverEventArgs request)
        {
            try
            {
                // Converting from Json bytes.
                var convertedMessage = MessageQueueCommonItems.DeserializeFromJsonBytes<TRequest>(request.Body);

                // Getting other required fields for response.
                var replyProperties = model.CreateBasicProperties();
                replyProperties.CorrelationId = request.BasicProperties.CorrelationId;

                // Calling handler.
                OnRequestReady?.Invoke(new RmqRequestMessage<TRequest, TResponse>(
                    model,
                    replyProperties,
                    rabbitMqConfiguration.ExchangeName ?? string.Empty,
                    request.BasicProperties.ReplyTo,
                    rabbitMqConfiguration.Acknowledgment,
                    request.DeliveryTag,
                    convertedMessage,
                    ref logger));
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

                if (!string.IsNullOrEmpty(rabbitMqConfiguration.RoutingKey))
                {
                    context.Add(CommonContextKeys.RoutingKey, rabbitMqConfiguration.RoutingKey);
                }
                #endregion

                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToReceiveRequestMessage,
                    message: ErrorMessages.FailedToReceiveRequestMessage,
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
