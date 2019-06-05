using Microsoft.Extensions.Logging;
using NLog;
using System;

namespace FootStone.Core
{
    public class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        private Logger logger = LogManager.GetLogger("Orleans");

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

      
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            NLog.LogLevel level = NLog.LogLevel.Off;
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    level = NLog.LogLevel.Trace;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    level = NLog.LogLevel.Debug;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    level = NLog.LogLevel.Info;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    level = NLog.LogLevel.Warn;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    level = NLog.LogLevel.Error;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    level = NLog.LogLevel.Fatal;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    level = NLog.LogLevel.Off;
                    break;
            }
            logger.Log(level, formatter(state, exception));
        }
    }

    public class NLogLoggerProvider : ILoggerProvider
    {
        public NLogLoggerProvider()
        {
        }
        
        public void Dispose()
        {
        }

        Microsoft.Extensions.Logging.ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return new NLogLogger();
        }
    }
}
