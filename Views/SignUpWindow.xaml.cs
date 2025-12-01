using GamingThroughVoiceRecognitionSystem.Database;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class SignUpWindow : Window
    {
        private readonly DbConn db = new DbConn();
        private byte[] capturedFaceData = null;
        private byte[] recordedVoiceData = null;

        // Voice command timer
        private DispatcherTimer voiceTimer;
        private readonly string voiceCommandFile = @"voice_command.txt";

        // Path to python listener exe
        private readonly string pythonExe = @"C:\Users\user\Desktop\New folder (3)\New folder\GamingThroughVoiceRecognition-Updated-main\dist\voice_listener.exe";
        private static Process pythonProcess = null;

        public SignUpWindow()
        {
            InitializeComponent();
            this.Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Resources["BackgroundAnimationStoryboard"] is Storyboard sb)
                sb.Begin();

            StartPythonListener();
            StartVoiceMonitor();
        }

        // -------------------------------
        // Start Python listener
        // -------------------------------
        private void StartPythonListener()
        {
            try
            {
                if (pythonProcess != null && !pythonProcess.HasExited) return;

                if (!File.Exists(pythonExe))
                {
                    MessageBox.Show("Python listener not found: " + pythonExe);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                pythonProcess = new Process { StartInfo = psi };
                pythonProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[PY] " + e.Data); };
                pythonProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[PY ERROR] " + e.Data); };
                pythonProcess.Start();
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python listener failed: " + ex.Message);
            }
        }

        // -------------------------------
        // Voice monitor
        // -------------------------------
        private void StartVoiceMonitor()
        {
            voiceTimer = new DispatcherTimer();
            voiceTimer.Interval = TimeSpan.FromMilliseconds(300);
            voiceTimer.Tick += ReadVoiceCommand;
            voiceTimer.Start();
        }

        private void ReadVoiceCommand(object sender, EventArgs e)
        {
            if (!File.Exists(voiceCommandFile)) return;

            string text = "";
            try { text = File.ReadAllText(voiceCommandFile).Trim().ToLower(); }
            catch { return; }

            if (string.IsNullOrWhiteSpace(text)) return;

            File.WriteAllText(voiceCommandFile, ""); // clear file

            if (text.Contains("signup")) SignupButton_Click(null, null);
            else if (text.Contains("login")) LoginTextBlock_Click(null, null);
            else if (text.Contains("back") || text.Contains("previous")) BackButton_Click(null, null);
            else if (text.Contains("start face")) FaceRegisterButton_Click(null, null);
            else if (text.Contains("record voice") || text.Contains("record")) VoiceRegisterButton_Click(null, null);
            else if (text.Contains("close")) CloseButton_Click(null, null);
            else if (text.Contains("minimize")) MinimizeButton_Click(null, null);
            else if (text.Contains("maximize") || text.Contains("full screen")) MaximizeButton_Click(null, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            voiceTimer?.Stop();
        }

        // -------------------------------
        // Normal UI functions
        // -------------------------------
        private void WindowChrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        // Placeholder logic
        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e) { if (NameTextBox.Text == "Full Name") { NameTextBox.Text = ""; NameTextBox.Foreground = Brushes.White; } }
        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(NameTextBox.Text)) { NameTextBox.Text = "Full Name"; NameTextBox.Foreground = Brushes.Gray; } }
        private void AgeTextBox_GotFocus(object sender, RoutedEventArgs e) { if (AgeTextBox.Text == "Age") { AgeTextBox.Text = ""; AgeTextBox.Foreground = Brushes.White; } }
        private void AgeTextBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(AgeTextBox.Text)) { AgeTextBox.Text = "Age"; AgeTextBox.Foreground = Brushes.Gray; } }
        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e) { if (EmailTextBox.Text == "Email") { EmailTextBox.Text = ""; EmailTextBox.Foreground = Brushes.White; } }
        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(EmailTextBox.Text)) { EmailTextBox.Text = "Email"; EmailTextBox.Foreground = Brushes.Gray; } }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) { PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Hidden; }

        // -------------------------------
        // Buttons
        // -------------------------------
        private void FaceRegisterButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var faceWindow = new FaceCaptureWindow();
                if (faceWindow.ShowDialog() == true && faceWindow.IsCaptured) capturedFaceData = faceWindow.CapturedFaceData;
            }
            catch { }
        }

        private void VoiceRegisterButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var voiceWindow = new VoiceRecordingWindow();
                if (voiceWindow.ShowDialog() == true && voiceWindow.IsRecorded) recordedVoiceData = voiceWindow.RecordedVoiceData;
            }
            catch { }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            // signup logic here...
        }

        private void LoginTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
