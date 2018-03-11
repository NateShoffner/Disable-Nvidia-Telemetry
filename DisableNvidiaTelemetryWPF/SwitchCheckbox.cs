using System.Windows;
using DisableNvidiaTelemetryWPF.Properties;

namespace DisableNvidiaTelemetryWPF
{
    internal class SwitchCheckbox
    {
        public static readonly DependencyProperty EnabledTextProperty =
            DependencyProperty.RegisterAttached("EnabledText", typeof(string), typeof(SwitchCheckbox), new PropertyMetadata(Resources.Enabled));

        public static readonly DependencyProperty DisabledTextProperty =
            DependencyProperty.RegisterAttached("DisabledText", typeof(string), typeof(SwitchCheckbox), new PropertyMetadata(Resources.Disabled));

        public static void SetEnabledText(UIElement element, string value)
        {
            element.SetValue(EnabledTextProperty, value);
        }

        public static string GetEnabledText(UIElement element)
        {
            return (string) element.GetValue(EnabledTextProperty);
        }

        public static void SetDisabledText(UIElement element, string value)
        {
            element.SetValue(DisabledTextProperty, value);
        }

        public static string GetDisabledText(UIElement element)
        {
            return (string) element.GetValue(DisabledTextProperty);
        }
    }
}