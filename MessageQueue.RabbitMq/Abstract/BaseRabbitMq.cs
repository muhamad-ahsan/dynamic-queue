using System;
using RabbitMQ.Client;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using RabbitMQ.Client.Exceptions;
using MessageQueue.Core.Properties;
using MessageQueue.RabbitMq.Helper;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.RabbitMq.Concrete;

namespace MessageQueue.RabbitMq.Abstract
{
    /// <summary>
    /// Base class for all RabbitMq based implementation of interfaces.
    /// </summary>
    internal abstract class BaseRabbitMq
    {
        #region Private Data Members
        private bool isInitialized;
        #endregion

        #region Protected Properties
        protected IModel model;
        protected IQueueLogger logger;
        protected IConnection connection;
        protected IBasicProperties modelProperties;
        protected RabbitMqConfiguration rabbitMqConfiguration;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Common initialization code.
        /// </summary>
        protected virtual void Initialize(Dictionary<string, string> configuration, AmqpExchangeType exchangeType, bool isInbound, IQueueLogger loggerObject = null)
        {
            try
            {
                #region Logger Initialization
                logger = loggerObject;
                #endregion

                #region Parameters Collection
                rabbitMqConfiguration = CommonItems.CollectConfiguration(ref configuration, isInbound, ref logger);
                #endregion

                #region Initializing Connection
                var connectionFactory = GetRmqConnectionFactory();
                connection = connectionFactory.CreateConnection();
                #endregion

                #region Initializing Queue
                model = connection.CreateModel();

                if (!IsQueueExistsHelper())
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.QueueDoesNotExist,
                        message: ErrorMessages.QueueDoesNotExist, 
                        innerException: null,
                        queueContext: CommonItems.RabbitMqName,
                        queueName: rabbitMqConfiguration.QueueName,
                        address: rabbitMqConfiguration.Address,
                        logger: logger);
                }

