#region

using System;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Forms;
using DisableNvidiaTelemetry.Utilities;

#endregion

namespace DisableNvidiaTelemetry
{
    static class Program
    {
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
                var ex = (Exception)e.ExceptionObject;
                Logging.GetLogger().Error(ex);
            };

            if (!IsAdministrator())
            {
                MessageBox.Show("Please run the program as administrator to continue.", "Administrator Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Run(new FormMain());
        }

        private static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}