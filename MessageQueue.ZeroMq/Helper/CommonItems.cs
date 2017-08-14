using System;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.ZeroMq.Concrete;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.ZeroMq.Helper
{
    /// <summary>
    /// Contains common functionality.
    /// </summary>
    internal static class CommonItems
    {
        #region Public Data Members
        public const string ZeroMqName = "ZeroMq";
        #endregion

        #region Public Methods
        /// <summary>
        /// Helper method to validate and collect ZeroMq parameters.
        /// </summary>
        public static ZeroMqConfiguration CollectZmqConfiguration(ref Dictionary<string, string> rawConfiguration, bool isInbound, ref IQueueLogger logger)
        {
            try
            {
                #region Initialization
                var zeroMqConfiguration = new ZeroMqConfiguration();
                rawConfiguration = rawConfiguration ?? new Dictionary<string, string>();
                #endregion

                #region Collecting Common Configuration
                MessageQueueCommonItems.CollectCommonConfiguration(ref rawConfiguration, zeroMqConfiguration, ZeroMqConfigurationKeys.GetAllKeys());
                #endregion

                #region Collecting Other Configuration
                // Nothing to collect.
                #endregion

                #region Return
                return zeroMqConfiguration;
                #endregion
            }
            catch (QueueException queueException)
            {
                #region Adding Context Data
                queueException.Data.Add(CommonContextKeys.QueueContext, ZeroMqName);
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
                    , ErrorMessages.GeneralConfigurationParsingError, ex, ZeroMqName, logger: logger);
            }
        }
        #endregion
    }
}
