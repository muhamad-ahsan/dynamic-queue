using System;
using RabbitMQ.Client;
using System.Threading.Tasks;
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
    /// RabbitMq based implementation of IOutboundFaFMq.
    /// </summary>
    internal sealed class RmqOutboundFaF<TMessage> : BaseRabbitMq, IOutboundFaFMq<TMessage>
    {
        #region Private Data Members
        private string exchangeName, routingKey;
        private object lockForQueueOperation = new object();
        #endregion

        #region Constructors
        public RmqOutboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, AmqpExchangeType.Direct, false, loggerObject);

                // Setting other fields.
                Address = rabbitMqConfiguration.Address;
                exchangeName = rabbitMqConfiguration.ExchangeName ?? string.Empty;
                routingKey = rabbitMqConfiguration.RoutingKey ?? rabbitMqConfiguration.QueueName;
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

        #region IOutboundFaFMq Implementation
        public string Address { get; private set; }

        public void SendMessage(TMessage message)
        {
            try
            {
                #region Sending Message
                lock (lockForQueueOperation)
                {
                    model.BasicPublish(exchangeName, routingKey, modelProperties, MessageQueueCommonItems.SerializeToJsonBytes(message));
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
    }
}
