using System;
using System.Threading.Tasks;

namespace MessageQueue.Log.Core.Abstract
{
    /// <summary>
    /// Asynchronous logging interface for different level of logging (trace, error etc.).
    /// </summary>
    public interface IQueueLoggerAsync
    {
        #region Methods
        /// <summary>
        /// Logs provided data asynchronously as Trace logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        Task TraceAsync(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data asynchronously as Info logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        Task InfoAsync(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data asynchronously as Warning logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        Task WarnAsync(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs provided data asynchronously as Error logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        Task ErrorAsync(Exception exception, string message, params object[] args);
        
        /// <summary>
        /// Logs provided data asynchronously as Fatal logging level.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The place holder values in message</param>
        Task FatalAsync(Exception exception, string message, params object[] args);
        #endregion
    }
}
