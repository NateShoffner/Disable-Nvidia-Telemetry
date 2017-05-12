#region

using System;
using System.Security.Principal;
using System.Windows.Forms;

#endregion

namespace DisableNvidiaTelemetry
{
    static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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