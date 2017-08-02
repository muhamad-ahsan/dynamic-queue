using System;
using System.Collections.Generic;

namespace MessageQueue.Core.Concrete
{
    /// <summary>
    /// Standard exception class for all the exceptions thrown from module.
    /// </summary>
    public sealed class QueueException : Exception
    {
        #region Public Data Members
        /// <summary>
        /// The exception error code.
        /// </summary>
        public QueueErrorCode ErrorCode { get; }
        #endregion

        #region Constructors
        public QueueException(QueueErrorCode errorCode, string message, Exception innerException = null, Dictionary<string, string> context = null) : base(message, innerException)
        {
            #region Initialization
            ErrorCode = errorCode;

            if (context != null)
            {
                foreach (var currentKeyValuePair in context)
                {
                    Data.Add(currentKeyValuePair.Key, currentKeyValuePair.Value);
                }
            }
            #endregion
        }
        #endregion
    }
}
