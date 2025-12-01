using System.Windows;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class GlassMessageBox : Window
    {
        public GlassMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Static helper to show messages easily
        public static void Show(string message)
        {
            GlassMessageBox box = new GlassMessageBox(message);
            box.ShowDialog();
        }
    }
}
