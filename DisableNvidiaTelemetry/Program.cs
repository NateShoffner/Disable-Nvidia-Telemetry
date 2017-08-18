#region

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Forms;
using DisableNvidiaTelemetry.Utilities;
using log4net.Core;

#endregion

namespace DisableNvidiaTelemetry
{
    internal static class Program
    {
        public const string StartupParamSilent = "-silent";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Logging.SetLogDirectory(logDirectory);

            // log all the errors
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception) e.ExceptionObject;
                Logging.GetLogger().Error(ex);
            };

            var silentMode = false;

            if (args.Length > 0)
            {
                Logging.GetLogger().Log(Level.Info, $"Startup params: {string.Join(" ", args)}");

                if (args.Contains(StartupParamSilent))
                {
                    silentMode = true;
                    SilentlyDisableTelemetry();
                }
            }

            if (!IsAdministrator())
            {
                MessageBox.Show("Please run the program as administrator to continue.", "Administrator Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!silentMode)
            {
                Application.Run(new FormMain());
            }
        }
        private static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void SilentlyDisableTelemetry()
        {
            Logging.GetLogger().Log(Level.Info, "Silently disabling telemetry services.");

            var services = NvidiaController.GetTelemetryServices(true);
            NvidiaController.DisableTelemetryServices(services.Select(s => s.Service).ToList());

            Logging.GetLogger().Log(Level.Info, "Silently disabling telemetry tasks.");

            var tasks = NvidiaController.GetTelemetryTasks(true);
            NvidiaController.DisableTelemetryTasks(tasks.Select(t => t.Task).ToList());
        }
    }
}