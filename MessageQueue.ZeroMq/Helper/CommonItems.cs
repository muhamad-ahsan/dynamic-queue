using System;
using System.Linq;
using MessageQueue.Core.Helper;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
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
                #region Parameters Validation
                var zeroMqSettings = new ZeroMqConfiguration();
                rawConfiguration = rawConfiguration ?? new Dictionary<string, string>();

                var notSupportedParams =
                    rawConfiguration.Keys.Where(x => !CommonConfigurationKeys.GetAllKeys().Contains(x) &&
                                                     !ZeroMqConfigurationKeys.GetAllKeys().Contains(x)).ToList();

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
                
                zeroMqSettings.Address = rawConfiguration[CommonConfigurationKeys.Address];
                #endregion

                #region Return
                return zeroMqSettings;
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
