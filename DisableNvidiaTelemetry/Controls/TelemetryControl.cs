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
        private readonly List<CheckBox> _telemetryCheckBoxes = new List<CheckBox>();
        private bool _suppressEvents;

        public TelemetryControl(string labelText)
        {
            InitializeComponent();
            chkDisableAll.Text = labelText;
        }

        public class TelemetrySelection
        {
            public ITelemetry Telemetry { get; }

            public bool Enabled { get; }

            public TelemetrySelection(ITelemetry telemetry, bool enabled)
            {
                Telemetry = telemetry;
                Enabled = enabled;
            }
        }

        public ReadOnlyCollection<ITelemetry> TelemetryItems => _telemetryItems.AsReadOnly();


        public ReadOnlyCollection<TelemetrySelection> SelectedItems
        {
            get
            {
                if (_telemetryItems.Count > 0)
                {
                    var items = _telemetryItems.Select((t, i) => new TelemetrySelection(t, !_telemetryCheckBoxes[i].Checked)).ToList();
                    return items.AsReadOnly();
                }

                return new List<TelemetrySelection>().AsReadOnly();
            }
        }

        public void AddTelemetryItem(ITelemetry telemetry, string displayText)
        {
            _telemetryItems.Add(telemetry);

            progressBar1.Maximum = _telemetryItems.Count;

            var cb = new CheckBox
            {
                AutoSize = true,
                Text = displayText,
                ForeColor = telemetry.IsRunning() ? SystemColors.ControlText : SystemColors.ControlDark
            };

            cb.Location = new Point(15, cb.Height * (_telemetryItems.Count - 1));
            cb.Checked = !telemetry.IsRunning();
            _telemetryCheckBoxes.Add(cb);
            panel1.Controls.Add(cb);

            cb.CheckStateChanged += (s, e) =>
            {
                if (_suppressEvents)
                    return;

                // check/uncheck parent based on children
                var count = _telemetryCheckBoxes.Count(c => c.Checked);
                _suppressEvents = true;
                chkDisableAll.Checked = count == _telemetryCheckBoxes.Count;
                _suppressEvents = false;
                cb.ForeColor = cb.Checked ? SystemColors.ControlDark : SystemColors.ControlText;

                CheckStateChanged?.Invoke(this, EventArgs.Empty);
            };

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
            chkDisableAll.CheckState = allDisabled ? CheckState.Checked : CheckState.Unchecked;
        }

        private void chkDisableAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            _suppressEvents = true;

            foreach (var cb in _telemetryCheckBoxes)
            {
                _suppressEvents = true;
                cb.Checked = chkDisableAll.Checked;
                cb.ForeColor = cb.Checked ? SystemColors.ControlDark : SystemColors.ControlText;
                _suppressEvents = false;
            }

            _suppressEvents = false;

            CheckStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            _suppressEvents = true;

            _telemetryItems.Clear();
            _telemetryCheckBoxes.Clear();
            progressBar1.Value = 0;
            panel1.Controls.Clear();
            chkDisableAll.CheckState = CheckState.Unchecked;

            _suppressEvents = false;
        }
    }
}