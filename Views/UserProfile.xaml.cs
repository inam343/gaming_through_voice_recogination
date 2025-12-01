using GamingThroughVoiceRecognitionSystem.Models;
using GamingThroughVoiceRecognitionSystem.Database;
using System.Windows;
using System.Windows.Controls;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class UserProfile : UserControl
    {
        private readonly DbConn db;
        private readonly int _userId;
        private readonly HomeWindow _parent;

        public UserProfile(int userId, HomeWindow parentWindow)
        {
            InitializeComponent();

            db = new DbConn();
            _userId = userId;
            _parent = parentWindow;

            LoadUserProfile();
        }

        public void LoadUserProfile()
        {
            UserModel user = db.GetUserById(_userId);

            if (user != null)
            {
                ProfileUserName.Text = user.FullName;
                ProfileEmail.Text = user.Email;

                // Avatar initials
                string initials = "";
                string[] nameParts = user.FullName.Split(' ');
                if (nameParts.Length > 0 && !string.IsNullOrEmpty(nameParts[0])) 
                    initials += nameParts[0][0];
                if (nameParts.Length > 1 && !string.IsNullOrEmpty(nameParts[nameParts.Length - 1])) 
                    initials += nameParts[nameParts.Length - 1][0];
                AvatarInitials.Text = initials.ToUpper();
            }

            // Dummy statistics for now 
            GamesPlayedText.Text = "24";
            VoiceCommandsText.Text = "156";
            TotalPlaytimeText.Text = "48h";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to dashboard
            if (_parent != null)
            {
                _parent.ContentArea.Content = new DashboardControl(db.GetUserById(_userId), db);
            }
        }
    }
}
