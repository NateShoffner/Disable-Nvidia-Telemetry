using System.Windows;

namespace DisableNvidiaTelemetry.View
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => lblMessage.Content.ToString();
            set => lblMessage.Content = value;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}