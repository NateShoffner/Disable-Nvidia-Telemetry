using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DisableNvidiaTelemetry.Model;

namespace DisableNvidiaTelemetry.View
{
    public partial class TelemetryControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TelemetryControl));
        private readonly List<ITelemetry> _telemetryItems = new List<ITelemetry>();
        private bool _suppressEvents;


        public TelemetryControl()
        {
            InitializeComponent();
            containerPanel.Loaded += (sender, e) => { MarginSetter.CreateThicknesForChildren(sender, new DependencyPropertyChangedEventArgs()); };

            Loaded += TelemetryControl_Loaded;

            lblRefresh.MouseLeftButtonDown += (sender, e) => { RefreshClicked?.Invoke(sender, e); };
            btnRefresh.MouseLeftButtonDown += (sender, e) => { RefreshClicked?.Invoke(sender, e); };
            lblDefault.MouseLeftButtonDown += (sender, e) => { DefaultClicked?.Invoke(sender, e); };
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                lblName.Content = value;
            }
        }

        public ReadOnlyCollection<ITelemetry> TelemetryItems => _telemetryItems.AsReadOnly();

        private void TelemetryControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblName.Content = Text;
            UpdateStatus();
        }

        public event EventHandler<EventArgs> RefreshClicked;
        public event EventHandler<EventArgs> DefaultClicked;

        public event EventHandler<TelemetryEventArgs> TelemetryChanged;
        public event EventHandler<TelemetryEventArgs> TelemetryChanging;

        public void AddTelemetryItem(ITelemetry telemetry, string displayText)
        {
            lblPlaceholder.Visibility = Visibility.Collapsed;
            btnDefault.Visibility = Visibility.Visible;
            lblDefault.Visibility = Visibility.Visible;

            _telemetryItems.Add(telemetry);

            var cb = new CheckBox
            {
                Content = new TextBlock
                {
                    Text = displayText
                },
                IsChecked = telemetry.IsActive(),
                Style = (Style) FindResource("SwitchCheckBox")
            };

            containerPanel.Children.Add(cb);

            cb.Checked += (s, e) => { ProcessCheckChange(telemetry, cb, true); };
            cb.Unchecked += (s, e) => { ProcessCheckChange(telemetry, cb, false); };

            UpdateStatus();

            MarginSetter.CreateThicknesForChildren(containerPanel, new DependencyPropertyChangedEventArgs());
        }

        private void ProcessCheckChange(ITelemetry telemetry, ToggleButton cb, bool isChecked)
        {
            if (_suppressEvents)
                return;

            var eventArgs = new TelemetryEventArgs(telemetry, isChecked);

            var cancelled = false;

            if (TelemetryChanging != null)
            {
                TelemetryChanging(this, eventArgs);
                cancelled = eventArgs.Cancel;
            }

            if (cancelled)
            {
                _suppressEvents = true;
                cb.IsChecked = !isChecked;
                _suppressEvents = false;
            }

            else
            {
                TelemetryChanged?.Invoke(this, eventArgs);
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var disabledCount = _telemetryItems.Count(x => !x.IsActive());

            if (lblName.Visibility == Visibility.Hidden)
                lblName.Visibility = Visibility.Visible;

            var allDisabled = disabledCount == _telemetryItems.Count;

            if (_telemetryItems.Count > 0)
                lblName.Content = $"{Text} - ({(allDisabled ? Properties.Resources.All_Disabled : $"{disabledCount} / {_telemetryItems.Count} {Properties.Resources.Disabled}")})";
        }

        public void Reset()
        {
            _suppressEvents = true;

            _telemetryItems.Clear();
            containerPanel.Children.Clear();
            lblPlaceholder.Visibility = Visibility.Visible;
            btnDefault.Visibility = Visibility.Collapsed;
            lblDefault.Visibility = Visibility.Collapsed;

            _suppressEvents = false;
        }

        public class TelemetryEventArgs : EventArgs
        {
            public TelemetryEventArgs(ITelemetry telemetry, bool enabled)
            {
                Telemetry = telemetry;
                Enabled = enabled;
            }

            public bool Cancel { get; set; }

            public bool Enabled { get; }

            public ITelemetry Telemetry { get; }
        }
    }
}