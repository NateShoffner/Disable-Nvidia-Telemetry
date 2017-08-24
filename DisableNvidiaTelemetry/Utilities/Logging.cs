using System;
using System.IO;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Core;

namespace DisableNvidiaTelemetry.Utilities
{
    internal static class LogExtensions
    {
        public static event EventHandler<LogEventArgs> LogEvent;

        public static void Log(this ILog log, Level level, string message, Exception ex = null, bool suppressEvents = false)
        {
            if (level == Level.Debug)
                log.Debug(message, ex);
            else if (level == Level.Error)
                log.Error(message);
            else if (level == Level.Info)
                log.Info(message);
            else if (level == Level.Warn)
                log.Warn(message, ex);
            else
                throw new ArgumentException("Log level not implemented", "level");

            if (!suppressEvents)
                LogEvent?.Invoke(log, new LogEventArgs(log, message, ex));
        }

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(ILog log, object message, Exception exception = null)
            {
                Log = log;
                Message = message;
                Exception = exception;
            }

            public ILog Log { get; }

            public object Message { get; }

            public Exception Exception { get; }
        }
    }

    internal class Logging
    {
        private static ILog _fileLogger;
        private static ILog _eventLogger;

        public static void Initialize(string logDirectory)
        {
            GlobalContext.Properties["HeaderInfo"] = $"Disable Nvidia Telemetry v{Application.ProductVersion}";
            GlobalContext.Properties["LogDirectory"] = logDirectory;

            XmlConfigurator.Configure();

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
        }

        public static ILog GetFileLogger()
        {
            return _fileLogger ?? (_fileLogger = LogManager.GetLogger("FileLogger"));
        }

        public static ILog GetEventLogger()
        {
            return _eventLogger ?? (_eventLogger = LogManager.GetLogger("EventLogger"));
        }
    }
}