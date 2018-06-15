using System.Windows;
using System.Windows.Controls;

namespace DisableNvidiaTelemetry
{
    public class MarginSetter
    {
        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), CreateThicknesForChildren));

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness) obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        public static void CreateThicknesForChildren(object sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as Panel;

            if (panel == null) return;

            foreach (var child in panel.Children)
            {
                var fe = child as FrameworkElement;

                if (fe == null) continue;

                // preserve any possible margins defined locally
                var currentMargin = fe.Margin; // new Thickness(fe.Margin.Left, fe.Margin.Top, fe.Margin.Right, fe.Margin.Bottom);
                var definedMargin = GetMargin(panel);

                var newMargin = new Thickness(currentMargin.Left + definedMargin.Left,
                    currentMargin.Top + definedMargin.Top,
                    currentMargin.Right + definedMargin.Right,
                    currentMargin.Bottom + definedMargin.Bottom);

                fe.Margin = GetMargin(panel);
            }
        }
    }
}