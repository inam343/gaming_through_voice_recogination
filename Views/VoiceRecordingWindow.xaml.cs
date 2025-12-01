using GamingThroughVoiceRecognitionSystem.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class VoiceRecordingWindow : Window
    {
        private VoiceRecognitionService voiceService;
        public byte[] RecordedVoiceData { get; private set; }
        public bool IsRecorded { get; private set; }
        private bool isRecording = false;

        // Voice command monitoring
        private DispatcherTimer voiceTimer;
        private readonly string voiceCommandFile = @"voice_command.txt";

        // Python listener
        private readonly string pythonExe = @"C:\Users\user\Desktop\New folder (3)\New folder\GamingThroughVoiceRecognition-Updated-main\dist\voice_listener.exe";
        private static Process pythonProcess = null;

        public VoiceRecordingWindow()
        {
            InitializeComponent();
            this.Closed += Window_Closed;
            voiceService = new VoiceRecognitionService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            voiceService.AudioLevelChanged += VoiceService_AudioLevelChanged;
            voiceService.RecordingStarted += VoiceService_RecordingStarted;
            voiceService.RecordingStopped += VoiceService_RecordingStopped;

            StartPythonListener();
            StartVoiceMonitor();
        }

        // -------------------------------
        // Python listener
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
            voiceTimer.Interval = TimeSpan.FromMilliseconds(250);
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

            if (text.Contains("start recording") && !isRecording)
            {
                StartRecording();
            }
            else if (text.Contains("stop recording") && isRecording)
            {
                StopRecording();
            }
            else if (text.Contains("end"))
            {
                CloseWindow();
            }
        }

        // -------------------------------
        // Recording logic
        // -------------------------------
        private void StartRecording()
        {
            voiceService.StartRecording();
            isRecording = true;
        }

        private void StopRecording()
        {
            RecordedVoiceData = voiceService.StopRecording();
            isRecording = false;

            if (RecordedVoiceData != null && RecordedVoiceData.Length > 0)
            {
                ShowSuccessAndClose();
            }
            else
            {
                GlassMessageBox.Show("No audio recorded. Please try again.");
            }
        }

        private async void ShowSuccessAndClose()
        {
            IsRecorded = true;
            SuccessOverlay.Visibility = Visibility.Visible;
            InstructionText.Text = "Voice recorded successfully!";

            await Task.Delay(1500);

            // Only set DialogResult if opened as a dialog
            if (this.IsVisible && this.WindowState != WindowState.Minimized && this.Owner != null)
            {
                try
                {
                    this.DialogResult = true;
                }
                catch
                {
                    // Ignore if not shown as dialog
                }
            }

            this.Close();
        }

        private void CloseWindow()
        {
            if (isRecording) StopRecording();
            this.DialogResult = false;
            this.Close();
        }

        // -------------------------------
        // Visualizer & UI
        // -------------------------------
        private void VoiceService_AudioLevelChanged(object sender, float level)
        {
            Dispatcher.Invoke(() =>
            {
                double scale = 1.0 + (level * 2);
                var transform = AudioWave.RenderTransform as ScaleTransform;
                if (transform != null)
                {
                    transform.ScaleX = scale;
                    transform.ScaleY = scale;
                }
            });
        }

        private void VoiceService_RecordingStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RecordButton.Content = "⏹ STOP RECORDING";
                InstructionText.Text = "Recording... Speak clearly!";
                StartPulseAnimation();
            });
        }

        private void VoiceService_RecordingStopped(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RecordButton.Content = "🎤 START RECORDING";
                InstructionText.Text = "Click to start recording";
                StopPulseAnimation();
            });
        }

        private void StartPulseAnimation()
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.2,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            RecordingIndicator.RenderTransform = new ScaleTransform(1, 1);
            RecordingIndicator.RenderTransformOrigin = new Point(0.5, 0.5);
            RecordingIndicator.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
            RecordingIndicator.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
        }

        private void StopPulseAnimation()
        {
            RecordingIndicator.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            RecordingIndicator.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        // -------------------------------
        // Button clicks still work
        // -------------------------------
        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording) StartRecording();
            else StopRecording();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            voiceService?.Dispose();
            voiceTimer?.Stop();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            voiceTimer?.Stop();
        }
    }
}
