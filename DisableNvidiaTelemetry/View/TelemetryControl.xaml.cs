using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            containerPanel.Loaded += ContainerPanel_Loaded;
            Loaded += TelemetryControl_Loaded;

            lblRefresh.MouseLeftButtonDown += (sender, e) =>
            {
                if (RefreshClicked != null)
                    RefreshClicked(sender, e);
            };

            btnRefresh.MouseLeftButtonDown += (sender, e) =>
            {
                if (RefreshClicked != null)
                    RefreshClicked(sender, e);
            };

            lblDefault.MouseLeftButtonDown += (sender, e) =>
            {
                if (DefaultClicked != null)
                    DefaultClicked(sender, e);
            };
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

        public event EventHandler<TelemetryModifiedEventArgs> TelemetryModified;
        public event EventHandler<EventArgs> RefreshClicked;
        public event EventHandler<EventArgs> DefaultClicked;

        private void ContainerPanel_Loaded(object sender, RoutedEventArgs e)
        {
            MarginSetter.CreateThicknesForChildren(sender, new DependencyPropertyChangedEventArgs());
        }

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
                FontSize = 13,
                Style = (Style) FindResource("SwitchCheckBox")
            };

            containerPanel.Children.Add(cb);

            cb.Margin = new Thickness(5, 0, cb.ActualHeight * (_telemetryItems.Count - 1), 0);
            cb.Checked += (s, e) =>
            {
                if (_suppressEvents)
                    return;

                var eventArgs = new TelemetryModifiedEventArgs(telemetry, true);

                if (TelemetryModified != null)
                {
                    TelemetryModified(this, eventArgs);

                    if (eventArgs.Cancel)
                    {
                        _suppressEvents = true;
                        cb.IsChecked = false;
                        _suppressEvents = false;
                    }

                    UpdateStatus();
                }
            };
            cb.Unchecked += (s, e) =>
            {
                if (_suppressEvents)
                    return;

                var eventArgs = new TelemetryModifiedEventArgs(telemetry, false);

                if (TelemetryModified != null)
                {
                    TelemetryModified(this, eventArgs);

                    if (eventArgs.Cancel)
                    {
                        _suppressEvents = true;
                        cb.IsChecked = true;
                        _suppressEvents = false;
                    }

                    UpdateStatus();
                }
            };

            UpdateStatus();

            MarginSetter.CreateThicknesForChildren(containerPanel, new DependencyPropertyChangedEventArgs());
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

            _suppressEvents = false;
        }

        public class TelemetryModifiedEventArgs : EventArgs
        {
            public TelemetryModifiedEventArgs(ITelemetry telemetry, bool enabled)
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