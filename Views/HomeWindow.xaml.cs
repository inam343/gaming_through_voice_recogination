using GamingThroughVoiceRecognitionSystem.Database;
using GamingThroughVoiceRecognitionSystem.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class HomeWindow : Window
    {
        private readonly DbConn db;
        private UserModel currentUser;

        // Public property to allow navigation from child controls
        public ContentControl ContentArea => MainContentArea;

        public HomeWindow(int userId)
        {
            InitializeComponent();
            
            db = new DbConn();
            currentUser = db.GetUserById(userId);

            if (currentUser != null)
            {
                LoadUserInfo();
                // Load Home page by default
                NavigateToHome();
            }
        }

        #region Load Data
        private void LoadUserInfo()
        {
            UserNameText.Text = currentUser.FullName;
        }
        #endregion

        #region Navigation Methods
        private void NavigateToHome()
        {
            MainContentArea.Content = new DashboardControl(currentUser, db);
        }

        private void NavigateToVoiceCommands()
        {
            MainContentArea.Content = new VoiceCommandsControl(currentUser, db);
        }

        private void NavigateToProfile()
        {
            MainContentArea.Content = new UserProfile(currentUser.UserId, this);
        }

        private void NavigateToSettings()
        {
            MainContentArea.Content = new SettingsControl(currentUser, db);
        }
        #endregion

        #region Navigation Buttons
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void VoiceCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToVoiceCommands();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProfile();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSettings();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new LoginWindow().Show();
        }
        #endregion

        #region Window Controls
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        #endregion
    }
}
