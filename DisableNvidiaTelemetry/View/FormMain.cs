#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Controller;
using DisableNvidiaTelemetry.Controls;
using DisableNvidiaTelemetry.Model;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using ExtendedVersion;
using log4net.Core;

#endregion

namespace DisableNvidiaTelemetry.Forms
{
    public partial class FormMain : Form
    {
        private readonly TelemetryControl _registryControl;
        private readonly TelemetryControl _servicesControl;
        private readonly TelemetryControl _tasksControl;

        private bool _ignoreTaskSetting;
        private List<TelemetryRegistryKey> _telemetryKeys = new List<TelemetryRegistryKey>();
        private List<TelemetryService> _telemetryServices = new List<TelemetryService>();
        private List<TelemetryTask> _telemetryTasks = new List<TelemetryTask>();

        public FormMain()
        {
            InitializeComponent();

            _servicesControl = new TelemetryControl(Resources.Telemetry_serivces) {Dock = DockStyle.Top};
            _servicesControl.CheckStateChanged += telemControl_CheckStateChanged;

            _tasksControl = new TelemetryControl(Resources.Telemetry_tasks) {Dock = DockStyle.Top};
            _tasksControl.CheckStateChanged += telemControl_CheckStateChanged;

            _registryControl = new TelemetryControl(Resources.Telemetry_registry_items) {Dock = DockStyle.Top};
            _registryControl.CheckStateChanged += telemControl_CheckStateChanged;

            tabTelemetry.Controls.Add(_registryControl);
            tabTelemetry.Controls.Add(_tasksControl);
            tabTelemetry.Controls.Add(_servicesControl);

            txtLicense.Text = Resources.ApplicationLicense;

            LogExtensions.LogEvent += OnLogEvent;
            UpdaterUtilities.UpdateResponse += UpdaterUtilities_UpdateResponse;

            CheckBackgroundTask();

            chkFileLogging.Checked = Settings.Default.FileLogging;
            chkUpdates.Checked = Settings.Default.StartupUpdate;
            cbTaskTrigger.SelectedIndex = Settings.Default.BackgroundTaskTrigger;

            if (Settings.Default.StartupUpdate)
            {
                btnUpdatecheck.Enabled = false;
                UpdaterUtilities.UpdateCheck(false);
            }

            var version = GetVersion();
            lblVersion.Text = $"{Resources.Version} {version.ToString(ExtendedVersionFormatFlags.BuildString | ExtendedVersionFormatFlags.CommitShort | ExtendedVersionFormatFlags.Truncated)}";
            lblVersion.LinkArea = version.Commit != null
                ? new LinkArea(lblVersion.Text.Length - version.Commit.ToShorthandString().Length, version.Commit.ToShorthandString().Length)
                : new LinkArea(0, 0);
        }

