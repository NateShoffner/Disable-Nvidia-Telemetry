#region

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Forms;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using log4net.Core;

#endregion

namespace DisableNvidiaTelemetry
{
    internal static class Program
    {
        public const string StartupParamSilent = "-silent";
        public const string StartupParamRegisterTask = "-registertask";
        public const string StartupParamUnregisterTask = "-unregistertask";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if PORTABLE
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
#else
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Disable Nvidia Telemetry");
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            var logDirectory = Path.Combine(appData, "Logs");
#endif
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            Logging.Initialize(logDirectory);

            // log all the errors
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception) e.ExceptionObject;
                Logging.GetFileLogger().Error(ex);
            };

            var silentMode = false;

            if (args.Length > 0)
            {
                Logging.GetFileLogger().Log(Level.Info, $"Startup params: {string.Join(" ", args)}");

                if (args.Contains(StartupParamSilent))
                {
                    silentMode = true;
                    SilentlyDisableTelemetry();
                }

                if (args.Contains(StartupParamRegisterTask))
                    if (TaskSchedulerUtilities.GetTask() == null)
                        TaskSchedulerUtilities.Create((TaskSchedulerUtilities.TaskTrigger)Settings.Default.BackgroundTaskTrigger);

                if (args.Contains(StartupParamUnregisterTask))
                    TaskSchedulerUtilities.Remove();
            }

            if (!IsAdministrator())
                MessageBox.Show("Please run the program as administrator to continue.", "Administrator Required", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (!silentMode)
                Application.Run(new FormMain());
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void SilentlyDisableTelemetry()
        {
            Logging.GetFileLogger().Log(Level.Info, "Silently disabling telemetry services.");

            var services = NvidiaController.GetTelemetryServices(true);
            NvidiaController.DisableTelemetryServices(services.Select(s => s.Service).ToList(), true, true);

            Logging.GetFileLogger().Log(Level.Info, "Silently disabling telemetry tasks.");

            var tasks = NvidiaController.GetTelemetryTasks(true);
            NvidiaController.DisableTelemetryTasks(tasks.Select(t => t.Task).ToList(), true, true);
        }
    }
}