using System;
using System.Linq;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.RabbitMq.Helper
{
    /// <summary>
    /// Contains common functionality.
    /// </summary>
    internal static class CommonItems
    {
        #region Public Data Members
        public const string RabbitMqName = "RabbitMq";
        #endregion

        #region Public Methods
        /// <summary>
        /// Helper method to validate and collect RabbitMq parameters.
        /// </summary>
        public static RabbitMqConfiguration CollectConfiguration(ref Dictionary<string, string> rawConfiguration, bool isInbound, ref IQueueLogger logger)
        {
            try
            {

                #region Parameters Validation
                var rabbitMqConfiguration = new RabbitMqConfiguration();
                rawConfiguration = rawConfiguration ?? new Dictionary<string, string>();

                var notSupportedParams =
                    rawConfiguration.Keys.Where(x => !CommonConfigurationKeys.GetAllKeys().Contains(x) &&
                                                     !RabbitMqConfigurationKeys.GetAllKeys().Contains(x)).ToList();

                if (notSupportedParams.Any())
                {
                    throw new QueueException(QueueErrorCode.NotSupportedConfigurationParameters,
                        ErrorMessages.NotSupportedConfigurationParameters, context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.NotSupportedParameters] = string.Join(",", notSupportedParams)
                        });
                }

                // Address
                if (!rawConfiguration.ContainsKey(CommonConfigurationKeys.Address) ||
                    string.IsNullOrWhiteSpace(rawConfiguration[CommonConfigurationKeys.Address]))
                {
                    throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                        string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                            CommonConfigurationKeys.Address),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = CommonConfigurationKeys.Address
                        });
                }

                rabbitMqConfiguration.Address = rawConfiguration[CommonConfigurationKeys.Address];

                // Port
                int port = 0;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.Port) &&
                    !int.TryParse(rawConfiguration[RabbitMqConfigurationKeys.Port], out port))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.Port),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.Port
                        });
                }

                rabbitMqConfiguration.Port = port;

                // UserName
                if (!rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.UserName) ||
                    string.IsNullOrWhiteSpace(rawConfiguration[RabbitMqConfigurationKeys.UserName]))
                {
                    throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                        string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                            RabbitMqConfigurationKeys.UserName),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.UserName
                        });
                }

                rabbitMqConfiguration.UserName = rawConfiguration[RabbitMqConfigurationKeys.UserName];

                // Password
                if (!rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.Password) ||
                    string.IsNullOrWhiteSpace(rawConfiguration[RabbitMqConfigurationKeys.Password]))
                {
                    throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                        string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                            RabbitMqConfigurationKeys.Password),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.Password
                        });
                }

                rabbitMqConfiguration.Password = rawConfiguration[RabbitMqConfigurationKeys.Password];

                // Queue Name
                if (!rawConfiguration.ContainsKey(CommonConfigurationKeys.QueueName) ||
                    string.IsNullOrWhiteSpace(rawConfiguration[CommonConfigurationKeys.QueueName]))
                {
                    throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                        string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                            CommonConfigurationKeys.QueueName),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = CommonConfigurationKeys.QueueName
                        });
                }

                rabbitMqConfiguration.QueueName = rawConfiguration[CommonConfigurationKeys.QueueName];

                // Exchange Name
                rabbitMqConfiguration.ExchangeName = null;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.ExchangeName))
                {
                    if (string.IsNullOrWhiteSpace(rawConfiguration[RabbitMqConfigurationKeys.ExchangeName]))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                RabbitMqConfigurationKeys.ExchangeName),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.ExchangeName
                            });
                    }

                    rabbitMqConfiguration.ExchangeName = rawConfiguration[RabbitMqConfigurationKeys.ExchangeName];
                }

                // Routing Key
                rabbitMqConfiguration.RoutingKey = null;

                if (rabbitMqConfiguration.ExchangeName == null &&
                    rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.RoutingKey))
                {
                    throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                        string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                            RabbitMqConfigurationKeys.RoutingKey),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.RoutingKey
                        });
                }

                if (rabbitMqConfiguration.ExchangeName != null)
                {
                    if (!rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.RoutingKey))
                    {
                        throw new QueueException(QueueErrorCode.ParameterRequiredInCurrentConfiguration,
                            string.Format(ErrorMessages.ParameterRequiredInCurrentConfiguration,
                                RabbitMqConfigurationKeys.RoutingKey),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.RoutingKey
                            });
                    }

                    rabbitMqConfiguration.RoutingKey = rawConfiguration[RabbitMqConfigurationKeys.RoutingKey];
                }

                // Durable Exchange
                var durableExchange = false;

                if (rabbitMqConfiguration.ExchangeName == null &&
                    rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.DurableExchange))
                {
                    throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                        string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                            RabbitMqConfigurationKeys.DurableExchange),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.DurableExchange
                        });
                }

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.DurableExchange) &&
                    !bool.TryParse(rawConfiguration[RabbitMqConfigurationKeys.DurableExchange], out durableExchange))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.DurableExchange),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.DurableExchange
                        });
                }

                rabbitMqConfiguration.DurableExchange = durableExchange;

                // Durable Queue
                var durableQueue = false;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.DurableQueue) &&
                    !bool.TryParse(rawConfiguration[RabbitMqConfigurationKeys.DurableQueue], out durableQueue))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.DurableQueue),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.DurableQueue
                        });
                }

                rabbitMqConfiguration.DurableQueue = durableQueue;

                // Durable Message
                var durableMessage = false;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.DurableMessage) &&
                    !bool.TryParse(rawConfiguration[RabbitMqConfigurationKeys.DurableMessage], out durableMessage))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.DurableMessage),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.DurableMessage
                        });
                }

                rabbitMqConfiguration.DurableMessage = durableMessage;

                // Acknowledgment
                var acknowledgment = false;

                if (isInbound == false && rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.Acknowledgment))
                {
                    throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                        string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                            RabbitMqConfigurationKeys.Acknowledgment),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.Acknowledgment
                        });
                }

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.Acknowledgment) &&
                    !bool.TryParse(rawConfiguration[RabbitMqConfigurationKeys.Acknowledgment], out acknowledgment))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.Acknowledgment),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.Acknowledgment
                        });
                }

                rabbitMqConfiguration.Acknowledgment = acknowledgment;

                // MaxConcurrentReceiveCallback
                ushort maxConcurrentReceiveCallback;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback))
                {
                    if (isInbound == false)
                    {
                        throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                            string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                                RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback
                            });
                    }

                    if (!ushort.TryParse(rawConfiguration[RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback],
                        out maxConcurrentReceiveCallback))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.MaxConcurrentReceiveCallback
                            });
                    }
                }
                else
                {
                    maxConcurrentReceiveCallback = Defaults.MaxConcurrentReceiveCallback;
                }

                rabbitMqConfiguration.MaxConcurrentReceiveCallback = maxConcurrentReceiveCallback;

                // Connection Timeout
                var connectionTimeout = Defaults.RabbitMqConnectionTimeoutInMinutes;

                if (rawConfiguration.ContainsKey(RabbitMqConfigurationKeys.ConnectionTimeoutInMinutes) &&
                    !int.TryParse(rawConfiguration[RabbitMqConfigurationKeys.ConnectionTimeoutInMinutes],
                        out connectionTimeout))
                {
                    throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                        string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                            RabbitMqConfigurationKeys.ConnectionTimeoutInMinutes),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = RabbitMqConfigurationKeys.ConnectionTimeoutInMinutes
                        });
                }

                rabbitMqConfiguration.ConnectionTimeoutInMinutes = connectionTimeout;
                #endregion

                #region Return
                return rabbitMqConfiguration;
                #endregion

            }
            catch (QueueException queueException)
            {
                #region Adding Context Data
                queueException.Data.Add(CommonContextKeys.QueueContext, RabbitMqName);
                #endregion

                #region Logging - Error
                logger.Error(queueException, queueException.Message);
                #endregion

                throw;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    QueueErrorCode.GeneralConfigurationParsingError
                    , ErrorMessages.GeneralConfigurationParsingError, ex, RabbitMqName, logger: logger);
            }
        }
        #endregion
    }
}