        private void UpdaterUtilities_UpdateResponse(object sender, UpdaterUtilities.UpdateResponseEventArgs e)
        {
            var showDialog = (bool) e.UserToken;

            if (e.Error == null)
            {
                var current = GetVersion().ToVersion();

                if (e.LatestVersion > current)
                {
                    var result = MessageBox.Show(Resources.Update_available_message, Resources.Update_available, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                        Process.Start(e.Url.ToString());
                }

                else if (showDialog)
                {
                    MessageBox.Show(Resources.No_updates_available_message, Resources.No_Upades, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            else
            {
                Logging.GetFileLogger().Log(Level.Error, e.Error, suppressEvents: true);

                if (showDialog)
                    MessageBox.Show(Resources.Update_error_messsage, Resources.Update_error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnUpdatecheck.Enabled = true;
        }

        private void CheckBackgroundTask()
        {
            _ignoreTaskSetting = true;

            chkBackgroundTask.Checked = TaskSchedulerUtilities.GetTask() != null;

            _ignoreTaskSetting = false;
        }

        private void OnLogEvent(object sender, LogExtensions.LogEventArgs e)
        {
            if (e.Log.Equals(Logging.GetFileLogger()))
            {
                txtEventLog.AppendText($"[{DateTime.Now:T}] {e.Message}{Environment.NewLine}");
                txtEventLog.SelectionStart = txtEventLog.TextLength;
                txtEventLog.ScrollToCaret();
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
            RefreshTelemetryRegistry(true);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshControls();
            txtEventLog.Clear();

            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
            RefreshTelemetryRegistry(true);
        }

        private void telemControl_CheckStateChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = _tasksControl.SelectedItems.Count > 0 || _servicesControl.SelectedItems.Count > 0 || _registryControl.SelectedItems.Count > 0;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var selectedServices = _servicesControl.SelectedItems;
            var disabledServices = selectedServices.Where(t => !t.Enabled).Select(t => (TelemetryService) t.Telemetry);
            var enabledServices = selectedServices.Where(t => t.Enabled).Select(t => (TelemetryService) t.Telemetry);

            foreach (var item in disabledServices)
            {
                var startupResult = NvidiaController.DisableTelemetryServiceStartup(item);

                Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null
                    ? $"{Resources.Disable_service_startup_failed}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Automatic_service_startup_disabled}: {item.Service.DisplayName} ({item.Service.ServiceName})");

                var result = NvidiaController.DisableTelemetryService(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_stop_service}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Service_stopped}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            foreach (var item in enabledServices)
            {
                var startupResult = NvidiaController.DisableTelemetryServiceStartup(item);

                Logging.GetFileLogger().Log(Level.Info, startupResult.Error != null
                    ? $"{Resources.Automatic_service_startup_failed}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Automatic_service_startup_enabled}: {item.Service.DisplayName} ({item.Service.ServiceName})");

                var result = NvidiaController.EnableTelemetryService(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_start_service}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Service_started}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            var selectedTasks = _tasksControl.SelectedItems;
            var disabledTasks = selectedTasks.Where(t => !t.Enabled).Select(t => (TelemetryTask) t.Telemetry);
            var enabledTasks = selectedTasks.Where(t => t.Enabled).Select(t => (TelemetryTask) t.Telemetry);

            foreach (var item in disabledTasks)
            {
                var result = NvidiaController.DisableTelemetryTask(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_disable_task}: {result.Item.Task.Path}"
                    : $"{Resources.Task_disabled}: {result.Item.Task.Path}");
            }

            foreach (var item in enabledTasks)
            {
                var result = NvidiaController.EnableTelemetryTask(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_enable_task}: {result.Item.Task.Path}"
                    : $"{Resources.Task_enabled}: {result.Item.Task.Path}");
            }

            var selectedKeys = _registryControl.SelectedItems;
            var disabledKeys = selectedKeys.Where(t => !t.Enabled).Select(t => (TelemetryRegistryKey) t.Telemetry);
            var enabledKeys = selectedKeys.Where(t => t.Enabled).Select(t => (TelemetryRegistryKey) t.Telemetry);

            foreach (var item in disabledKeys)
            {
                var result = NvidiaController.DisableTelemetryRegistryItem(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_disable_registry_item}: {result.Item.Name}"
                    : $"{Resources.Registry_item_disabled}: {result.Item.Name}");
            }

            foreach (var item in enabledKeys)
            {
                var result = NvidiaController.EnableTelemetryRegistryItem(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_enable_registry_item}: {result.Item.Name}"
                    : $"{Resources.Registry_item_enabled}: {result.Item.Name}");
            }

            RefreshControls();

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
            RefreshTelemetryRegistry(false);
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            RefreshControls();

            foreach (var item in _telemetryServices)
            {
                var result = NvidiaController.EnableTelemetryServiceStartup(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Automatic_service_startup_failed}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Automatic_service_startup_enabled}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            foreach (var item in _telemetryServices)
            {
                var result = NvidiaController.EnableTelemetryService(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_start_service}: {item.Service.DisplayName} ({item.Service.ServiceName})"
                    : $"{Resources.Service_started}: {item.Service.DisplayName} ({item.Service.ServiceName})");
            }

            foreach (var item in _telemetryTasks)
            {
                var result = NvidiaController.EnableTelemetryTask(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_enable_task}: {result.Item.Task.Path}"
                    : $"{Resources.Task_enabled}: {result.Item.Task.Path}");
            }

            foreach (var item in _telemetryKeys)
            {
                var result = NvidiaController.EnableTelemetryRegistryItem(item);

                Logging.GetFileLogger().Log(Level.Info, result.Error != null
                    ? $"{Resources.Failed_to_enable_registry_item}: {result.Item.Name}"
                    : $"{Resources.Registry_item_enabled}: {result.Item.Name}");
            }

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
            RefreshTelemetryRegistry(false);

            btnApply.Enabled = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRefresh.Visible = btnDefaults.Visible = btnApply.Visible = tabControl1.SelectedTab == tabTelemetry;

            if (tabControl1.SelectedTab == tabLog)
            {
                txtEventLog.SelectionStart = txtEventLog.TextLength;
                txtEventLog.ScrollToCaret();
            }
        }

        private void RefreshControls()
        {
            _tasksControl.Reset();
            _servicesControl.Reset();
            _registryControl.Reset();
        }

