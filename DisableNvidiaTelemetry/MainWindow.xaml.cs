using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using DisableNvidiaTelemetry.Controller;
using DisableNvidiaTelemetry.Model;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using DisableNvidiaTelemetry.View;
using ExtendedVersion;
using log4net.Core;
using WPFCustomMessageBox;

namespace DisableNvidiaTelemetry
{
    public partial class MainWindow : Window
    {
        private readonly List<Logging.LogEvent> _logEvents = new List<Logging.LogEvent>();
        private bool _ignoreTaskSetting;
        private List<TelemetryRegistryKey> _telemetryKeys = new List<TelemetryRegistryKey>();
        private List<TelemetryService> _telemetryServices = new List<TelemetryService>();
        private List<TelemetryTask> _telemetryTasks = new List<TelemetryTask>();

        public MainWindow()
        {
            InitializeComponent();

            lvLogs.ItemsSource = _logEvents;

            LogExtensions.LogEvent += OnLogEvent;
            UpdaterUtilities.UpdateResponse += UpdaterUtilities_UpdateResponse;

            var driverVersion = GeforceUtilities.GetDriverVersion();
            Logging.GetFileLogger().Log(Level.Info, $"Driver Version: {(driverVersion != null ? driverVersion.ToString() : "N/A")}");

            var gfeVersion = GeforceUtilities.GetGeForceExperienceVersion();
            Logging.GetFileLogger().Log(Level.Info, $"GeForce Experience Version: {(gfeVersion != null ? gfeVersion.ToString() : "N/A")}");

            CheckBackgroundTask();

            chkFileLogging.IsChecked = Settings.Default.FileLogging;
            chkUpdates.IsChecked = Settings.Default.StartupUpdate;
            cbTaskTrigger.SelectedIndex = Settings.Default.BackgroundTaskTrigger;

            if (Logging.IsReadOnly)
            {
                chkFileLogging.IsEnabled = false;
                chkFileLogging.Content += $" ({Properties.Resources.Logging_disabled_on_read_only_device})";
            }

            if (Settings.Default.StartupUpdate)
            {
                btnUpdateCheck.IsEnabled = false;
                UpdaterUtilities.UpdateCheck(false);
            }

            var version = AppUtils.GetVersion();

            tbVersion.Inlines.Clear();
            tbVersion.Inlines.Add(new Run($"{Properties.Resources.Version} {version.ToString(ExtendedVersionFormatFlags.BuildString | ExtendedVersionFormatFlags.Truncated)} "));

            Title += $" v{version.ToString(ExtendedVersionFormatFlags.BuildString | ExtendedVersionFormatFlags.Truncated)} ";

            if (version.Commit != null)
            {
                var link = new Hyperlink(new Run(version.Commit.ToShorthandString()))
                {
                    NavigateUri = new Uri($"{Properties.Resources.GithubUrl}/commit/{AppUtils.GetVersion().Commit}")
                };
                link.RequestNavigate += Hyperlink_OnRequestNavigate;
                tbVersion.Inlines.Add(link);
            }

#if PORTABLE

            Title += $" ({Properties.Resources.Portable})";

#endif
        }

        private void UpdaterUtilities_UpdateResponse(object sender, UpdaterUtilities.UpdateResponseEventArgs e)
        {
            var showDialog = (bool) e.UserToken;

            if (e.Error == null)
            {
                var current = AppUtils.GetVersion().ToVersion();

                if (e.LatestVersion > current)
                {
                    var result = CustomMessageBox.Show(Properties.Resources.Update_available_message, Properties.Resources.Update_available, MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                        Process.Start(e.Url.ToString());
                }

                else if (showDialog)
                {
                    CustomMessageBox.Show(
                        Properties.Resources.No_updates_available_message,
                        Properties.Resources.Updates);
                }
            }

            else
            {
                Logging.GetFileLogger().Log(Level.Error, e.Error, suppressEvents: true);

                if (showDialog)
                {
                    CustomMessageBox.Show(
                        Properties.Resources.Update_error_messsage,
                        Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            btnUpdateCheck.IsEnabled = true;
        }

        private void CheckBackgroundTask()
        {
            _ignoreTaskSetting = true;

            chkBackgroundTask.IsChecked = TaskSchedulerUtilities.GetTask() != null;

            _ignoreTaskSetting = false;
        }

        private void OnLogEvent(object sender, LogExtensions.LogEventArgs e)
        {
            if (e.Log.Equals(Logging.GetFileLogger()))
            {
                _logEvents.Add(e.Event);
                lvLogs.Items.Refresh();
                lvLogs.SelectedItem = _logEvents.Last();
                lvLogs.ScrollIntoView(_logEvents.Last());
            }
        }

        private void RefreshTelemetryTasks(bool logging)
        {
            var tasks = new List<TelemetryTask>();

            foreach (var result in NvidiaController.EnumerateTelemetryTasks())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Properties.Resources.Failed_to_find_task}: {result.Name}"
                        : $"{Properties.Resources.Found_task}: {result.Item.Task.Name}");

                if (result.Error == null)
                {
                    var task = result.Item;

                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, task.Task.Enabled
                            ? $"{Properties.Resources.Task_is}: {Properties.Resources.Enabled}"
                            : $"{Properties.Resources.Task_is}: {Properties.Resources.Disabled}");

                    tcTasks.AddTelemetryItem(task, $"{Properties.Resources.Task}: {task.Task.Path}");
                    tasks.Add(task);
                }
            }

            _telemetryTasks = tasks;
        }

