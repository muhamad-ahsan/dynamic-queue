using System;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using System.Collections.Generic;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.ServiceBus.Concrete;

namespace MessageQueue.ServiceBus.Helper
{
    /// <summary>
    /// Contains common functionality.
    /// </summary>
    internal static class CommonItems
    {
        #region Public Data Members
        public const string ServiceBusName = "ServiceBus";
        public const string SbConnectionStringAddressPartName = "Endpoint";
        #endregion

        #region Public Methods

        /// <summary>
        /// Helper method to validate and collect ServiceBus parameters.
        /// </summary>
        internal static ServiceBusConfiguration CollectSbConfiguration(ref Dictionary<string, string> rawConfiguration, bool isInbound, ref IQueueLogger logger)
        {
            try
            {
                #region Initialization
                var serviceBusConfiguration = new ServiceBusConfiguration();
                rawConfiguration = rawConfiguration ?? new Dictionary<string, string>();
                #endregion

                #region Collecting Common Configuration
                MessageQueueCommonItems.CollectCommonConfiguration(ref rawConfiguration, serviceBusConfiguration, ServiceBusConfigurationKeys.GetAllKeys());
                #endregion

                #region Collecting Other Configuration
                // Namespace Address
                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.NamespaceAddress))
                {
                    if (string.IsNullOrWhiteSpace(rawConfiguration[ServiceBusConfigurationKeys.NamespaceAddress]))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.NamespaceAddress),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.NamespaceAddress
                            });
                    }

                    serviceBusConfiguration.NamespaceAddress = rawConfiguration[ServiceBusConfigurationKeys.NamespaceAddress];
                }

                // Queue Name
                if (!rawConfiguration.ContainsKey(CommonConfigurationKeys.QueueName) || string.IsNullOrWhiteSpace(rawConfiguration[CommonConfigurationKeys.QueueName]))
                {
                    throw new QueueException(QueueErrorCode.MissingRequiredConfigurationParameter,
                        string.Format(ErrorMessages.MissingRequiredConfigurationParameter,
                            CommonConfigurationKeys.QueueName),
                        context: new Dictionary<string, string>
                        {
                            [CommonContextKeys.ParameterName] = CommonConfigurationKeys.QueueName
                        });
                }

                serviceBusConfiguration.QueueName = rawConfiguration[CommonConfigurationKeys.QueueName];

                // MaxDeliveryCount
                short maxDeliveryCount;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.MaxDeliveryCount))
                {
                    if (!short.TryParse(rawConfiguration[ServiceBusConfigurationKeys.MaxDeliveryCount], out maxDeliveryCount))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.MaxDeliveryCount),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.MaxDeliveryCount
                            });
                    }
                }
                else
                {
                    maxDeliveryCount = Defaults.MaxDeliveryCount;
                }

                serviceBusConfiguration.MaxDeliveryCount = maxDeliveryCount;

                // MaxSizeInMegabytes
                long maxSizeInMegabytes;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.MaxSizeInMegabytes))
                {
                    if (!long.TryParse(rawConfiguration[ServiceBusConfigurationKeys.MaxSizeInMegabytes], out maxSizeInMegabytes))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.MaxSizeInMegabytes),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.MaxSizeInMegabytes
                            });
                    }
                }
                else
                {
                    maxSizeInMegabytes = Defaults.MaxSizeInMegabytes;
                }

                serviceBusConfiguration.MaxSizeInMegabytes = maxSizeInMegabytes;

                // EnableDeadLettering
                bool enableDeadLettering;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.EnableDeadLettering))
                {
                    if (!bool.TryParse(rawConfiguration[ServiceBusConfigurationKeys.EnableDeadLettering], out enableDeadLettering))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.EnableDeadLettering),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.EnableDeadLettering
                            });
                    }
                }
                else
                {
                    enableDeadLettering = Defaults.EnableDeadLettering;
                }

                serviceBusConfiguration.EnableDeadLettering = enableDeadLettering;

                // EnablePartitioning
                bool enablePartitioning;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.EnablePartitioning))
                {
                    if (!bool.TryParse(rawConfiguration[ServiceBusConfigurationKeys.EnablePartitioning], out enablePartitioning))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.EnablePartitioning),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.EnablePartitioning
                            });
                    }
                }
                else
                {
                    enablePartitioning = Defaults.EnablePartitioning;
                }

                serviceBusConfiguration.EnablePartitioning = enablePartitioning;

                // RequiresDuplicateDetection
                bool requiresDuplicateDetection;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.RequiresDuplicateDetection))
                {
                    if (!bool.TryParse(rawConfiguration[ServiceBusConfigurationKeys.RequiresDuplicateDetection], out requiresDuplicateDetection))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.RequiresDuplicateDetection),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.RequiresDuplicateDetection
                            });
                    }
                }
                else
                {
                    requiresDuplicateDetection = Defaults.RequiresDuplicateDetection;
                }

                serviceBusConfiguration.RequiresDuplicateDetection = requiresDuplicateDetection;

                // EnableBatchedOperations
                bool enableBatchedOperations;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.EnableBatchedOperations))
                {
                    if (!bool.TryParse(rawConfiguration[ServiceBusConfigurationKeys.EnableBatchedOperations], out enableBatchedOperations))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.EnableBatchedOperations),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.EnableBatchedOperations
                            });
                    }
                }
                else
                {
                    enableBatchedOperations = Defaults.EnableBatchedOperations;
                }

                serviceBusConfiguration.EnableBatchedOperations = enableBatchedOperations;

                // MessageTimeToLiveInMinutes
                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.MessageTimeToLiveInMinutes))
                {
                    int messageTimeToLiveInMinutes;

                    if (!int.TryParse(rawConfiguration[ServiceBusConfigurationKeys.MessageTimeToLiveInMinutes], out messageTimeToLiveInMinutes))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.MessageTimeToLiveInMinutes),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.MessageTimeToLiveInMinutes
                            });
                    }

                    serviceBusConfiguration.MessageTimeToLiveInMinutes = TimeSpan.FromMinutes(messageTimeToLiveInMinutes);
                }
                else
                {
                    serviceBusConfiguration.MessageTimeToLiveInMinutes = Defaults.MessageTimeToLive;
                }

                // LockDurationInSeconds
                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.LockDurationInSeconds))
                {
                    int lockDurationInSeconds;

                    if (!int.TryParse(rawConfiguration[ServiceBusConfigurationKeys.LockDurationInSeconds], out lockDurationInSeconds))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.LockDurationInSeconds),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.LockDurationInSeconds
                            });
                    }

                    serviceBusConfiguration.LockDurationInSeconds = TimeSpan.FromSeconds(lockDurationInSeconds);
                }
                else
                {
                    serviceBusConfiguration.LockDurationInSeconds = Defaults.LockDurationInSeconds;
                }

                // Acknowledgment
                bool acknowledgment;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.Acknowledgment))
                {
                    if (isInbound == false)
                    {
                        throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                            string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                                ServiceBusConfigurationKeys.Acknowledgment),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.Acknowledgment
                            });
                    }

                    if (!bool.TryParse(rawConfiguration[ServiceBusConfigurationKeys.Acknowledgment], out acknowledgment))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.Acknowledgment),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.Acknowledgment
                            });
                    }
                }
                else
                {
                    acknowledgment = Defaults.Acknowledgment;
                }

                serviceBusConfiguration.Acknowledgment = acknowledgment;

                // MaxConcurrentReceiveCallback
                ushort maxConcurrentReceiveCallback;

                if (rawConfiguration.ContainsKey(ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback))
                {
                    if (isInbound == false)
                    {
                        throw new QueueException(QueueErrorCode.ParameterNotApplicationInCurrentConfiguration,
                            string.Format(ErrorMessages.ParameterNotApplicationInCurrentConfiguration,
                                ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback
                            });
                    }

                    if (!ushort.TryParse(rawConfiguration[ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback], out maxConcurrentReceiveCallback))
                    {
                        throw new QueueException(QueueErrorCode.InvalidValueForConfigurationParameter,
                            string.Format(ErrorMessages.InvalidValueForConfigurationParameter,
                                ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback),
                            context: new Dictionary<string, string>
                            {
                                [CommonContextKeys.ParameterName] = ServiceBusConfigurationKeys.MaxConcurrentReceiveCallback
                            });
                    }
                }
                else
                {
                    maxConcurrentReceiveCallback = Defaults.MaxConcurrentReceiveCallback;
                }

                serviceBusConfiguration.MaxConcurrentReceiveCallback = maxConcurrentReceiveCallback;
                #endregion

                #region Return
                return serviceBusConfiguration;
                #endregion
            }
            catch (QueueException queueException)
            {
                #region Adding Context Data
                queueException.Data.Add(CommonContextKeys.QueueContext, ServiceBusName);
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
                    , ErrorMessages.GeneralConfigurationParsingError, ex, ServiceBusName, logger: logger);
            }
        }
        #endregion
    }
}
