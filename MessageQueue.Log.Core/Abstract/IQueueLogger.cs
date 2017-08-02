using System;

namespace MessageQueue.Log.Core.Abstract
{
    /// <summary>
    /// Logging interface for different level of logging (trace, error etc.).
    /// </summary>
    public interface IQueueLogger
    {
        #region Methods
        /// <summary>
        /// Logs provided data as Trace logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        void Trace(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data as Info logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        void Info(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data as Warning logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        void Warn(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data as Error logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        void Error(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data as Fatal logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        void Fatal(Exception exception, string message, params object[] args);
        #endregion
    }
}