        private void RefreshTelemetryServices(bool logging)
        {
            var services = new List<TelemetryService>();

            foreach (var result in NvidiaController.EnumerateTelemetryServices())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Properties.Resources.Failed_to_find_service}: {result.Name}"
                        : $"{Properties.Resources.Found_service}: {result.Item.Service.DisplayName} ({result.Item.Service.ServiceName})");

                if (result.Error == null)
                {
                    var service = result.Item;
                    var running = service.Service.Status == ServiceControllerStatus.Running;
                    var startupMode = ServiceHelper.GetServiceStartMode(service.Service);

                    var startupModeString = "";

                    switch (startupMode)
                    {
                        case ServiceStartMode.Manual:
                            startupModeString = Properties.Resources.Manual;
                            break;
                        case ServiceStartMode.Automatic:
                            startupModeString = Properties.Resources.Automatic;
                            break;
                        case ServiceStartMode.Disabled:
                            startupModeString = Properties.Resources.Disabled;
                            break;
                    }

                    if (logging)
                    {
                        Logging.GetFileLogger().Log(Level.Info, running
                            ? $"{Properties.Resources.Service_is}: {Properties.Resources.Enabled}"
                            : $"{Properties.Resources.Service_is}: {Properties.Resources.Disabled}");

                        Logging.GetFileLogger().Log(Level.Info, $"{Properties.Resources.Service_startup_mode}: {startupModeString}");
                    }

                    tcServices.AddTelemetryItem(service, $"{Properties.Resources.Service}: {service.Service.DisplayName} ({service.Service.ServiceName})");
                    services.Add(service);
                }
            }

            _telemetryServices = services;
        }

        private void RefreshTelemetryRegistry(bool logging)
        {
            var keys = new List<TelemetryRegistryKey>();

            foreach (var result in NvidiaController.EnumerateTelemetryRegistryItems())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Properties.Resources.Failed_to_find_registry_item}: {result.Name}"
                        : $"{Properties.Resources.Found_registry_item}: {result.Item.Name}");

                if (result.Error == null)
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"{Properties.Resources.Registry_item_is}: {Properties.Resources.Enabled}");

                    var key = result.Item;

                    var sb = new StringBuilder();

                    sb.AppendLine(key.Name);

                    foreach (var vd in key.GetValues())
                    {
                        sb.Append("@=\"");
                        sb.Append(vd.Key);
                        sb.Append("\"");
                        sb.AppendLine();
                    }

                    tcRegistry.AddTelemetryItem(key, sb.ToString());
                    keys.Add(key);
                }
            }

            _telemetryKeys = keys;
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
            RefreshTelemetryRegistry(true);
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Resources.PaypalUrl);
        }

        private void chkBackgroundTask_Checked(object sender, RoutedEventArgs e)
        {
            cbTaskTrigger.IsEnabled = chkBackgroundTask.IsChecked.Value;

            if (_ignoreTaskSetting)
                return;

            if (chkBackgroundTask.IsChecked.Value)
                TaskSchedulerUtilities.Create((TaskSchedulerUtilities.TaskTrigger) Settings.Default.BackgroundTaskTrigger);
            else
                TaskSchedulerUtilities.Remove();
        }

        private void btnUpdatecheck_Click(object sender, RoutedEventArgs e)
        {
            btnUpdateCheck.IsEnabled = false;
            UpdaterUtilities.UpdateCheck(true);
        }

        private void cbTaskTrigger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.BackgroundTaskTrigger = cbTaskTrigger.SelectedIndex;
            Settings.Default.Save();
        }

        private void ChkFileLogging_OnClick(object sender, RoutedEventArgs e)
        {
            Logging.Enabled = chkFileLogging.IsChecked.Value;
            Settings.Default.FileLogging = chkFileLogging.IsChecked.Value;
            Settings.Default.Save();
        }

        private void ChkUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.StartupUpdate = chkUpdates.IsChecked.Value;
            Settings.Default.Save();
        }

        private void BtnGithub_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Resources.GithubUrl);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((Logging.LogEvent) lvLogs.SelectedItem).Message.ToString());
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void tcServices_RefreshClicked(object sender, EventArgs e)
        {
            tcServices.Reset();
            RefreshTelemetryServices(true);
        }

        private void tcTasks_RefreshClicked(object sender, EventArgs e)
        {
            tcTasks.Reset();
            RefreshTelemetryTasks(true);
        }

        private void tcRegistry_RefreshClicked(object sender, EventArgs e)
        {
            tcRegistry.Reset();
            RefreshTelemetryRegistry(true);
        }

        private void tcServices_DefaultClicked(object sender, EventArgs e)
        {
            tcServices.Reset();

            foreach (var item in _telemetryServices)
            {
                var result = NvidiaController.EnableTelemetryServiceStartup(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Automatic_service_startup_failed}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Properties.Resources.Automatic_service_startup_enabled}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            foreach (var item in _telemetryServices)
            {
                var result = NvidiaController.EnableTelemetryService(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_start_service}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Properties.Resources.Service_started}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            RefreshTelemetryServices(false);
        }

        private void tcTasks_DefaultClicked(object sender, EventArgs e)
        {
            tcTasks.Reset();

            foreach (var item in _telemetryTasks)
            {
                var result = NvidiaController.EnableTelemetryTask(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_enable_task}: {result.Item.Task.Path}"
                    : $"{Properties.Resources.Task_enabled}: {result.Item.Task.Path}");
            }

            foreach (var item in _telemetryKeys)
            {
                var result = NvidiaController.EnableTelemetryRegistryItem(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_enable_registry_item}: {result.Item.Name}"
                    : $"{Properties.Resources.Registry_item_enabled}: {result.Item.Name}");
            }

            RefreshTelemetryTasks(false);
        }

        private void tcRegistry_DefaultClicked(object sender, EventArgs e)
        {
            tcRegistry.Reset();

            foreach (var item in _telemetryKeys)
            {
                var result = NvidiaController.EnableTelemetryRegistryItem(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_enable_registry_item}: {result.Item.Name}"
                    : $"{Properties.Resources.Registry_item_enabled}: {result.Item.Name}");
            }

            RefreshTelemetryRegistry(false);
        }

        private void TcRegistry_OnTelemetryChanging(object sender, TelemetryControl.TelemetryEventArgs e)
        {
            var telemetry = (TelemetryRegistryKey) e.Telemetry;

            if (e.Enabled)
            {
                var result = NvidiaController.EnableTelemetryRegistryItem(telemetry);

                if (result.Error != null)
                {
                    e.Cancel = true;
                    CustomMessageBox.Show(
                        Properties.Resources.Failed_to_enable_registry_item,
                        Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_enable_registry_item}: {result.Item.Name}"
                    : $"{Properties.Resources.Registry_item_enabled}: {result.Item.Name}");
            }

            else
            {
                var result = NvidiaController.DisableTelemetryRegistryItem(telemetry);

                if (result.Error != null)
                {
                    e.Cancel = true;
                    CustomMessageBox.Show(
                        Properties.Resources.Failed_to_disable_registry_item,
                        Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_disable_registry_item}: {result.Item.Name}"
                    : $"{Properties.Resources.Registry_item_disabled}: {result.Item.Name}");
            }
        }

        private void TcServices_OnTelemetryChanging(object sender, TelemetryControl.TelemetryEventArgs e)
        {
            var telemetry = (TelemetryService) e.Telemetry;

            if (e.Enabled)
            {
                var startupResult = NvidiaController.EnableTelemetryServiceStartup(telemetry);

                Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null
                    ? $"{Properties.Resources.Automatic_service_startup_failed}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})"
                    : $"{Properties.Resources.Automatic_service_startup_enabled}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})");

                var result = NvidiaController.EnableTelemetryService(telemetry);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_start_service}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})"
                    : $"{Properties.Resources.Service_started}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})");
            }

            else
            {
                var startupResult = NvidiaController.DisableTelemetryServiceStartup(telemetry);

                Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null
                    ? $"{Properties.Resources.Disable_service_startup_failed}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})"
                    : $"{Properties.Resources.Automatic_service_startup_disabled}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})");

                var result = NvidiaController.DisableTelemetryService(telemetry);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_stop_service}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})"
                    : $"{Properties.Resources.Service_stopped}: {telemetry.Service.DisplayName} ({telemetry.Service.ServiceName})");
            }
        }

        private void TcTasks_OnTelemetryChanging(object sender, TelemetryControl.TelemetryEventArgs e)
        {
            var telemetry = (TelemetryTask) e.Telemetry;

            if (e.Enabled)
            {
                var result = NvidiaController.EnableTelemetryTask(telemetry);

                if (result.Error != null)
                    e.Cancel = true;

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_enable_task}: {result.Item.Task.Path}"
                    : $"{Properties.Resources.Task_enabled}: {result.Item.Task.Path}");
            }

            else
            {
                var result = NvidiaController.DisableTelemetryTask(telemetry);

                if (result.Error != null)
                    e.Cancel = true;

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Properties.Resources.Failed_to_disable_task}: {result.Item.Task.Path}"
                    : $"{Properties.Resources.Task_disabled}: {result.Item.Task.Path}");
            }
        }

        private void OnTelemetryChanged(object sender, TelemetryControl.TelemetryEventArgs e)
        {
            if (e.Telemetry.RestartRequired)
                Task.Factory.StartNew(() => Thread.Sleep(200))
                    .ContinueWith(t => { PromptRestart(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static void PromptRestart()
        {
            var result = CustomMessageBox.ShowYesNo(
                Properties.Resources.Restart_required_message,
                Properties.Resources.Restart_requried,
                Properties.Resources.Restart_now,
                Properties.Resources.Restart_later);

            if (result == MessageBoxResult.Yes)
                WindowsUtils.Restart();
        }
    }
}