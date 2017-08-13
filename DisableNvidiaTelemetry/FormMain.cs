#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;
using DisableNvidiaTelemetry.Properties;
using Microsoft.Win32.TaskScheduler;

#endregion

namespace DisableNvidiaTelemetry
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
                DisableTelemetryServices();

            if (_tasksControl.CheckState == CheckState.Checked)
                DisableTelemetryTasks();

            RefreshControls();

            RefreshTelemetryServices(false);
            RefreshTelemetryTasks(false);
        }

        private void btnDefaults_Click(object sender, EventArgs e)
        {
            RefreshControls();

            EnableTelemetryServices();
            EnableTelemetryTasks();

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


        private void AppendLog(string message)
        {
            textBox1.AppendText($"[{DateTime.Now:T}] {message}{Environment.NewLine}");
        }

        private void DisableTelemetryTasks()
        {
            foreach (var task in _telemetryTasks)
            {
                try
                {
                    if (task.Enabled)
                    {
                        task.Enabled = false;
                        AppendLog($"Disabled task: {task.Path}");
                    }
                }

                catch
                {
                    AppendLog($"Failed to disable task: {task.Path}");
                }
            }
        }

        private void EnableTelemetryTasks()
        {
            foreach (var task in _telemetryTasks)
            {
                if (task != null)
                {
                    try
                    {
                        if (!task.Enabled)
                        {
                            task.Enabled = true;
                            AppendLog($"Enabled task: {task.Path}");
                        }
                    }

                    catch
                    {
                        AppendLog($"Failed to enable task: {task.Path}");
                    }
                }
            }
        }

        private void RefreshTelemetryTasks(bool logging)
        {
            var tasks = new List<Task>();
            var taskQueries = NvidiaController.GetTelemetryTasks();

            if (taskQueries.Count > 0)
            {
                foreach (var query in taskQueries)
                {
                    if (query.Result == null)
                    {
                        if (logging)
                            AppendLog($"Failed to find task: {query.Query}");
                    }

                    else
                    {
                        if (logging)
                        {
                            AppendLog($"Found Task: {query.Result.Name}");
                            AppendLog($"Task is: {(query.Result.Enabled ? "Enabled" : "Disabled")}");
                        }

                        if (!query.Result.Enabled)
                        {
                            _tasksControl.DisabledCount++;
                        }

                        _tasksControl.AddSubAction($"Task: {query.Result.Path}", query.Result.Enabled);

                        tasks.Add(query.Result);
                    }
                }
            }

            _tasksControl.Enabled = !_tasksControl.IsEmpty;

            _telemetryTasks = tasks;
        }

        private void RefreshTelemetryServices(bool logging)
        {
            var services = new List<ServiceController>();
            var serviceQueries = NvidiaController.GetTelemetryServices();

            if (serviceQueries.Count > 0)
            {
                foreach (var query in serviceQueries)
                {
                    var serviceFound = false;
                    var running = false;
                    try
                    {
                        running = query.Result.Status == ServiceControllerStatus.Running;
                        serviceFound = true;
                    }

                    catch
                    {
                        if (logging)
                            AppendLog($"Failed to find service: {query.Query}");
                    }

                    if (serviceFound)
                    {
                        if (logging)
                        {
                            AppendLog($"Found Service: {query.Result.DisplayName} ({query.Result.ServiceName})");
                            AppendLog($"Service is: {(running ? "Enabled" : "Disabled")}");
                        }

                        if (!running)
                            _servicesControl.DisabledCount++;

                        _servicesControl.AddSubAction($"Service: {query.Result.DisplayName} ({query.Result.ServiceName})", running);

                        services.Add(query.Result);
                    }
                }
            }

            _servicesControl.Enabled = !_servicesControl.IsEmpty;

            _telemetryServices = services;
        }

        private void EnableTelemetryServices()
        {
            foreach (var service in _telemetryServices)
            {
                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Automatic)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Automatic);

                        AppendLog($"Enabled automatic service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    AppendLog($"Failed to enable automatic service startup: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        AppendLog($"Enabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    AppendLog($"Failed to start service: {service.DisplayName} ({service.ServiceName})");
                }
            }
        }

        private void DisableTelemetryServices()
        {
            foreach (var service in _telemetryServices)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        AppendLog($"Disabled service: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    AppendLog($"Failed to disable service: {service.DisplayName} ({service.ServiceName})");
                }

                try
                {
                    if (ServiceHelper.GetServiceStartMode(service) != ServiceStartMode.Disabled)
                    {
                        ServiceHelper.ChangeStartMode(service, ServiceStartMode.Disabled);
                        AppendLog($"Disabled service startup: {service.DisplayName} ({service.ServiceName})");
                    }
                }

                catch
                {
                    AppendLog($"Failed to disable service startup: {service.DisplayName} ({service.ServiceName})");
                }
            }
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