using System.Collections.Generic;

namespace MessageQueue.CofigurationProvider.Core.Abstract
{
    /// <summary>
    /// Provides configuration for queues initialization.
    /// </summary>
    public interface IQueueConfigurationProvider
    {
        #region Methods
        /// <summary>
        /// Returns configuration based on the configuration identifier.
        /// </summary>
        /// <param name="configurationIdentifier">The configuration identifier</param>
        /// <returns>Key value pair of configuration</returns>
        Dictionary<string, string> GetConfiguration(string configurationIdentifier);
        #endregion
    }
}
