using System.Diagnostics;

namespace DisableNvidiaTelemetry.Utilities
{
    internal class WindowsUtils
    {
        public static void Restart()
        {
            StartShutDown("-f -r -t 5");
        }

        private static void StartShutDown(string param)
        {
            var proc = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "/C shutdown " + param
            };

            Process.Start(proc);
        }
    }
}