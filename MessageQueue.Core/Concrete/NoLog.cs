using System;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.Core.Concrete
{
    /// <summary>
    /// Dummy logger to be used when no logger is passed.
    /// </summary>
    internal sealed class NoLog : IQueueLogger
    {
        #region IQueueLogger Implementation
        public void Error(Exception exception, string message, params object[] args)
        {
            // Swallow...
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            // Swallow...
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            // Swallow...
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            // Swallow...
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            // Swallow...
        }
        #endregion
    }
}
