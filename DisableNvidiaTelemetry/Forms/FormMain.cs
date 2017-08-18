#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Controls;
using DisableNvidiaTelemetry.Properties;
using DisableNvidiaTelemetry.Utilities;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry.Forms
{
    public partial class FormMain : Form
    {
        private readonly TelemetryControl _servicesControl;
        private readonly TelemetryControl _tasksControl;
        private List<ServiceController> _telemetryServices = new List<ServiceController>();
        private List<Task> _telemetryTasks = new List<Task>();

        public FormMain()
        {
            InitializeComponent();

            _tasksControl = new TelemetryControl("Telemetry Tasks") {Dock = DockStyle.Top};
            _tasksControl.CheckStateChanged += telemControl_CheckStateChanged;
            tabPage1.Controls.Add(_tasksControl);
            _servicesControl = new TelemetryControl("Telemetry Services") {Dock = DockStyle.Top};
            _servicesControl.CheckStateChanged += telemControl_CheckStateChanged;
            tabPage1.Controls.Add(_servicesControl);

            txtLicense.Text = Resources.ApplicationLicense;

            LogExtensions.LogEvent += OnLogEvent;
        }

        private void OnLogEvent(object sender, LogExtensions.LogEventArgs e)
        {
            textBox1.AppendText($"[{DateTime.Now:T}] {e.Message}{Environment.NewLine}");
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshControls();
            textBox1.Clear();

            RefreshTelemetryServices(true);
            RefreshTelemetryTasks(true);
        }

        private void telemControl_CheckStateChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = _tasksControl.CheckState == CheckState.Checked || _servicesControl.CheckState == CheckState.Checked;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (_servicesControl.CheckState == CheckState.Checked)
                NvidiaController.DisableTelemetryServices(_telemetryServices);

            if (_tasksControl.CheckState == CheckState.Checked)
                NvidiaController.DisableTelemetryTasks(_telemetryTasks);

            RefreshControls();

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            RefreshControls();

            NvidiaController.EnableTelemetryServices(_telemetryServices);
            NvidiaController.EnableTelemetryTasks(_telemetryTasks);

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);

            btnApply.Enabled = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRefresh.Visible = tabControl1.SelectedIndex <= 1;
            btnDefaults.Visible = tabControl1.SelectedIndex <= 1;
            btnApply.Visible = tabControl1.SelectedIndex <= 1;
        }

        private void RefreshControls()
        {
            _tasksControl.Reset();
            _servicesControl.Reset();
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

        private void lblCopyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://nateshoffner.com");
        }
        
        private void pbGithub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/NateShoffner/Disable-Nvidia-Telemetry");
        }
    }
}