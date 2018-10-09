#region

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using DisableNvidiaTelemetry.Model;
using DisableNvidiaTelemetry.Utilities;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry.Controller
{
    internal class NvidiaController
    {
        /// <summary>
        ///     Returns an enumerable collection of telemetry tasks.
        /// </summary>
        public static IEnumerable<NvidiaControllerResult<TelemetryTask>> EnumerateTelemetryTasks()
        {
            var taskFilters = new[]
            {
                new Regex("NvTmMon_*", RegexOptions.Compiled),
                new Regex("NvTmRep*", RegexOptions.Compiled),
                new Regex("NvTmRepOnLogon_*", RegexOptions.Compiled)
            };

            foreach (var filter in taskFilters)
            {
                var tasks = TaskService.Instance.FindAllTasks(filter);

                if (tasks.Length == 0)
                    yield return new NvidiaControllerResult<TelemetryTask>(null, new TaskNotFoundException($"Failed to find task: {filter}")) {Name = filter.ToString()};

                foreach (var task in tasks)
                {
                    var telemetryTask = new TelemetryTask(task);
                    yield return new NvidiaControllerResult<TelemetryTask>(telemetryTask) {Name = filter.ToString()};
                }
            }
        }

        /// <summary>
        ///     Returns an enumerable collection of telemetry services.
        /// </summary>
        public static IEnumerable<NvidiaControllerResult<TelemetryService>> EnumerateTelemetryServices()
        {
            var serviceNames = new[] {"NvTelemetryContainer"};

            foreach (var serviceName in serviceNames)
            {
                var sc = new ServiceController(serviceName);

                TelemetryService service = null;
                Exception error = null;

                try
                {
                    // throw error if service is not found
                    var running = sc.Status == ServiceControllerStatus.Running;
                    service = new TelemetryService(sc);
                }

                catch (Exception ex)
                {
                    error = ex;
                }

                yield return new NvidiaControllerResult<TelemetryService>(service, error) {Name = serviceName};
            }
        }

        /// <summary>
        ///     Returns an enumerable collection of telemetry registry items.
        /// </summary>
        public static IEnumerable<NvidiaControllerResult<TelemetryRegistryKey>> EnumerateTelemetryRegistryItems()
        {
            var keys = new List<TelemetryRegistryKey>
            {
                new TelemetryRegistryKey(Registry.CurrentUser, @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client",
                    new Dictionary<string, TelemetryRegistryKey.RegistryValuePair>
                    {
                        {"OptInOrOutPreference", new TelemetryRegistryKey.RegistryValuePair("1", "0")}
                    }),
                new TelemetryRegistryKey(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem",
                    new Dictionary<string, TelemetryRegistryKey.RegistryExpressionModifiers>
                    {
                        {
                            "ImagePath", new TelemetryRegistryKey.RegistryExpressionModifiers(
                                new Regex(@"-st ""(.*)\\NVIDIA Corporation\\NvContainer\\NvContainerTelemetryApi.dll""$", RegexOptions.Compiled),
                                new TelemetryRegistryKey.Replacement(
                                    new Regex(@"""(.*)\\NVIDIA Corporation\\NvContainer\\nvcontainer.exe""(.*)$", RegexOptions.Compiled),
                                    @"""$1\NVIDIA Corporation\NvContainer\nvcontainer.exe""$2 -st ""$1\NVIDIA Corporation\NvContainer\NvContainerTelemetryApi.dll"""),
                                new TelemetryRegistryKey.Replacement(
                                    new Regex(@"(.*) -st ""(.*)\\NVIDIA Corporation\\NvContainer\\NvContainerTelemetryApi.dll""$", RegexOptions.Compiled),
                                    "$1"))
                        }
                    }) {RestartRequired = true}
            };

            foreach (var key in keys)
            {
                TelemetryRegistryKey telemetryRegistryKey = null;
                Exception error = null;

                try
                {
                    // attempt to enter subkey
                    var subKey = key.SubKey;
                    telemetryRegistryKey = key;

                    if (subKey == null)
                        throw new NullReferenceException();
                }

                catch (Exception ex)
                {
                    error = ex;
                }

                if (key.exists())
                    yield return new NvidiaControllerResult<TelemetryRegistryKey>(telemetryRegistryKey, error) { Name = key.Name };
                else
                    yield return new NvidiaControllerResult<TelemetryRegistryKey>(null, new RegistryKeyNotFoundException($"Failed to find registry key: {key.Name}"));
            }
        }

        /// <summary>
        ///     Disables automatic startup for the provided service.
        /// </summary>
        /// <param name="telemetryService">The service to disable automatic startup for.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryService> DisableTelemetryServiceStartup(TelemetryService telemetryService)
        {
            try
            {
                var modified = false;

                // set service startup to disabled
                if (ServiceHelper.GetServiceStartMode(telemetryService.Service) != ServiceStartMode.Disabled)
                {
                    ServiceHelper.ChangeStartMode(telemetryService.Service, ServiceStartMode.Disabled);
                    modified = true;
                }

                return new NvidiaControllerResult<TelemetryService>(telemetryService) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryService>(telemetryService, ex);
            }
        }

        /// <summary>
        ///     Disables the provided service and waits for it to stop.
        /// </summary>
        /// <param name="telemetryService">The service to disable.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryService> DisableTelemetryService(TelemetryService telemetryService)
        {
            try
            {
                var modified = false;

                if (telemetryService.Service.Status == ServiceControllerStatus.Running)
                {
                    telemetryService.Service.Stop();
                    telemetryService.Service.WaitForStatus(ServiceControllerStatus.Stopped);
                    modified = true;
                }

                return new NvidiaControllerResult<TelemetryService>(telemetryService) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryService>(telemetryService, ex);
            }
        }

        /// <summary>
        ///     Disables the provided task.
        /// </summary>
        /// <param name="telemetryTask">The task to disable.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryTask> DisableTelemetryTask(TelemetryTask telemetryTask)
        {
            try
            {
                var modified = false;

                if (telemetryTask.Task.Enabled)
                {
                    telemetryTask.Task.Enabled = false;
                    modified = true;
                }


                return new NvidiaControllerResult<TelemetryTask>(telemetryTask) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryTask>(telemetryTask, ex);
            }
        }

        /// <summary>
        ///     Disables the provided registry keys and its respective value(s).
        /// </summary>
        /// <param name="telemetryRegistryKey">The registry key to disable.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryRegistryKey> DisableTelemetryRegistryItem(TelemetryRegistryKey telemetryRegistryKey)
        {
            try
            {
                var modified = false;

                if (telemetryRegistryKey.IsActive())
                {
                    telemetryRegistryKey.Enabled = false;
                    modified = true;
                }


                return new NvidiaControllerResult<TelemetryRegistryKey>(telemetryRegistryKey) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryRegistryKey>(telemetryRegistryKey, ex);
            }
        }

        /// <summary>
        ///     Enables automatic startup for the provided service.
        /// </summary>
        /// <param name="telemetryService">The service to enable automatic startup for.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryService> EnableTelemetryServiceStartup(TelemetryService telemetryService)
        {
            try
            {
                var modified = false;

                // set service startup to automatic
                if (ServiceHelper.GetServiceStartMode(telemetryService.Service) != ServiceStartMode.Automatic)
                {
                    ServiceHelper.ChangeStartMode(telemetryService.Service, ServiceStartMode.Automatic);
                    modified = true;
                }

                return new NvidiaControllerResult<TelemetryService>(telemetryService) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryService>(telemetryService, ex);
            }
        }

        /// <summary>
        ///     Enables the provided service and waits for it to start.
        /// </summary>
        /// <param name="telemetryService">The service to enable.</param>
        /// <returns></returns>
        public static NvidiaControllerResult<TelemetryService> EnableTelemetryService(TelemetryService telemetryService)
        {
            try
            {
                var modified = false;

                if (telemetryService.Service.Status != ServiceControllerStatus.Running)
                {
                    telemetryService.Service.Start();
                    telemetryService.Service.WaitForStatus(ServiceControllerStatus.Running);
                    modified = true;
                }

                return new NvidiaControllerResult<TelemetryService>(telemetryService) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryService>(telemetryService, ex);
            }
        }

        /// <summary>
        ///     Enables the provided task.
        /// </summary>
        /// <param name="telemetryTask">The task to enable.</param>
        public static NvidiaControllerResult<TelemetryTask> EnableTelemetryTask(TelemetryTask telemetryTask)
        {
            try
            {
                var modified = false;

                if (telemetryTask.Task != null && !telemetryTask.Task.Enabled)
                {
                    telemetryTask.Task.Enabled = true;
                    modified = true;
                }


                return new NvidiaControllerResult<TelemetryTask>(telemetryTask) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryTask>(telemetryTask, ex);
            }
        }


        /// <summary>
        ///     Enables the provided registry key and its respective value(s).
        /// </summary>
        /// <param name="key">The registry key in which to enable.</param>
        public static NvidiaControllerResult<TelemetryRegistryKey> EnableTelemetryRegistryItem(TelemetryRegistryKey key)
        {
            try
            {
                var modified = false;

                if (!key.IsActive())
                {
                    key.Enabled = true;
                    modified = true;
                }

                return new NvidiaControllerResult<TelemetryRegistryKey>(key) {Modified = modified};
            }

            catch (Exception ex)
            {
                return new NvidiaControllerResult<TelemetryRegistryKey>(key, ex);
            }
        }
    }
}