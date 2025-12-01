using GamingThroughVoiceRecognitionSystem.Database;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DbConn db = new DbConn();

        // Voice command monitor
        private DispatcherTimer voiceTimer;

        // Shared voice command file
        private readonly string voiceCommandFile = @"voice_command.txt";

        public LoginWindow()
        {
            InitializeComponent();
            this.Closed += Window_Closed;
        }

        // ---------------------------------------------------------
        // WINDOW LOADED
        // ---------------------------------------------------------
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Resources["BackgroundAnimationStoryboard"] is Storyboard sb)
                sb.Begin();

            StartVoiceMonitor();
        }

        private void WindowChrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ---------------------------------------------------------
        // PLACEHOLDER HANDLING
        // ---------------------------------------------------------
        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "Email")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = Brushes.White;
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "Email";
                EmailTextBox.Foreground = Brushes.Gray;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        // ---------------------------------------------------------
        // BUTTON EVENTS
        // ---------------------------------------------------------
        private void FaceLoginButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var faceCaptureWindow = new FaceCaptureWindow();
                bool? result = faceCaptureWindow.ShowDialog();

                if (result == true && faceCaptureWindow.IsCaptured &&
                    faceCaptureWindow.CapturedFaceData != null)
                {
                    if (db.AuthenticateWithFace(faceCaptureWindow.CapturedFaceData, out int userId))
                    {
                        GlassMessageBox.Show("Face authentication successful!");
                        HomeWindow homeWindow = new HomeWindow(userId);
                        homeWindow.Show();
                        this.Close();
                    }
                    else GlassMessageBox.Show("Face not recognized. Try again or register.");
                }
            }
            catch (Exception ex)
            {
                GlassMessageBox.Show($"Face login error: {ex.Message}");
            }
        }

        private void VoiceLoginButton_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var voiceRecordingWindow = new VoiceRecordingWindow();
                bool? result = voiceRecordingWindow.ShowDialog();

                if (result == true && voiceRecordingWindow.IsRecorded &&
                    voiceRecordingWindow.RecordedVoiceData != null)
                {
                    if (db.AuthenticateWithVoice(voiceRecordingWindow.RecordedVoiceData, out int userId))
                    {
                        GlassMessageBox.Show("Voice authentication successful!");
                        HomeWindow homeWindow = new HomeWindow(userId);
                        homeWindow.Show();
                        this.Close();
                    }
                    else GlassMessageBox.Show("Voice not recognized. Try again or register.");
                }
            }
            catch (Exception ex)
            {
                GlassMessageBox.Show($"Voice login error: {ex.Message}");
            }
        }

        private void ManualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = (EmailTextBox.Text == "Email") ? "" : EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (email == "" || password == "")
            {
                GlassMessageBox.Show("Please enter both email and password.");
                return;
            }

            if (db.Login(email, password, out int userId))
            {
                HomeWindow homeWindow = new HomeWindow(userId);
                homeWindow.Show();
                this.Close();
            }
            else GlassMessageBox.Show("Invalid email or password.");
        }

        private void SignUpTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            SignUpWindow signup = new SignUpWindow();
            signup.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        // ---------------------------------------------------------
        // VOICE MONITOR
        // ---------------------------------------------------------
        private void StartVoiceMonitor()
        {
            voiceTimer = new DispatcherTimer();
            voiceTimer.Interval = TimeSpan.FromMilliseconds(250);
            voiceTimer.Tick += ReadVoiceCommand;
            voiceTimer.Start();
        }

        private void ReadVoiceCommand(object sender, EventArgs e)
        {
            if (!File.Exists(voiceCommandFile))
                return;

            string text;
            try
            {
                text = File.ReadAllText(voiceCommandFile).Trim().ToLower();
            }
            catch { return; }

            if (string.IsNullOrWhiteSpace(text)) return;

            File.WriteAllText(voiceCommandFile, "");

            // ---------------------------------------------------------
            // MAIN VOICE COMMANDS
            // ---------------------------------------------------------

            // Back Navigation
            if (text.Contains("back") || text.Contains("previous"))
                BackButton_Click(null, null);

            // Signup Window
            else if (text.Contains("signup") || text.Contains("sign up"))
                SignUpTextBlock_Click(null, null);

            // Manual Login
            else if (text.Contains("sign in") || text.Contains("manual login"))
                ManualLoginButton_Click(null, null);

            // Face Login Button
            else if (text.Contains("face") || text.Contains("face login") || text.Contains("open face"))
                FaceLoginButton_Click(null, null);

            // Voice Login Button
            else if (text.Contains("voice") || text.Contains("voice login") || text.Contains("voice recording"))
                VoiceLoginButton_Click(null, null);

            // Close Window
            else if (text.Contains("close"))
                CloseButton_Click(null, null);

            // Minimize Window
            else if (text.Contains("minimise") || text.Contains("minimize"))
                MinimizeButton_Click(null, null);

            // Maximize Window
            else if (text.Contains("full screen") || text.Contains("maximize"))
                MaximizeButton_Click(null, null);
        }

        // ---------------------------------------------------------
        // STOP ONLY THE MONITOR
        // ---------------------------------------------------------
        private void Window_Closed(object sender, EventArgs e)
        {
            voiceTimer?.Stop();
        }
    }
}
