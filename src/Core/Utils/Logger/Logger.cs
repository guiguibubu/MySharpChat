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

        public void LogDebug(string text)
        {
            _logger.Debug(text);
        }

        public void LogInfo(string text)
        {
            _logger.Info(text);
        }

        public void LogWarning(string text)
        {
            _logger.Warn(text);
        }

        public void LogError(string text)
        {
            _logger.Error(text);
        }

        public void LogCritical(string text)
        {
            _logger.Fatal(text);
        }
    }
}
