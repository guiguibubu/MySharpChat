using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InternalLoggerType = NLog.Logger;

namespace MySharpChat.Core.Utils.Logger
{
    public partial class Logger
    {
        private readonly InternalLoggerType _logger;

        private Logger(InternalLoggerType logger) 
        {
            _logger = logger;
        }

        public void LogTrace(string text)
        {
            _logger.Trace(text);
        }

        public void LogTrace(string format, params object?[] args)
        {
            LogTrace(string.Format(format, args));
        }

        public void LogDebug(string text)
        {
            _logger.Debug(text);
        }

        public void LogDebug(string format, params object?[] args)
        {
            LogDebug(string.Format(format, args));
        }

        public void LogInfo(string text)
        {
            _logger.Info(text);
        }

        public void LogInfo(string format, params object?[] args)
        {
            LogInfo(string.Format(format, args));

        }
        
        public void LogWarning(string text)
        {
            _logger.Warn(text);
        }
        
        public void LogWarning(string format, params object?[] args)
        {
            LogWarning(string.Format(format, args));
        }

        public void LogError(string text)
        {
            _logger.Error(text);
        }

        public void LogError(string format, params object?[] args)
        {
            LogError(string.Format(format, args));

        }
        public void LogCritical(string text)
        {
            _logger.Fatal(text);
        }

        public void LogCritical(string format, params object?[] args)
        {
            LogCritical(string.Format(format, args));

        }
    }
}
