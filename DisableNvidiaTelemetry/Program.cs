#region

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Forms;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using log4net.Core;

#endregion

namespace DisableNvidiaTelemetry
{
    internal static class Program
    {
        public const string StartupParamSilent = "-silent";
        public const string StartupParamRegisterTask = "-registertask";
        public const string StartupParamUnregisterTask = "-unregistertask";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitializeSettings();

#if PORTABLE
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Settings.Default.FileLogging = false; 
            Settings.Default.Save();
#else
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Resources.Disable_Nvidia_Telemetry);
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            var logDirectory = Path.Combine(appData, "Logs");
#endif

            Logging.Prepare(logDirectory);
            Logging.Enabled = Settings.Default.FileLogging;

            // log all the errors
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception) e.ExceptionObject;
                Logging.GetFileLogger().Log(Level.Error, ex.Message, ex);
            };

            var silentMode = false;

            if (args.Length > 0)
            {
                Logging.GetFileLogger().Log(Level.Info, $"Startup params: {string.Join(" ", args)}");

                if (args.Contains(StartupParamSilent))
                {
                    silentMode = true;
                    SilentlyDisableTelemetry();
                }

                if (args.Contains(StartupParamRegisterTask))
                {
                    silentMode = true;

                    if (TaskSchedulerUtilities.GetTask() == null)
                        TaskSchedulerUtilities.Create((TaskSchedulerUtilities.TaskTrigger) Settings.Default.BackgroundTaskTrigger);
                }

                if (args.Contains(StartupParamUnregisterTask))
                {
                    silentMode = true;
                    TaskSchedulerUtilities.Remove();
                }
            }

            if (!IsAdministrator())
                MessageBox.Show(Resources.Please_run_the_program_as_administrator_to_continue, Resources.AdministratorRequired, MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (!silentMode)
                Application.Run(new FormMain());
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void SilentlyDisableTelemetry()
        {
            Logging.GetFileLogger().Log(Level.Info, Resources.Silently_disabling_telemetry_services);

            foreach (var serviceResult in NvidiaController.EnumerateTelemetryServices().ToList())
            {
                if (serviceResult.Error == null)
                {
                    var startupResult = NvidiaController.DisableTelemetryServiceStartup(serviceResult.Item);
                    Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null ? $"{Resources.Disable_service_startup_failed}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})" : $"{Resources.Automatic_service_startup_disabled}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                    Logging.GetEventLogger().Log(Level.Info, startupResult.Error != null ? $"{Resources.Disable_service_startup_failed}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})" : $"{Resources.Automatic_service_startup_disabled}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");

                    var result = NvidiaController.DisableTelemetryService(serviceResult.Item);
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_stop_service}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})" : $"{Resources.Service_stopped}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_stop_service}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})" : $"{Resources.Service_stopped}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                }
            }

            Logging.GetFileLogger().Log(Level.Info, Resources.Silently_disabling_telemetry_tasks);

            foreach (var taskResult in NvidiaController.EnumerateTelemetryTasks().ToList())
            {
                if (taskResult.Error == null)
                {
                    var result = NvidiaController.DisableTelemetryTask(taskResult.Item);

                    Logging.GetFileLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_disable_task}: {result.Item.Task.Path}" : $"{Resources.Task_disabled}: {result.Item.Task.Path}");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_disable_task}: {result.Item.Task.Path}" : $"{Resources.Task_disabled}: {result.Item.Task.Path}");
                }

                else
                {
                    Logging.GetFileLogger().Log(Level.Error, taskResult.Error);
                    Logging.GetEventLogger().Log(Level.Error, taskResult.Error);
                }
            }

            Logging.GetFileLogger().Log(Level.Info, Resources.Silently_disabling_telemetry_registry_items);

            foreach (var registryResult in NvidiaController.EnumerateTelemetryRegistryItems().ToList())
            {
                if (registryResult.Error == null)
                {
                    var result = NvidiaController.DisableTelemetryRegistryItem(registryResult.Item);

                    Logging.GetFileLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_disable_registry_item}: {result.Item.Name}" : $"{Resources.Registry_item_disabled}: {result.Item.Name}");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null ? $"{Resources.Failed_to_disable_registry_item}: {result.Item.Name}" : $"{Resources.Registry_item_disabled}: {result.Item.Name}");
                }
            }
        }

        /// <summary>
        ///     Set custom settings provider here since it seems to break VS designer.
        /// </summary>
        private static void InitializeSettings()
        {
            var provider = new PortableSettingsProvider();
            Settings.Default.Providers.Add(provider);
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                property.Provider = provider;
            }
        }
    }
}