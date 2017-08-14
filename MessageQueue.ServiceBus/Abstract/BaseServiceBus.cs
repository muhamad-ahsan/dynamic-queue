using System;
using System.Linq;
using Microsoft.ServiceBus;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.ServiceBus.Helper;
using Microsoft.ServiceBus.Messaging;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.ServiceBus.Concrete;

namespace MessageQueue.ServiceBus.Abstract
{
    /// <summary>
    /// Base class for all ServiceBus classes.
    /// </summary>
    internal abstract class BaseServiceBus
    {
        #region Private Data Members
        private bool isInitialized = false;
        private NamespaceManager namespaceManager;
        #endregion

        #region Protected Properties
        protected IQueueLogger logger;
        protected QueueClient queueClient;
        protected ServiceBusConfiguration sbConfiguration;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Common initialization code.
        /// </summary>
        protected virtual void Initialize(Dictionary<string, string> configuration, bool isInbound, IQueueLogger loggerObject = null)
        {
            try
            {
                #region Logger Initialization
                logger = loggerObject;
                #endregion

                #region Parameters Collection
                sbConfiguration = CommonItems.CollectSbConfiguration(ref configuration, isInbound, ref logger);
                #endregion

                #region Initializing Queue
                queueClient = QueueClient.CreateFromConnectionString(sbConfiguration.Address);

                if (!string.IsNullOrWhiteSpace(sbConfiguration.NamespaceAddress))
                {
                    namespaceManager = NamespaceManager.CreateFromConnectionString(sbConfiguration.NamespaceAddress);
                }

                // Updating address field to remove confidential data.
                sbConfiguration.Address = sbConfiguration.Address?.Split(';')?.FirstOrDefault(x => x.StartsWith(CommonItems.SbConnectionStringAddressPartName, StringComparison.InvariantCultureIgnoreCase));
                sbConfiguration.NamespaceAddress = sbConfiguration.NamespaceAddress?.Split(';')?.FirstOrDefault(x => x.StartsWith(CommonItems.SbConnectionStringAddressPartName, StringComparison.InvariantCultureIgnoreCase));

                if (!IsQueueExistsHelper())
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.QueueDoesNotExist,
                        message: ErrorMessages.QueueDoesNotExist,
                        innerException: null,
                        queueContext: CommonItems.ServiceBusName,
                        queueName: sbConfiguration.QueueName,
                        address: sbConfiguration.Address,
                        logger: logger);
                }

                // Updating flag.
                isInitialized = true;
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }

        /// <summary>
        /// Helper method to check if the queue exits.
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
        /// Helper method to create and return the queue handle. If queue already exists, it will
        /// return the handle to that.
        /// </summary>
        protected virtual QueueDescription CreateQueue(bool enableSession = false)
        {
            #region Validation
            CheckIfQueueHasInitialized();

            if (namespaceManager == null)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.MissingNamespaceAddressInConfiguration,
                    message: ErrorMessages.MissingNamespaceAddressInConfiguration,
                    innerException: null,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration?.QueueName,
                    address: sbConfiguration?.Address,
                    logger: logger);
            }
            #endregion

            #region Initialization
            QueueDescription queueDescription = null;
            #endregion

            #region Creating Queue
            try
            {
                if (IsQueueExists())
                {
                    queueDescription = namespaceManager.GetQueue(sbConfiguration.QueueName);
                }
                else
                {
                    queueDescription = namespaceManager.CreateQueue(new QueueDescription(sbConfiguration.QueueName)
                    {
                        RequiresSession = enableSession,
                        DefaultMessageTimeToLive = sbConfiguration.MessageTimeToLiveInMinutes,
                        MaxDeliveryCount = sbConfiguration.MaxDeliveryCount,
                        EnablePartitioning = sbConfiguration.EnablePartitioning,
                        MaxSizeInMegabytes = sbConfiguration.MaxSizeInMegabytes,
                        LockDuration = sbConfiguration.LockDurationInSeconds,
                        EnableBatchedOperations = sbConfiguration.EnableBatchedOperations,
                        RequiresDuplicateDetection = sbConfiguration.RequiresDuplicateDetection,
                        EnableDeadLetteringOnMessageExpiration = sbConfiguration.EnableDeadLettering,
                    });
                }
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCreateMessageQueue,
                    message: ErrorMessages.FailedToCreateMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
            #endregion

            #region Return
            return queueDescription;
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
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration?.QueueName,
                    address: sbConfiguration?.Address,
                    logger: logger);
            }
        }

        /// <summary>
        /// Helper method to check if queue exists.
        /// </summary>
        private bool IsQueueExistsHelper()
        {
            #region Initialization
            bool result;
            #endregion

            #region Checking Queue Existence
            try
            {
                try
                {
                    queueClient.Peek();

                    result = true;
                }
                catch (UnauthorizedAccessException)
                {
                    // If queue has only Send permission, then it will get this  exception if queue exists.
                    // Which is fine to check queue existence.
                    result = true;
                }
                catch (Exception ex) when (ex is MessagingEntityNotFoundException)
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueExistence,
                    message: ErrorMessages.FailedToCheckQueueExistence,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration?.QueueName,
                    address: sbConfiguration?.Address,
                    logger: logger);
            }
            #endregion

            #region Return
            return result;
            #endregion
        }
        #endregion
    }
}
