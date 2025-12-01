using GamingThroughVoiceRecognitionSystem.Database;
using GamingThroughVoiceRecognitionSystem.Models;
using GamingThroughVoiceRecognitionSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class SettingsControl : UserControl
    {
        private readonly UserModel currentUser;
        private readonly DbConn db;

        public SettingsControl(UserModel user, DbConn database)
        {
            InitializeComponent();
            currentUser = user;
            db = database;

            // Set initial toggle state
            UpdateToggleUI(ThemeManager.CurrentTheme == AppTheme.Dark);
        }

        private void ThemeToggle_Click(object sender, MouseButtonEventArgs e)
        {
            // Toggle theme
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Light;
            ThemeManager.CurrentTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            ThemeManager.SaveThemePreference();
            
            // Animate toggle
            UpdateToggleUI(isDark);
        }

        private void UpdateToggleUI(bool isDark)
        {
            // Animate the toggle circle
            var animation = new ThicknessAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            if (isDark)
            {
                // Move to right (Dark mode)
                animation.To = new Thickness(0, 0, 5, 0);
                ToggleCircle.HorizontalAlignment = HorizontalAlignment.Right;
                LightIcon.Opacity = 0.3;
                DarkIcon.Opacity = 1.0;
            }
            else
            {
                // Move to left (Light mode)
                animation.To = new Thickness(5, 0, 0, 0);
                ToggleCircle.HorizontalAlignment = HorizontalAlignment.Left;
                LightIcon.Opacity = 1.0;
                DarkIcon.Opacity = 0.3;
            }

            ToggleCircle.BeginAnimation(MarginProperty, animation);
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.SaveThemePreference();
            GlassMessageBox.Show("Settings saved successfully!");
        }
    }
}
