using System;
using System.IO;
using DisableNvidiaTelemetry.Properties;
using log4net;
using log4net.Config;
using log4net.Core;

namespace DisableNvidiaTelemetry.Utilities
{
    internal static class LogExtensions
    {
        public static event EventHandler<LogEventArgs> LogEvent;

        public static void Log(this ILog log, Level level, object message, Exception ex = null, bool suppressEvents = false)
        {
            if (Logging.IsReadOnly)
                return;

            if (level == Level.Debug)
                log.Debug(message, ex);
            else if (level == Level.Error)
                if (ex == null)
                    log.Error(message);
                else
                    log.Error(message, ex);
            else if (level == Level.Info)
                log.Info(message);
            else if (level == Level.Warn)
                log.Warn(message, ex);
            else
                throw new ArgumentException("Log level not implemented", "level");

            if (!suppressEvents)
                LogEvent?.Invoke(log, new LogEventArgs(log, new Logging.LogEvent {Date = DateTime.Now, Message = message, Error = ex}));
        }

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(ILog log, Logging.LogEvent @event)
            {
                Log = log;
                Event = @event;
            }

            public ILog Log { get; }

            public Logging.LogEvent Event { get; set; }
        }
    }

    internal class Logging
    {
        private static ILog _fileLogger;
        private static ILog _eventLogger;

        private static string _logDirectory;
        private static bool _enabled;
        private static bool _configured;

        private static bool? _isReadOnly;

        public static bool IsReadOnly
        {
            get
            {
                if (!_isReadOnly.HasValue)
                {
                    var cwd = Path.GetDirectoryName(_logDirectory);

                    var di = new DirectoryInfo(cwd);
                    _isReadOnly = di.Attributes.HasFlag(FileAttributes.ReadOnly);
                }

                return _isReadOnly.Value;
            }
        }

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (value)
                {
                    if (!_configured)
                    {
                        if (!Directory.Exists(_logDirectory))
                            Directory.CreateDirectory(_logDirectory);

                        GlobalContext.Properties["HeaderInfo"] = $"{Resources.Disable_Nvidia_Telemetry} v{AppUtils.GetVersion()}";
                        GlobalContext.Properties["LogDirectory"] = _logDirectory;

                        XmlConfigurator.Configure();

                        _configured = true;
                    }

                    LogManager.GetRepository().Threshold = Level.All;
                }

                else
                {
                    LogManager.GetRepository().Threshold = Level.Off;
                }


                _enabled = value;
            }
        }

        public static void Prepare(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        public static ILog GetFileLogger()
        {
            return _fileLogger ?? (_fileLogger = LogManager.GetLogger("FileLogger"));
        }

        public static ILog GetEventLogger()
        {
            return _eventLogger ?? (_eventLogger = LogManager.GetLogger("EventLogger"));
        }

        public class LogEvent
        {
            public DateTime Date { get; set; }

            public object Message { get; set; }

            public Exception Error { get; set; }
        }
    }
}