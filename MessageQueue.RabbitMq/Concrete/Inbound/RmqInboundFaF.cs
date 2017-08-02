using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Abstract;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.RabbitMq.Abstract;
using MessageQueue.Core.Abstract.Inbound;

namespace MessageQueue.RabbitMq.Concrete.Inbound
{
    /// <summary>
    /// RabbitMq based implementation of IInboundFaFMq.
    /// </summary>
    internal sealed class RmqInboundFaF<TMessage> : BaseRabbitMq, IInboundFaFMq<TMessage>
    {
        #region Private Methods
        private EventingBasicConsumer consumer;
        private volatile bool isReceivingMessages;
        #endregion

        #region Constructors
        public RmqInboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
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

        #region IInboundFaFMq Implementation
        public string Address { get; }

        public event Action<TMessage, IMessageReceiveOptions> OnMessageReady;
        public event Func<TMessage, IMessageReceiveOptions, Task> OnMessageReadyAsync;

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

        public void StartReceivingMessage()
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
                    errorCode: QueueErrorCode.FailedToStartReceivingMessage,
                    message: ErrorMessages.FailedToStartReceivingMessage,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    logger: logger);
            }
        }

        public void StopReceivingMessage()
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
                    errorCode: QueueErrorCode.FailedToStopReceivingMessage,
                    message: ErrorMessages.FailedToStopReceivingMessage,
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
        private async void ReceiveReady(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                // Converting from Json bytes.
                var convertedMessage = MessageQueueCommonItems.DeserializeFromJsonBytes<TMessage>(e.Body);
                var messageReceiveOptions = new RmqMessageReceiveOptions(model, e.DeliveryTag, rabbitMqConfiguration.QueueName, rabbitMqConfiguration.Acknowledgment, ref logger);

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
                    errorCode: QueueErrorCode.FailedToReceiveMessage,
                    message: ErrorMessages.FailedToReceiveMessage,
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
