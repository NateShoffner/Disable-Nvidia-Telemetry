using System.ServiceProcess;

namespace DisableNvidiaTelemetry.Model
{
    internal class TelemetryService : ITelemetry
    {
        public TelemetryService(ServiceController service)
        {
            Service = service;
        }

        public ServiceController Service { get; }

        #region Implementation of ITelemetry

        public bool RestartRequired { get; set; }

        public bool IsActive()
        {
            return Service != null && Service.Status == ServiceControllerStatus.Running;
        }

        #endregion
    }
}