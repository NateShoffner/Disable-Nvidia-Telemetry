#region

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry.Utilities
{
    internal class TelemetryQuery<T>
    {
        public TelemetryQuery(string query, T result)
        {
            Query = query;
            Result = result;
        }

        public string Query { get; }

        public T Result { get; }
    }

    internal class NvidiaController
    {
        public static List<TelemetryQuery<Task>> GetTelemetryTasks()
        {
            var tasks = new List<TelemetryQuery<Task>>();

            var taskNames = new[] {"NvTmMon_*", "NvTmRep_*", "NvTmRepOnLogon_*"};

            foreach (var taskName in taskNames)
            {
                var task = TaskService.Instance.FindTask(taskName);
                tasks.Add(new TelemetryQuery<Task>(taskName, task));
            }

            return tasks;
        }

        public static List<TelemetryQuery<ServiceController>> GetTelemetryServices()
        {
            var services = new List<TelemetryQuery<ServiceController>>();

            var serviceNames = new[] {"NvTelemetryContainer"};

            foreach (var serviceName in serviceNames)
            {
                var service = new ServiceController(serviceName);

                try
                {
                    services.Add(new TelemetryQuery<ServiceController>(serviceName, service));
                }

                catch (InvalidOperationException)
                {
                    services.Add(new TelemetryQuery<ServiceController>(serviceName, null));
                }
            }

            return services;
        }
    }
}