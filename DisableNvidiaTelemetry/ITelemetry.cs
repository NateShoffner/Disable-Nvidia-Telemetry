using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace DisableNvidiaTelemetry
{
    internal interface ITelemetry
    {
        bool IsActive();
    }

    internal class TelemetryTask : ITelemetry
    {
        public TelemetryTask(Task task)
        {
            Task = task;
        }

        public Task Task { get; }

        #region Implementation of ITelemetry

        public bool IsActive()
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

        public bool IsActive()
        {
            return Service != null && Service.Status == ServiceControllerStatus.Running;
        }

        #endregion
    }

    internal class TelemetryRegistryKey : ITelemetry
    {
        private readonly string _subKeyPath;

        public Dictionary<string, RegistryValuePair> ValueData;

        public TelemetryRegistryKey(RegistryKey baseKey, string subKeyPath, Dictionary<string, RegistryValuePair> valueData)
        {
            BaseKey = baseKey;
            _subKeyPath = subKeyPath;
            ValueData = valueData;
        }

        private RegistryKey BaseKey { get; }

        public RegistryKey SubKey => BaseKey.OpenSubKey(_subKeyPath);

        public string Name => $"{BaseKey.Name}\\{_subKeyPath}";

        public bool Enabled
        {
            set
            {
                var subKey = SubKey;

                foreach (var vd in ValueData)
                {
                    subKey.SetValue(vd.Key, value ? vd.Value.Enabled : vd.Value.Disabled);
                }
            }
        }

        #region Implementation of ITelemetry

        public bool IsActive()
        {
            var subKey = SubKey;

            if (subKey == null)
                return false;

            return ValueData.Any(vd => subKey.GetValue(vd.Key).ToString() == vd.Value.Enabled);
        }

        #endregion

        public Dictionary<string, string> GetValues()
        {
            var values = new Dictionary<string, string>();

            var subKey = SubKey;

            if (subKey == null)
                return null;

            foreach (var vd in ValueData)
            {
                var value = subKey.GetValue(vd.Key);
                values.Add(vd.Key, value?.ToString());
            }

            return values;
        }

        public class RegistryValuePair
        {
            public RegistryValuePair(string enabled, string disabled)
            {
                Enabled = enabled;
                Disabled = disabled;
            }

            public string Enabled { get; }

            public string Disabled { get; }
        }
    }
}