        private void RefreshTelemetryTasks(bool logging)
        {
            var tasks = new List<TelemetryTask>();

            foreach (var result in NvidiaController.EnumerateTelemetryTasks())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Resources.Failed_to_find_task}: {result.Name}"
                        : $"{Resources.Found_task}: {result.Item.Task.Name}");

                if (result.Error == null)
                {
                    var task = result.Item;

                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, task.Task.Enabled
                            ? $"{Resources.Task_is}: {Resources.Enabled}"
                            : $"{Resources.Task_is}: {Resources.Disabled}");

                    _tasksControl.AddTelemetryItem(task, $"{Resources.Task}: {task.Task.Path}");
                    tasks.Add(task);
                }
            }

            _tasksControl.Enabled = _tasksControl.TelemetryItems.Count != 0;
            _telemetryTasks = tasks;
        }

        private void RefreshTelemetryServices(bool logging)
        {
            var services = new List<TelemetryService>();

            foreach (var result in NvidiaController.EnumerateTelemetryServices())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Resources.Failed_to_find_service}: {result.Name}"
                        : $"{Resources.Found_service}: {result.Item.Service.DisplayName} ({result.Item.Service.ServiceName})");

                if (result.Error == null)
                {
                    var service = result.Item;
                    var running = service.Service.Status == ServiceControllerStatus.Running;
                    var startupMode = ServiceHelper.GetServiceStartMode(service.Service);

                    var startupModeString = "";

                    switch (startupMode)
                    {
                        case ServiceStartMode.Manual:
                            startupModeString = Resources.Manual;
                            break;
                        case ServiceStartMode.Automatic:
                            startupModeString = Resources.Automatic;
                            break;
                        case ServiceStartMode.Disabled:
                            startupModeString = Resources.Disabled;
                            break;
                    }

                    if (logging)
                    {
                        Logging.GetFileLogger().Log(Level.Info, running
                            ? $"{Resources.Service_is}: {Resources.Enabled}"
                            : $"{Resources.Service_is}: {Resources.Disabled}");

                        Logging.GetFileLogger().Log(Level.Info, $"{Resources.Service_startup_mode}: {startupModeString}");
                    }

                    _servicesControl.AddTelemetryItem(service, $"{Resources.Service}: {service.Service.DisplayName}");
                    services.Add(service);
                }
            }

            _servicesControl.Enabled = _servicesControl.TelemetryItems.Count != 0;
            _telemetryServices = services;
        }

        private void RefreshTelemetryRegistry(bool logging)
        {
            var keys = new List<TelemetryRegistryKey>();

            foreach (var result in NvidiaController.EnumerateTelemetryRegistryItems())
            {
                if (logging)
                    Logging.GetFileLogger().Log(Level.Info, result.Error != null
                        ? $"{Resources.Failed_to_find_registry_item}: {result.Name}"
                        : $"{Resources.Found_registry_item}: {result.Item.Name}");

                if (result.Error == null)
                {
                    if (logging)
                        Logging.GetFileLogger().Log(Level.Info, $"{Resources.Registry_item_is}: {Resources.Enabled}");

                    var key = result.Item;

                    var sb = new StringBuilder();

                    sb.AppendLine(key.Name);

                    foreach (var vd in key.ValueData)
                    {
                        sb.Append("@=\"");
                        sb.Append(vd.Key);
                        sb.Append("\"");
                        sb.AppendLine();
                    }

                    _registryControl.AddTelemetryItem(key, sb.ToString());
                    keys.Add(key);
                }
            }

            _registryControl.Enabled = _registryControl.TelemetryItems.Count != 0;
            _telemetryKeys = keys;
        }

        private void lblCopyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Resources.Homepage);
        }

        private void pbGithub_Click(object sender, EventArgs e)
        {
            Process.Start(Resources.GithubUrl);
        }

        private void lblGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Resources.GithubUrl);
        }

        private void pbDonate_Click(object sender, EventArgs e)
        {
            Process.Start(Resources.PaypalUrl);
        }

        private void chkBackroundTask_CheckedChanged(object sender, EventArgs e)
        {
            cbTaskTrigger.Enabled = chkBackgroundTask.Checked;

            if (_ignoreTaskSetting)
                return;

            if (chkBackgroundTask.Checked)
                TaskSchedulerUtilities.Create((TaskSchedulerUtilities.TaskTrigger) Settings.Default.BackgroundTaskTrigger);
            else
                TaskSchedulerUtilities.Remove();
        }

        private void btnUpdatecheck_Click(object sender, EventArgs e)
        {
            btnUpdatecheck.Enabled = false;
            UpdaterUtilities.UpdateCheck(true);
        }

        private void chkUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.StartupUpdate = chkUpdates.Checked;
            Settings.Default.Save();
        }

        private void cbTaskTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.BackgroundTaskTrigger = cbTaskTrigger.SelectedIndex;
            Settings.Default.Save();
        }

        private void lblVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"{Resources.GithubUrl}/commit/{GetVersion().Commit}");
        }

        private static ExtendedVersion.ExtendedVersion GetVersion()
        {
            var attribute =
                (AssemblyInformationalVersionAttribute) Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();
            var version = attribute != null
                ? new ExtendedVersion.ExtendedVersion(attribute.InformationalVersion)
                : new ExtendedVersion.ExtendedVersion(Application.ProductVersion);

            return version;
        }

        private void chkFileLogging_CheckedChanged(object sender, EventArgs e)
        {
            Logging.Enabled = chkFileLogging.Checked;
            Settings.Default.FileLogging = chkFileLogging.Checked;
            Settings.Default.Save();
        }
    }
}