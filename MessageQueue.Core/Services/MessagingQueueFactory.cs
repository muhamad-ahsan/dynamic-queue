using System;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.Core.Abstract.Inbound;
using MessageQueue.Core.Abstract.Outbound;
using MessageQueue.CofigurationProvider.Core.Abstract;

namespace MessageQueue.Core.Services
{
    /// <summary>
    /// Message queue factory to create different implementation of queues 
    /// based on the communication pattern (Push-Pull, Request-Response etc.).
    /// </summary>
    public static class MessagingQueueFactory
    {
        #region Public Methods
        /// <summary>
        /// Creates IOutboundFaFMq implementation with the configuration provided by provider.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        /// <param name="logger">The queue logger</param>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        public static IOutboundFaFMq<TMessage> CreateOutboundFaF<TMessage>(IQueueConfigurationProvider configurationProvider, string configurationIdentifier, IQueueLogger logger = null)
        {
            #region Initialization
            logger = logger ?? new NoLog();
            Dictionary<string, string> configuration = null;
            #endregion

            try
            {
                #region Configuration Retrieval
                configuration = RetrieveAndValidateConfiguration(configurationProvider, configurationIdentifier);
                #endregion

                #region Creating Implementation
                // Getting type details.
                var messageQueueTypeInfo = Type.GetType(configuration[CommonConfigurationKeys.Implementation], true, true);

                // Substituting generic parameters.
                var typeParams = new[] {typeof(TMessage)};
                var genericTypeInfo = messageQueueTypeInfo.MakeGenericType(typeParams);

                // Creating instance.
                var messagingQueue =
                    Activator.CreateInstance(genericTypeInfo, configuration, logger) as IOutboundFaFMq<TMessage>;
                #endregion

                #region Return
                return messagingQueue;
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
                var queueException = new QueueException(QueueErrorCode.FailedToInstantiateOutboundFaFMq,
                    ErrorMessages.FailedToInstantiateOutboundFaF, ex);

                queueException.Data["Implementation"] = configuration?[CommonConfigurationKeys.Implementation];

                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw queueException;
            }
        }

        /// <summary>
        /// Creates IInboundFaFMq implementation with the configuration provided by provider.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        /// <param name="logger">The queue logger</param>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        public static IInboundFaFMq<TMessage> CreateInboundFaF<TMessage>(IQueueConfigurationProvider configurationProvider, string configurationIdentifier, IQueueLogger logger = null)
        {
            #region Initialization
            logger = logger ?? new NoLog();
            Dictionary<string, string> configuration = null;
            #endregion

            try
            {
                #region Configuration Retrieval
                configuration = RetrieveAndValidateConfiguration(configurationProvider, configurationIdentifier);
                #endregion

                #region Creating Implementation
                // Getting type details.
                var messageQueueTypeInfo = Type.GetType(configuration[CommonConfigurationKeys.Implementation], true, true);

                // Substituting generic parameters.
                var typeParams = new[] { typeof(TMessage) };
                var genericTypeInfo = messageQueueTypeInfo.MakeGenericType(typeParams);

                // Creating instance.
                var messagingQueue = Activator.CreateInstance(genericTypeInfo, configuration, logger) as IInboundFaFMq<TMessage>;
                #endregion

                #region Return
                return messagingQueue;
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
                var queueException = new QueueException(QueueErrorCode.FailedToInstantiateInboundFaFMq,
                    ErrorMessages.FailedToInstantiateInboundFaFMq, ex);

                queueException.Data["Implementation"] = configuration?[CommonConfigurationKeys.Implementation];

                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw queueException;
            }
        }

        /// <summary>
        /// Creates IOutboundRaRMq implementation with the configuration provided by provider.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        /// <param name="logger">The queue logger</param>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        public static IOutboundRaRMq<TResponse, TMessage> CreateOutboundRaR<TResponse, TMessage>(IQueueConfigurationProvider configurationProvider, string configurationIdentifier, IQueueLogger logger = null)
        {
            #region Initialization
            logger = logger ?? new NoLog();
            Dictionary<string, string> configuration = null;
            #endregion

            try
            {
                #region Configuration Retrieval
                configuration = RetrieveAndValidateConfiguration(configurationProvider, configurationIdentifier);
                #endregion

                #region Creating Implementation
                // Getting type details.
                var messageQueueTypeInfo = Type.GetType(configuration[CommonConfigurationKeys.Implementation], true, true);

                // Substituting generic parameters.
                var typeParams = new[] { typeof(TResponse), typeof(TMessage) };
                var genericTypeInfo = messageQueueTypeInfo.MakeGenericType(typeParams);

                // Creating instance.
                var messagingQueue = Activator.CreateInstance(genericTypeInfo, configuration, logger) as IOutboundRaRMq<TResponse, TMessage>;
                #endregion

                #region Return
                return messagingQueue;
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
                var queueException = new QueueException(QueueErrorCode.FailedToInstantiateOutboundRaRMq,
                    ErrorMessages.FailedToInstantiateOutboundRaRMq, ex);

                queueException.Data["Implementation"] = configuration?[CommonConfigurationKeys.Implementation];

                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw queueException;
            }
        }

        /// <summary>
        /// Creates IOutboundRaRMq implementation with the configuration provided by provider.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        /// <param name="logger">The queue logger</param>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        public static IInboundRaRMq<TRequest, TResponse> CreateInboundRaR<TRequest, TResponse>(IQueueConfigurationProvider configurationProvider, string configurationIdentifier, IQueueLogger logger = null)
        {
            #region Initialization
            logger = logger ?? new NoLog();
            Dictionary<string, string> configuration = null;
            #endregion

            try
            {
                #region Configuration Retrieval
                configuration = RetrieveAndValidateConfiguration(configurationProvider, configurationIdentifier);
                #endregion

                #region Creating Implementation
                // Getting type details.
                var messageQueueTypeInfo = Type.GetType(configuration[CommonConfigurationKeys.Implementation], true, true);

                // Substituting generic parameters.
                var typeParams = new[] { typeof(TRequest), typeof(TResponse) };
                var genericTypeInfo = messageQueueTypeInfo.MakeGenericType(typeParams);

                // Creating instance.
                var messagingQueue = Activator.CreateInstance(genericTypeInfo, configuration, logger) as IInboundRaRMq<TRequest, TResponse>;
                #endregion

                #region Return
                return messagingQueue;
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
                var queueException = new QueueException(QueueErrorCode.FailedToInstantiateInboundRaRMq,
                    ErrorMessages.FailedToInstantiateInboundRaRMq, ex);

                queueException.Data["Implementation"] = configuration?[CommonConfigurationKeys.Implementation];

                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw queueException;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper method to retrieve and validate configuration.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        private static Dictionary<string, string> RetrieveAndValidateConfiguration(IQueueConfigurationProvider configurationProvider, string configurationIdentifier)
        {
            #region Initialization
            Dictionary<string, string> configuration = null;
            #endregion

            #region Configuration Retrieval
            configuration = configurationProvider.GetConfiguration(configurationIdentifier);
            #endregion

            #region Validation
            configuration = configuration ?? new Dictionary<string, string>();

            // Implementation
            if (!configuration.ContainsKey(CommonConfigurationKeys.Implementation) ||
                string.IsNullOrEmpty(configuration[CommonConfigurationKeys.Implementation]))
            {
                throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                    string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                        CommonConfigurationKeys.Implementation));
            }
            #endregion

            #region Return
            return configuration;
            #endregion
        }
        #endregion
    }
}
