#region

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using log4net.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry.Utilities
{
    internal class NvidiaController
    {
        /// <summary>
        ///     Returns known telemetry tasks.
        /// </summary>
        /// <param name="logging">Determines whether logging should be done.</param>
        public static List<TelemetryTask> GetTelemetryTasks(bool logging)
        {
            var tasks = new List<TelemetryTask>();

            var taskNames = new[] {"NvTmMon_*", "NvTmRep_*", "NvTmRepOnLogon_*"};

            foreach (var taskName in taskNames)
            {
                var task = TaskService.Instance.FindTask(taskName);

                if (task == null)
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to find task: {taskName}");
                }

                else
                {
                    if (logging)
                    {
                        Logging.GetFileLogger().Log(Level.Info, $"Found Task: {task.Name}");
                        Logging.GetFileLogger().Log(Level.Info, $"Task is: {(task.Enabled ? "Enabled" : "Disabled")}");
                    }

                    tasks.Add(new TelemetryTask(task));
                }
            }

            return tasks;
        }

        /// <summary>
        ///     Returns known telemetry services.
        /// </summary>
        /// <param name="logging">Determines whether logging should be performed.</param>
        public static List<TelemetryService> GetTelemetryServices(bool logging)
        {
            var services = new List<TelemetryService>();

            var serviceNames = new[] {"NvTelemetryContainer"};

            foreach (var serviceName in serviceNames)
            {
                var service = new ServiceController(serviceName);

                try
                {
                    // throw error if service as not found
                    var running = service.Status == ServiceControllerStatus.Running;

                    services.Add(new TelemetryService(service));

                    if (logging)
                    {
                        Logging.GetFileLogger().Log(Level.Info, $"Found Service: {service.DisplayName} ({service.ServiceName})");
                        Logging.GetFileLogger().Log(Level.Info, $"Service is: {(running ? "Enabled" : "Disabled")}");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to find service: {serviceName}");
                }
            }

            return services;
        }

        /// <summary>
        ///     Returns known telemetry registry keys.
        /// </summary>
        /// <param name="logging">Determines whether logging should be performed.</param>
        /// <returns></returns>
        public static List<TelemetryRegistryKey> GetTelemetryRegistryEntires(bool logging)
        {
            var entries = new List<TelemetryRegistryKey>
            {
                new TelemetryRegistryKey(Registry.CurrentUser, @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client",
                    new Dictionary<string, TelemetryRegistryKey.RegistryValuePair>
                    {
                        {"OptInOrOutPreference", new TelemetryRegistryKey.RegistryValuePair("1", "0")}
                    })
            };

            foreach (var entry in entries)
            {
                try
                {
                    var key = entry.SubKey;

                    if (logging)
                    {
                        var values = entry.GetValues();

                        var count = 0;

                        if (values != null)
                            foreach (var kv in values)
                            {
                                if (kv.Value != null)
                                    count++;
                            }

                        Logging.GetFileLogger().Log(Level.Info, $"Found registry key: {key}");
                        Logging.GetFileLogger().Log(Level.Info, $"Found {count}/{values.Values.Count} expected value(s)");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to find registry key: {entry.Name}");
                }
            }

            return entries;
        }

        /// <summary>
        ///     Disables the provided telemetry services if they are currently running.
        /// </summary>
        /// <param name="services">The services to disable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        /// <param name="eventLog">Determines whether event logging should be performed.</param>
        public static void DisableTelemetryServices(List<ServiceController> services, bool logging, bool eventLog)
        {
            foreach (var service in services)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Disabled service: {service.DisplayName} ({service.ServiceName})");

                        if (eventLog)
                            Logging.GetEventLogger().Log(Level.Info, $"Disabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to disable service: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Disabled)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Disabled);

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Disabled service startup: {service.DisplayName} ({service.ServiceName})");

                        if (eventLog)
                            Logging.GetEventLogger().Log(Level.Info, $"Disabled service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    Logging.GetFileLogger().Log(Level.Info, $"Failed to disable service startup: {service.DisplayName} ({service.ServiceName})");
                }
            }
        }


        /// <summary>
        ///     Disables the provided tasks if they are currently enabled.
        /// </summary>
        /// <param name="tasks">The tasks to disable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        /// <param name="eventLog">Determines whether event logging should be performed.</param>
        public static void DisableTelemetryTasks(List<Task> tasks, bool logging, bool eventLog)
        {
            foreach (var task in tasks)
            {
                try
                {
                    if (task.Enabled)
                    {
                        task.Enabled = false;

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Disabled task: {task.Path}");

                        if (eventLog)
                            Logging.GetEventLogger().Log(Level.Info, $"Disabled task: {task.Path}");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to disable task: {task.Path}");
                }
            }
        }

        /// <summary>
        ///     Disables the provideed registry keys if they are currently enabled.
        /// </summary>
        /// <param name="keys">The registry keys in which to disable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        /// <param name="eventLog">Determines whether event logging should be performed.</param>
        public static void DisableTelemetryRegistryEntries(List<TelemetryRegistryKey> keys, bool logging, bool eventLog)
        {
            foreach (var key in keys)
            {
                try
                {
                    if (key.IsActive())
                    {
                        key.Enabled = false;

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Disabled key: {key.Name}");

                        if (eventLog)
                            Logging.GetEventLogger().Log(Level.Info, $"Disabled key: {key.Name}");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to disable registry key: {key.Name}");
                }
            }
        }

        /// <summary>
        ///     Enables the provided services.
        /// </summary>
        /// <param name="services">The services to enable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        public static void EnableTelemetryServices(List<ServiceController> services, bool logging)
        {
            foreach (var service in services)
            {
                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Automatic)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Automatic);

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Enabled automatic service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to enable automatic service startup: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Enabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to start service: {service.DisplayName} ({service.ServiceName})");
                }
            }
        }

        /// <summary>
        ///     Enables the provided tasks.
        /// </summary>
        /// <param name="tasks">The tasks to enable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        public static void EnableTelemetryTasks(List<Task> tasks, bool logging)
        {
            foreach (var task in tasks)
            {
                if (task != null)
                    try
                    {
                        if (!task.Enabled)
                        {
                            task.Enabled = true;

                            if (logging)
                                Logging.GetFileLogger().Log(Level.Info, $"Enabled task: {task.Path}");
                        }
                    }

                    catch
                    {
                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Failed to enable task: {task.Path}");
                    }
            }
        }

        /// <summary>
        ///     Enables the provides registry keys and their respective value(s).
        /// </summary>
        /// <param name="keys">The registry keys in which to enable.</param>
        /// <param name="logging">Determines whether logging should be performed.</param>
        public static void EnableTelemetryRegistryEntries(List<TelemetryRegistryKey> keys, bool logging)
        {
            foreach (var key in keys)
            {
                try
                {
                    if (!key.IsActive())
                    {
                        key.Enabled = true;

                        if (logging)
                            Logging.GetFileLogger().Log(Level.Info, $"Enabled key: {key.Name}");
                    }
                }

                catch (Exception ex)
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"Failed to enable registry key: {key.Name}");
                }
            }
        }
    }
}