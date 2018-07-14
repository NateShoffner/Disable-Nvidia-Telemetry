using Microsoft.Win32.TaskScheduler;

namespace DisableNvidiaTelemetry.Model
{
    internal class TelemetryTask : ITelemetry
    {
        public TelemetryTask(Task task)
        {
            Task = task;
        }

        public Task Task { get; }

        #region Implementation of ITelemetry

        public bool RestartRequired { get; set; }

        public bool IsActive()
        {
            return Task != null && Task.Enabled;
        }

        #endregion
    }
}