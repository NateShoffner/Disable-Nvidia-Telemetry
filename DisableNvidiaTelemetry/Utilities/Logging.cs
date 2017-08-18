using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace DisableNvidiaTelemetry.Utilities
{
    internal static class LogExtensions
    {
        public static event EventHandler<LogEventArgs> LogEvent;

        public static void Log(this ILog log, Level level, string message, Exception ex = null)
        {
            if (level == Level.Debug)
                log.Debug(message, ex);
            else if (level == Level.Error)
                log.Error(message);
            else if (level == Level.Info)
                log.Info(message, ex);
            else if (level == Level.Warn)
                log.Warn(message, ex);
            else
                throw new ArgumentException("Log level not implemented", "level");

            LogEvent?.Invoke(log, new LogEventArgs(message, ex));
        }

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(object message, Exception exception = null)
            {
                Message = message;
                Exception = exception;
            }

            public object Message { get; }

            public Exception Exception { get; }
        }
    }

    internal class Logging
    {
        private static ILog _logger;
        private static string _logDirectory;
        private static bool _configured;

        public static void SetLogDirectory(string directory)
        {
            _logDirectory = directory;
        }

        public static void Initialize()
        {
            if (!_configured)
            {
                GlobalContext.Properties["HeaderInfo"] = $"Disable Nvidia Telemetry v{Application.ProductVersion}";
                GlobalContext.Properties["LogDirectory"] = _logDirectory;

                XmlConfigurator.Configure();

                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                _configured = true;
            }
        }

        public static ILog GetLogger()
        {
            Initialize();

            return _logger ?? (_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
        }

        public static string GetLogFile()
        {
            Initialize();

            return ((Hierarchy) LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>()
                .FirstOrDefault().File;
        }
    }
}