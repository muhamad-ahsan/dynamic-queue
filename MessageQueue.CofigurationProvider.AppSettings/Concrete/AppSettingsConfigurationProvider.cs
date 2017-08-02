using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using MessageQueue.CofigurationProvider.Core.Abstract;
using MessageQueue.CofigurationProvider.AppSettings.Properties;

namespace MessageQueue.CofigurationProvider.AppSettings.Concrete
{
    /// <summary>
    /// Implementation of IQueueConfigurationProvider provider which returns configuration from .config file (app or web).
    /// </summary>
    public class AppSettingsConfigurationProvider : IQueueConfigurationProvider
    {
        #region Private Data Members
        private const string configIdentiferKeySeparator = ":";
        #endregion

        #region IQueueConfigurationProvider Implementation
        public Dictionary<string, string> GetConfiguration(string configurationIdentifier)
        {
            try
            {
                #region Business Description
                // 1-   All the keys in appSettings should be prefixed with configuration identifier
                //      and separated by ":". E.g. MyConfig:Key1, MyConfig:Key2 and so on.
                #endregion

                #region Validation
                if (string.IsNullOrEmpty(configurationIdentifier))
                {
                    throw new ArgumentNullException(nameof(configurationIdentifier));
                }
                #endregion

                #region Configuration Retrieval
                configurationIdentifier = configurationIdentifier.ToUpper();
                var appSettings = ConfigurationManager.AppSettings;

                return ConfigurationManager.AppSettings
                    .AllKeys
                    .Where(x => x.ToUpper().Contains(configurationIdentifier))
                    .ToDictionary(
                        key => key.Substring(key.IndexOf(configIdentiferKeySeparator, StringComparison.Ordinal) + 1),
                        val => appSettings[val]);
                #endregion
            }
            catch (Exception exception)
            {
                throw new ApplicationException(string.Format(ErrorMessages.FailedToReadConfiguration, configurationIdentifier), exception);
            }
        }
        #endregion
    }
}