                if (!IsExchangeExistsHelper())
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.ExchangeDoesNotExist,
                        message: ErrorMessages.ExchangeDoesNotExist,
                        innerException: null,
                        queueContext: CommonItems.RabbitMqName,
                        address: rabbitMqConfiguration.Address,
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ExchangeName] = rabbitMqConfiguration.ExchangeName
                        },
                        logger: logger);
                }

                // Binding queue with exchange if configured.
                if (!string.IsNullOrWhiteSpace(rabbitMqConfiguration.ExchangeName))
                {
                    model.QueueBind(rabbitMqConfiguration.QueueName, rabbitMqConfiguration.ExchangeName, rabbitMqConfiguration.RoutingKey ?? string.Empty);
                }

                if (isInbound)
                {
                    model.BasicQos(0, rabbitMqConfiguration.MaxConcurrentReceiveCallback, false);
                }

                //  Setting model properties.
                modelProperties = model.CreateBasicProperties();
                modelProperties.Persistent = rabbitMqConfiguration.DurableMessage;
                #endregion

                #region Registering Events
                model.BasicReturn += OnBasicReturn;
                model.ModelShutdown += OnModelShutdown;
                connection.ConnectionShutdown += OnConnectionShutdown;
                #endregion

                #region Updating Flag
                isInitialized = true;
                #endregion

                #region Cleaning up Sensitive Info
                rabbitMqConfiguration.Password = null;
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    logger: logger);
            }
        }

        /// <summary>
        /// Method to check if the queue exits.
        /// </summary>
        protected virtual bool IsQueueExists()
        {
            #region Validation
            CheckIfQueueHasInitialized();
            #endregion

            #region Checking Queue Existence
            return IsQueueExistsHelper();
            #endregion
        }

        /// <summary>
        /// Method to check if the exchange exits.
        /// </summary>
        protected virtual bool IsExchangeExists()
        {
            #region Validation
            CheckIfQueueHasInitialized();
            #endregion

            #region Checking Exchange Existence
            return IsExchangeExistsHelper();
            #endregion
        }

        /// <summary>
        /// Method to create the queue.
        /// </summary>
        protected virtual QueueDeclareOk CreateQueue()
        {
            #region Validation
            CheckIfQueueHasInitialized();
            #endregion

            #region Initialization
            QueueDeclareOk queueDeclareOk;
            #endregion

            #region Creating Queue
            try
            {
                queueDeclareOk = model.QueueDeclare(rabbitMqConfiguration.QueueName, rabbitMqConfiguration.DurableQueue, false, false, null);
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCreateMessageQueue,
                    message: ErrorMessages.FailedToCreateMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration.QueueName,
                    address: rabbitMqConfiguration.Address,
                    logger: logger);
            }
            #endregion

            #region Return
            return queueDeclareOk;
            #endregion
        }

        /// <summary>
        /// Helper method to create the exchange.
        /// </summary>
        protected virtual void CreateExchange(AmqpExchangeType exchangeType)
        {
            #region Validation
            CheckIfQueueHasInitialized();
            #endregion

            #region Creating Exchange
            try
            {
                model.ExchangeDeclare(rabbitMqConfiguration.ExchangeName, exchangeType.ToString(), rabbitMqConfiguration.DurableExchange);
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCreateExchange,
                    message: ErrorMessages.FailedToCreateExchange,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    address: rabbitMqConfiguration.Address,
                    context: new Dictionary<string, string>
                    {
                        [CommonContextKeys.ExchangeName] = rabbitMqConfiguration.ExchangeName
                    },
                    logger: logger);
            }
            #endregion
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Event handler for connection shutdown.
        /// </summary>
        protected virtual void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        /// <summary>
        /// Event handler for model shutdown.
        /// </summary>
        protected virtual void OnModelShutdown(object sender, ShutdownEventArgs e)
        {
        }

        /// <summary>
        /// Event handler for message return.
        /// </summary>
        protected virtual void OnBasicReturn(object sender, RabbitMQ.Client.Events.BasicReturnEventArgs e)
        {
            #region Logging - Error
            MessageQueueCommonItems.PrepareAndLogQueueException(
                errorCode: QueueErrorCode.MessageReturnedFromQueue,
                message: ErrorMessages.MessageReturnedFromQueue,
                innerException: null,
                queueContext: CommonItems.RabbitMqName,
                queueName: rabbitMqConfiguration?.QueueName,
                address: rabbitMqConfiguration?.Address,
                context: new Dictionary<string, string>
                {
                    [CommonContextKeys.ExchangeName] = e?.Exchange,
                    [CommonContextKeys.RoutingKey] = e?.RoutingKey,
                    [ContextKeys.ReplyCode] = e?.ReplyCode.ToString(),
                    [ContextKeys.ReplyText] = e?.ReplyText
                },
                logger: logger);
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper method to check if queue has been initialized.
        /// </summary>
        private void CheckIfQueueHasInitialized()
        {
            if (!isInitialized)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.MessageQueueIsNotInitialized,
                    message: ErrorMessages.MessageQueueIsNotInitialized,
                    innerException: null,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration?.QueueName,
                    address: rabbitMqConfiguration?.Address,
                    logger: logger);
            }
        }

        /// <summary>
        /// Helper method to create connection factory for RabbitMq.
        /// </summary>
        private ConnectionFactory GetRmqConnectionFactory()
        {
            #region Initialization
            var connectionFactory = new ConnectionFactory
            {
                HostName = rabbitMqConfiguration.Address,
                UserName = rabbitMqConfiguration.UserName,
                Password = rabbitMqConfiguration.Password,
                ContinuationTimeout = TimeSpan.FromMinutes(rabbitMqConfiguration.ConnectionTimeoutInMinutes),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(Defaults.RabbitMqNetworkRecoveryIntervalInSeconds)
            };

            if (rabbitMqConfiguration.Port > 0)
            {
                connectionFactory.Port = rabbitMqConfiguration.Port;
            }
            #endregion

            #region Return
            return connectionFactory;
            #endregion
        }

        /// <summary>
        /// Helper method to check if queue exists.
        /// </summary>
        private bool IsQueueExistsHelper()
        {
            #region Checking Queue Existence
            try
            {
                if (!string.IsNullOrWhiteSpace(rabbitMqConfiguration.QueueName))
                {
                    try
                    {
                        model.QueueDeclarePassive(rabbitMqConfiguration.QueueName);
                    }
                    catch (Exception ex)
                    {
                        var exceptionInfo = ex as OperationInterruptedException;

                        if (exceptionInfo?.ShutdownReason.ReplyCode == 404)
                        {
                            return false;
                        }

                        throw;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueExistence,
                    message: ErrorMessages.FailedToCheckQueueExistence,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    queueName: rabbitMqConfiguration?.QueueName,
                    address: rabbitMqConfiguration?.Address,
                    logger: logger);
            }
            #endregion
        }

        /// <summary>
        /// Helper method to if exchange exists.
        /// </summary>
        private bool IsExchangeExistsHelper()
        {
            #region Checking Exchange Existence
            try
            {
                if (!string.IsNullOrWhiteSpace(rabbitMqConfiguration.ExchangeName))
                {
                    try
                    {
                        model.ExchangeDeclarePassive(rabbitMqConfiguration.ExchangeName);
                    }
                    catch (Exception ex)
                    {
                        var exceptionInfo = ex as OperationInterruptedException;

                        if (exceptionInfo?.ShutdownReason.ReplyCode == 404)
                        {
                            return false;
                        }

                        throw;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckExchangeExistence,
                    message: ErrorMessages.FailedToCheckExchangeExistence,
                    innerException: ex,
                    queueContext: CommonItems.RabbitMqName,
                    address: rabbitMqConfiguration?.Address,
                    context: new Dictionary<string, string>
                    {
                        [CommonContextKeys.ExchangeName] = rabbitMqConfiguration?.ExchangeName
                    },
                    logger: logger);
            }
            #endregion
        }
        #endregion
    }
}
