using System.ServiceProcess;
using Microsoft.Win32.TaskScheduler;

namespace DisableNvidiaTelemetry
{
    internal interface ITelemetry
    {
        bool IsRunning();
    }

    internal class TelemetryTask : ITelemetry
    {
        public TelemetryTask(Task task)
        {
            Task = task;
        }

        public Task Task { get; }

        #region Implementation of ITelemetry

        public bool IsRunning()
        {
            return Task != null && Task.Enabled;
        }

        #endregion
    }

    internal class TelemetryService : ITelemetry
    {
        public TelemetryService(ServiceController service)
        {
            Service = service;
        }

        public ServiceController Service { get; }

        #region Implementation of ITelemetry

        public bool IsRunning()
        {
            return Service != null && Service.Status == ServiceControllerStatus.Running;
        }

        #endregion
    }
}