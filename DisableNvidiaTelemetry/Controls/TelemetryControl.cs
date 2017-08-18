#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#endregion

namespace DisableNvidiaTelemetry.Controls
{
    internal partial class TelemetryControl : UserControl
    {
        private readonly List<ITelemetry> _telemetryItems = new List<ITelemetry>();
        private bool _suppressEvents;

        public TelemetryControl(string labelText)
        {
            InitializeComponent();
            chkDisableTelemetry.Text = labelText;
        }

        public ReadOnlyCollection<ITelemetry> TelemetryItems => _telemetryItems.AsReadOnly();

        public CheckState CheckState
        {
            get => chkDisableTelemetry.CheckState;
            set => chkDisableTelemetry.CheckState = value;
        }

        public void AddTelemetryItem(ITelemetry telemetry, string displayText)
        {
            _telemetryItems.Add(telemetry);

            progressBar1.Maximum = _telemetryItems.Count;

            var lbl = new Label
            {
                AutoSize = true,
                Text = displayText,
                ForeColor = telemetry.IsRunning() ? SystemColors.ControlText : SystemColors.ControlDark,
                Location = new Point(0, lblStatus.Height * (_telemetryItems.Count - 1))
            };
            panel1.Controls.Add(lbl);

            UpdateStatus();
        }

        public event EventHandler CheckStateChanged;

        private void UpdateStatus()
        {
            var disabledCount = _telemetryItems.Count(x => !x.IsRunning());

            progressBar1.Value = disabledCount;

            if (lblStatus.Visible == false)
                lblStatus.Visible = true;

            var allDisabled = disabledCount == _telemetryItems.Count;
            lblStatus.Text = allDisabled ? "All disabled" : $"{disabledCount} of {_telemetryItems.Count} disabled";
            chkDisableTelemetry.CheckState = allDisabled ? CheckState.Checked : CheckState.Unchecked;
        }

        private void chkDisableTelemetry_CheckStateChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            CheckStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            _suppressEvents = true;

            _telemetryItems.Clear();
            progressBar1.Value = 0;
            panel1.Controls.Clear();
            chkDisableTelemetry.CheckState = CheckState.Unchecked;

            _suppressEvents = false;
        }
    }
}