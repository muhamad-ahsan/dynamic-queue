using NLog;
using System;
using System.Threading.Tasks;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.Log.NLog.Concrete
{
    public class NQueueLogger : IQueueLogger, IQueueLoggerAsync
    {
        #region Private Data Members
        private readonly Logger logger;
        #endregion

        #region Constructors
        public NQueueLogger(string loggerIdentifier)
        {
            #region Initialization
            logger = LogManager.GetLogger(loggerIdentifier);
            #endregion
        }
        #endregion

        #region IQueueLogger Implementation
        public void Trace(Exception exception, string message, params object[] args)
        {
            logger.Trace(exception, message, args);   
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            logger.Info(exception, message, args);
        }
        
        public void Warn(Exception exception, string message, params object[] args)
        {
            logger.Warn(exception, message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            logger.Error(exception, message, args);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            logger.Fatal(exception, message, args);
        }
        #endregion

        #region IQueueLoggerAsync Implementation
        public async Task TraceAsync(Exception exception, string message, params object[] args)
        {
            await Task.Run(() => logger.Trace(exception, message, args));
        }

        public async Task InfoAsync(Exception exception, string message, params object[] args)
        {
            await Task.Run(() => logger.Info(exception, message, args));
        }

        public async Task WarnAsync(Exception exception, string message, params object[] args)
        {
            await Task.Run(() => logger.Warn(exception, message, args));
        }

        public async Task ErrorAsync(Exception exception, string message, params object[] args)
        {
            await Task.Run(() => logger.Error(exception, message, args));
        }

        public async Task FatalAsync(Exception exception, string message, params object[] args)
        {
            await Task.Run(() => logger.Fatal(exception, message, args));
        }
        #endregion
    }
}
