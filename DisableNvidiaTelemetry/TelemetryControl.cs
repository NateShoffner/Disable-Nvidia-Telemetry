#region

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace DisableNvidiaTelemetry
{
    public partial class TelemetryControl : UserControl
    {
        private bool _suppressEvents;
        private int _totalActions;

        public TelemetryControl(string labelText)
        {
            InitializeComponent();
            chkDisableTelemetry.Text = labelText;
        }

        public CheckState CheckState
        {
            get => chkDisableTelemetry.CheckState;
            set => chkDisableTelemetry.CheckState = value;
        }

        public int DisabledCount { set; get; }

        public bool IsEmpty => _totalActions == 0;

        public bool AllDisabled => DisabledCount == _totalActions;

        public event EventHandler CheckStateChanged;

        public void AddSubAction(string item, bool isEnabled)
        {
            _totalActions++;
            progressBar1.Maximum = _totalActions;

            var lbl = new Label {AutoSize = true, Text = item, ForeColor = isEnabled ? SystemColors.ControlText : SystemColors.ControlDark, Location = new Point(0, lblStatus.Height * (_totalActions - 1))};
            panel1.Controls.Add(lbl);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            progressBar1.Value = DisabledCount;

            if (lblStatus.Visible == false)
                lblStatus.Visible = true;

            lblStatus.Text = AllDisabled ? "All disabled" : $"{DisabledCount} of {_totalActions} disabled";
            chkDisableTelemetry.CheckState = AllDisabled ? CheckState.Checked : CheckState.Unchecked;
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

            DisabledCount = 0;
            _totalActions = 0;
            progressBar1.Value = 0;
            panel1.Controls.Clear();
            chkDisableTelemetry.CheckState = CheckState.Unchecked;

            _suppressEvents = false;
        }
    }
}