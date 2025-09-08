using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Base
{
    public class Log4NetAdapterFactory: ILoggerFactory
    {
        private readonly ILog _logger;
        public LogLevel? LevelOverride { get; set; }

        public Log4NetAdapterFactory(ILog logger) {  _logger = logger; }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Log4NetAdapterLogger(_logger, categoryName) { factory = this };
        }

        public void Dispose()
        {
        }
    }

    public class Log4NetAdapterLogger : ILogger
    {
        private readonly ILog _logger;
        private IDisposable myState;
        private string myCategoryName;
        public Log4NetAdapterFactory factory { get; set; }

        public Log4NetAdapterLogger(ILog logger, string myCategoryName)
        {
            _logger = logger;
            this.myCategoryName = myCategoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            myState = state as IDisposable;
            return myState;    
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (factory.LevelOverride != null)
            {
                return factory.LevelOverride <= logLevel;
            } 
            switch (logLevel)
            {
                case LogLevel.Trace: return _logger.IsDebugEnabled;
                case LogLevel.Debug: return _logger.IsDebugEnabled;
                case LogLevel.Information: return _logger.IsInfoEnabled;
                case LogLevel.Warning: return _logger.IsWarnEnabled;
                default: return _logger.IsErrorEnabled;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: 
                case LogLevel.Debug:
                    _logger.Debug("Id:" + eventId + ":" + formatter(state, exception), exception);
                    break;
                case LogLevel.Information:
                    _logger.Info("Id:" + eventId + ":" + formatter(state, exception), exception);
                    break;

                case LogLevel.Warning:
                    _logger.Warn("Id:" + eventId + ":" + formatter(state, exception), exception);
                    break;
                default:
                    _logger.Error("Id:" + eventId + ":" + formatter(state, exception), exception);
                    break;
            }
        }
    }
}
