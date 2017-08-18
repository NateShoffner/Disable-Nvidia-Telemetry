#region

using System.Collections.Generic;
using System.ServiceProcess;
using log4net.Core;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry.Utilities
{
    internal class NvidiaController
    {
        public static List<Task> GetTelemetryTasks(bool logging)
        {
            var tasks = new List<Task>();

            var taskNames = new[] {"NvTmMon_*", "NvTmRep_*", "NvTmRepOnLogon_*"};

            foreach (var taskName in taskNames)
            {
                var task = TaskService.Instance.FindTask(taskName);

                if (task == null)
                {
                    if (logging)
                        Logging.GetLogger().Log(Level.Info, $"Failed to find task: {taskName}");
                }

                else
                {
                    if (logging)
                    {
                        Logging.GetLogger().Log(Level.Info, $"Found Task: {task.Name}");
                        Logging.GetLogger().Log(Level.Info, $"Task is: {(task.Enabled ? "Enabled" : "Disabled")}");
                    }

                    tasks.Add(task);
                }
            }

            return tasks;
        }

        public static List<ServiceController> GetTelemetryServices(bool logging)
        {
            var services = new List<ServiceController>();

            var serviceNames = new[] {"NvTelemetryContainer"};

            foreach (var serviceName in serviceNames)
            {
                var service = new ServiceController(serviceName);

                try
                {
                    // throw error if service as not found
                    var running = service.Status == ServiceControllerStatus.Running;

                    services.Add(service);

                    if (logging)
                    {
                        Logging.GetLogger().Log(Level.Info, $"Found Service: {service.DisplayName} ({service.ServiceName})");
                        Logging.GetLogger().Log(Level.Info, $"Service is: {(running ? "Enabled" : "Disabled")}");
                    }
                }

                catch
                {
                    if (logging)
                        Logging.GetLogger().Log(Level.Info, $"Failed to find service: {serviceName}");
                }
            }

            return services;
        }

        public static void DisableTelemetryServices(List<ServiceController> services)
        {
            foreach (var service in services)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        Logging.GetLogger().Log(Level.Info, $"Disabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    Logging.GetLogger().Log(Level.Info, $"Failed to disable service: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Disabled)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Disabled);
                        Logging.GetLogger().Log(Level.Info, $"Disabled service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    Logging.GetLogger().Log(Level.Info, $"Failed to disable service startup: {service.DisplayName} ({service.ServiceName})");
                }
            }
        }

        public static void DisableTelemetryTasks(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                try
                {
                    if (task.Enabled)
                    {
                        task.Enabled = false;
                        Logging.GetLogger().Log(Level.Info, $"Disabled task: {task.Path}");
                    }
                }

                catch
                {
                    Logging.GetLogger().Log(Level.Info, $"Failed to disable task: {task.Path}");
                }
            }
        }

        public static void EnableTelemetryServices(List<ServiceController> services)
        {
            foreach (var service in services)
            {
                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Automatic)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Automatic);

                        Logging.GetLogger().Log(Level.Info, $"Enabled automatic service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    Logging.GetLogger().Log(Level.Info, $"Failed to enable automatic service startup: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        Logging.GetLogger().Log(Level.Info, $"Enabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    Logging.GetLogger().Log(Level.Info, $"Failed to start service: {service.DisplayName} ({service.ServiceName})");
                }
            }
        }

        public static void EnableTelemetryTasks(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                if (task != null)
                    try
                    {
                        if (!task.Enabled)
                        {
                            task.Enabled = true;
                            Logging.GetLogger().Log(Level.Info, $"Enabled task: {task.Path}");
                        }
                    }

                    catch
                    {
                        Logging.GetLogger().Log(Level.Info, $"Failed to enable task: {task.Path}");
                    }
            }
        }
    }
}