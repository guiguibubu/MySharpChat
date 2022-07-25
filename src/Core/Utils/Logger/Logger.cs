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

        public void LogTrace(Exception e, string text)
        {
            _logger.Trace(e, text);
        }

        public void LogTrace(Exception e, string format, params object?[] args)
        {
            LogTrace(e, string.Format(format, args));
        }

        public void LogDebug(string text)
        {
            _logger.Debug(text);
        }

        public void LogDebug(string format, params object?[] args)
        {
            LogDebug(string.Format(format, args));
        }

        public void LogDebug(Exception e, string text)
        {
            _logger.Debug(e, text);
        }

        public void LogDebug(Exception e, string format, params object?[] args)
        {
            LogDebug(e, string.Format(format, args));
        }

        public void LogInfo(string text)
        {
            _logger.Info(text);
        }

        public void LogInfo(string format, params object?[] args)
        {
            LogInfo(string.Format(format, args));

        }

        public void LogInfo(Exception e, string text)
        {
            _logger.Info(e, text);
        }

        public void LogInfo(Exception e, string format, params object?[] args)
        {
            LogInfo(e, string.Format(format, args));
        }

        public void LogWarning(string text)
        {
            _logger.Warn(text);
        }
        
        public void LogWarning(string format, params object?[] args)
        {
            LogWarning(string.Format(format, args));
        }

        public void LogWarning(Exception e, string text)
        {
            _logger.Warn(e, text);
        }

        public void LogWarning(Exception e, string format, params object?[] args)
        {
            LogWarning(e, string.Format(format, args));
        }

        public void LogError(string text)
        {
            _logger.Error(text);
        }

        public void LogError(string format, params object?[] args)
        {
            LogError(string.Format(format, args));
        }

        public void LogError(Exception e, string text)
        {
            _logger.Error(e, text);
        }

        public void LogError(Exception e, string format, params object?[] args)
        {
            LogError(e, string.Format(format, args));
        }

        public void LogCritical(string text)
        {
            _logger.Fatal(text);
        }

        public void LogCritical(string format, params object?[] args)
        {
            LogCritical(string.Format(format, args));
        }

        public void LogCritical(Exception e, string text)
        {
            _logger.Fatal(e, text);
        }

        public void LogCritical(Exception e, string format, params object?[] args)
        {
            LogCritical(e, string.Format(format, args));
        }
    }
}
