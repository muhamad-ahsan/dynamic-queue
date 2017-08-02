using System;

namespace MessageQueue.Core.Abstract
{
    /// <summary>
    /// Common interface for all different messaging queue interfaces.
    /// </summary>
    public interface IMessageQueue : IDisposable
    {
        #region Properties
        /// <summary>
        /// The queue address.
        /// </summary>
        string Address { get; }
        #endregion
    }
}
