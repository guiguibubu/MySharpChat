using System;
using System.Collections.Generic;

namespace MySharpChat.Core.Utils.Logger
{
    public partial class Logger
    {
        public static class Factory
        {
            private readonly static NLog.Targets.Target logFileTarget = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "log.txt",
                ArchiveFileName = "log.{#}.txt",
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                ArchiveDateFormat = "dd_MM_yyyy",
            };
            private readonly static NLog.Targets.Target logConsoleTarget = new NLog.Targets.ConsoleTarget("logconsole");

            static Factory()
            {
                SetLoggingType(LoggerType.Both);
            }

            private static Dictionary<string, Logger?> _loggersCache = new Dictionary<string, Logger?>();

            public static void SetLoggingType(LoggerType type)
            {
                var config = new NLog.Config.LoggingConfiguration();

                if (type.HasFlag(LoggerType.Console))
#if DEBUG
                    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logConsoleTarget);
#else
                    config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsoleTarget);
#endif
                if (type.HasFlag(LoggerType.File))
                    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logFileTarget);

                // Apply config           
                NLog.LogManager.Configuration = config;
            }

            public static Logger GetLogger<T>()
            {
                string name = typeof(T).FullName!;
                Logger? logger;
                if (!_loggersCache.TryGetValue(name, out logger))
                {
                    logger = new Logger(NLog.LogManager.GetLogger(name));
                    _loggersCache.Add(name, logger);
                }
                return logger!;
            }
        }
    }
}
