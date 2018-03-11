using System;
using System.IO;
using System.Linq;
using System.Windows;
using DisableNvidiaTelemetry.Controller;
using DisableNvidiaTelemetry.Utilities;
using DisableNvidiaTelemetryWPF.Properties;
using DisableNvidiaTelemetryWPF.Utilities;
using log4net.Core;

namespace DisableNvidiaTelemetryWPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static void SilentlyDisableTelemetry()
        {
            Logging.GetFileLogger().Log(Level.Info, DisableNvidiaTelemetryWPF.Properties.Resources.Silently_disabling_telemetry_services);

            foreach (var serviceResult in NvidiaController.EnumerateTelemetryServices().ToList())
            {
                if (serviceResult.Error == null)
                {
                    var startupResult = NvidiaController.DisableTelemetryServiceStartup(serviceResult.Item);
                    Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Disable_service_startup_failed}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Automatic_service_startup_disabled}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                    Logging.GetEventLogger().Log(Level.Info, startupResult.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Disable_service_startup_failed}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Automatic_service_startup_disabled}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");

                    var result = NvidiaController.DisableTelemetryService(serviceResult.Item);
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_stop_service}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Service_stopped}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_stop_service}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Service_stopped}: {serviceResult.Item.Service.DisplayName} ({serviceResult.Item.Service.ServiceName})");
                }
            }

            Logging.GetFileLogger().Log(Level.Info, DisableNvidiaTelemetryWPF.Properties.Resources.Silently_disabling_telemetry_tasks);

            foreach (var taskResult in NvidiaController.EnumerateTelemetryTasks().ToList())
            {
                if (taskResult.Error == null)
                {
                    var result = NvidiaController.DisableTelemetryTask(taskResult.Item);

                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_disable_task}: {result.Item.Task.Path}"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Task_disabled}: {result.Item.Task.Path}");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_disable_task}: {result.Item.Task.Path}"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Task_disabled}: {result.Item.Task.Path}");
                }

                else
                {
                    Logging.GetFileLogger().Log(Level.Error, taskResult.Error);
                    Logging.GetEventLogger().Log(Level.Error, taskResult.Error);
                }
            }

            Logging.GetFileLogger().Log(Level.Info, DisableNvidiaTelemetryWPF.Properties.Resources.Silently_disabling_telemetry_registry_items);

            foreach (var registryResult in NvidiaController.EnumerateTelemetryRegistryItems().ToList())
            {
                if (registryResult.Error == null)
                {
                    var result = NvidiaController.DisableTelemetryRegistryItem(registryResult.Item);

                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_disable_registry_item}: {result.Item.Name}"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Registry_item_disabled}: {result.Item.Name}");
                    Logging.GetEventLogger().Log(Level.Info, result.Error != null
                        ? $"{DisableNvidiaTelemetryWPF.Properties.Resources.Failed_to_disable_registry_item}: {result.Item.Name}"
                        : $"{DisableNvidiaTelemetryWPF.Properties.Resources.Registry_item_disabled}: {result.Item.Name}");
                }
            }
        }


        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppUtils.InitializeSettings();

#if PORTABLE
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Settings.Default.FileLogging = false;
            Settings.Default.Save();
#else
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DisableNvidiaTelemetryWPF.Properties.Resources.Disable_Nvidia_Telemetry);
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            var logDirectory = Path.Combine(appData, "Logs");
#endif

            Logging.Prepare(logDirectory);
            Logging.Enabled = Settings.Default.FileLogging;

            // log all the errors
            AppDomain.CurrentDomain.UnhandledException += (s, ee) =>
            {
                var ex = (Exception) ee.ExceptionObject;
                Logging.GetFileLogger().Log(Level.Error, ex.Message, ex);
            };

            var showUI = true;

            if (e.Args.Length > 0)
            {
                Logging.GetFileLogger().Log(Level.Info, $"Startup params: {string.Join(" ", e.Args)}");

                if (e.Args.Contains(AppUtils.StartupParamSilent))
                {
                    showUI = false;
                    SilentlyDisableTelemetry();
                }

                if (e.Args.Contains(AppUtils.StartupParamRegisterTask))
                {
                    showUI = false;

                    if (TaskSchedulerUtilities.GetTask() == null)
                        TaskSchedulerUtilities.Create((TaskSchedulerUtilities.TaskTrigger) Settings.Default.BackgroundTaskTrigger);
                }

                if (e.Args.Contains(AppUtils.StartupParamUnregisterTask))
                {
                    showUI = false;
                    TaskSchedulerUtilities.Remove();
                }
            }

            if (!AppUtils.IsAdministrator())
                MessageBox.Show(DisableNvidiaTelemetryWPF.Properties.Resources.Please_run_the_program_as_administrator_to_continue,
                    DisableNvidiaTelemetryWPF.Properties.Resources.AdministratorRequired, MessageBoxButton.OK, MessageBoxImage.Error);

            if (showUI)
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        }
    }
}