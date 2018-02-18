#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Controls;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using ExtendedVersion;
using log4net.Core;
using Microsoft.Win32.TaskScheduler;

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
        private List<ServiceController> _telemetryServices = new List<ServiceController>();
        private List<Task> _telemetryTasks = new List<Task>();

        public FormMain()
        {
            InitializeComponent();

            _servicesControl = new TelemetryControl("Telemetry Services") {Dock = DockStyle.Top};
            _servicesControl.CheckStateChanged += telemControl_CheckStateChanged;

            _tasksControl = new TelemetryControl("Telemetry Tasks") {Dock = DockStyle.Top};
            _tasksControl.CheckStateChanged += telemControl_CheckStateChanged;

            _registryControl = new TelemetryControl("Telemetry Registry Entries") {Dock = DockStyle.Top};
            _registryControl.CheckStateChanged += telemControl_CheckStateChanged;

            tabPage1.Controls.Add(_registryControl);
            tabPage1.Controls.Add(_tasksControl);
            tabPage1.Controls.Add(_servicesControl);

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
            lblVersion.Text = $"{"Version"} {version.ToString(ExtendedVersionFormatFlags.BuildString | ExtendedVersionFormatFlags.CommitShort | ExtendedVersionFormatFlags.Truncated)}";
            lblVersion.LinkArea = version.Commit != null ? new LinkArea(lblVersion.Text.Length - version.Commit.ToShorthandString().Length, version.Commit.ToShorthandString().Length) : new LinkArea(0, 0);
        }

        private void UpdaterUtilities_UpdateResponse(object sender, UpdaterUtilities.UpdateResponseEventArgs e)
        {
            var showDialog = (bool) e.UserToken;

            if (e.Error == null)
            {
                var current = GetVersion().ToVersion();

                if (e.LatestVersion > current)
                {
                    var result = MessageBox.Show("A new update is available, would you like to download it?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                        Process.Start(e.Url.ToString());
                }

                else if (showDialog)
                {
                    MessageBox.Show("There are no updates available.", "No Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            else
            {
                Logging.GetFileLogger().Log(Level.Error, e.Error, suppressEvents: true);

                if (showDialog)
                    MessageBox.Show("There was an error while checking for updates.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                textBox1.AppendText($"[{DateTime.Now:T}] {e.Message}{Environment.NewLine}");
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
            textBox1.Clear();

            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
            RefreshTelemetryRegistry(true);
        }

        private void telemControl_CheckStateChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = _tasksControl.SelectedItems.Count > 0 || _servicesControl.SelectedItems.Count > 0;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var selectedServices = _servicesControl.SelectedItems;

            var disabledServices = (from t in selectedServices where !t.Enabled select (TelemetryService) t.Telemetry).Select(s => s.Service).ToList();
            var enabledServices = (from t in selectedServices where t.Enabled select (TelemetryService) t.Telemetry).Select(s => s.Service).ToList();

            NvidiaController.DisableTelemetryServices(disabledServices, true, true);
            NvidiaController.EnableTelemetryServices(enabledServices, true);

            var selectedTasks = _tasksControl.SelectedItems;

            var disabledTasks = (from t in selectedTasks where !t.Enabled select (TelemetryTask) t.Telemetry).Select(s => s.Task).ToList();
            var enabledTasks = (from t in selectedTasks where t.Enabled select (TelemetryTask) t.Telemetry).Select(s => s.Task).ToList();

            NvidiaController.DisableTelemetryTasks(disabledTasks, true, true);
            NvidiaController.EnableTelemetryTasks(enabledTasks, true);

            var selectedKeys = _registryControl.SelectedItems;

            var disabledKeys = (from t in selectedKeys where !t.Enabled select (TelemetryRegistryKey) t.Telemetry).ToList();
            var enabledKeys = (from t in selectedKeys where t.Enabled select (TelemetryRegistryKey) t.Telemetry).ToList();

            NvidiaController.DisableTelemetryRegistryEntries(disabledKeys, true, true);
            NvidiaController.EnableTelemetryRegistryEntries(enabledKeys, true);

            RefreshControls();

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
            RefreshTelemetryRegistry(false);
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            RefreshControls();

            NvidiaController.EnableTelemetryServices(_telemetryServices, true);
            NvidiaController.EnableTelemetryTasks(_telemetryTasks, true);
            NvidiaController.EnableTelemetryRegistryEntries(_telemetryKeys, true);

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
            RefreshTelemetryRegistry(false);

            btnApply.Enabled = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRefresh.Visible = btnDefaults.Visible = btnApply.Visible = tabControl1.SelectedTab == tabPage1;
        }

        private void RefreshControls()
        {
            _tasksControl.Reset();
            _servicesControl.Reset();
            _registryControl.Reset();
        }

        private void RefreshTelemetryTasks(bool logging)
        {
            var tasks = NvidiaController.GetTelemetryTasks(logging);

            foreach (var task in tasks)
            {
                _tasksControl.AddTelemetryItem(task, $"Task: {task.Task.Path}");
            }

            _tasksControl.Enabled = _tasksControl.TelemetryItems.Count != 0;
            _telemetryTasks = tasks.Select(t => t.Task).ToList();
        }

        private void RefreshTelemetryServices(bool logging)
        {
            var services = NvidiaController.GetTelemetryServices(logging);

            foreach (var service in services)
            {
                _servicesControl.AddTelemetryItem(service, $"Service: {service.Service.DisplayName}");
            }

            _servicesControl.Enabled = _servicesControl.TelemetryItems.Count != 0;
            _telemetryServices = services.Select(s => s.Service).ToList();
        }

        private void RefreshTelemetryRegistry(bool logging)
        {
            var entries = NvidiaController.GetTelemetryRegistryEntires(logging);

            foreach (var entry in entries)
            {
                var sb = new StringBuilder();

                sb.AppendLine(entry.Name);

                foreach (var vd in entry.ValueData)
                {
                    sb.Append("@=\"");
                    sb.Append(vd.Key);
                    sb.Append("\"");
                    sb.AppendLine();
                }

                _registryControl.AddTelemetryItem(entry, sb.ToString());
            }

            _registryControl.Enabled = _registryControl.TelemetryItems.Count != 0;
            _telemetryKeys = entries.ToList();
        }

        private void lblCopyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://nateshoffner.com");
        }

        private void pbGithub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NateShoffner/Disable-Nvidia-Telemetry");
        }

        private void lblGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/NateShoffner/Disable-Nvidia-Telemetry");
        }

        private void pbDonate_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=nate.shoffner@gmail.com&lc=US&item_name=Disable%20Nvidia%20Telemetry&currency_code=USD&bn=PP%2dDonationsBF");
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
            Process.Start($"https://github.com/NateShoffner/Disable-Nvidia-Telemetry/commit/{GetVersion().Commit}");
        }

        private static ExtendedVersion.ExtendedVersion GetVersion()
        {
            var attribute =
                (AssemblyInformationalVersionAttribute) Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();
            var version = attribute != null ? new ExtendedVersion.ExtendedVersion(attribute.InformationalVersion) : new ExtendedVersion.ExtendedVersion(Application.ProductVersion);

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