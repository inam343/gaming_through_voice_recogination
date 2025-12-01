using GamingThroughVoiceRecognitionSystem.Database;
using GamingThroughVoiceRecognitionSystem.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class DashboardControl : UserControl
    {
        private readonly UserModel currentUser;
        private readonly DbConn db;

        public DashboardControl(UserModel user, DbConn database)
        {
            InitializeComponent();
            currentUser = user;
            db = database;

            LoadData();
        }

        private void LoadData()
        {
            WelcomeText.Text = $"Welcome back, {currentUser.FullName.Split(' ')[0]}! 👋";
            LoadStats();
            LoadGames();
        }

        private void LoadStats()
        {
            try
            {
                var gamesPlayedData = db.GetData($"SELECT COUNT(*) FROM user_game_history WHERE UserID={currentUser.UserId}", null);
                GamesPlayedCard.Text = gamesPlayedData.Rows.Count > 0 ? gamesPlayedData.Rows[0][0].ToString() : "0";
            }
            catch
            {
                GamesPlayedCard.Text = "0";
            }

            try
            {
                var voiceCommandsData = db.GetData($"SELECT COUNT(*) FROM user_voice_history WHERE UserID={currentUser.UserId}", null);
                VoiceCommandsCard.Text = voiceCommandsData.Rows.Count > 0 ? voiceCommandsData.Rows[0][0].ToString() : "0";
            }
            catch
            {
                VoiceCommandsCard.Text = "0";
            }

            try
            {
                var hoursPlayedData = db.GetData($"SELECT ISNULL(SUM(Duration), 0) FROM user_game_history WHERE UserID={currentUser.UserId}", null);
                HoursPlayedCard.Text = hoursPlayedData.Rows.Count > 0 ? hoursPlayedData.Rows[0][0].ToString() : "0";
            }
            catch
            {
                HoursPlayedCard.Text = "0";
            }
        }

        private void LoadGames()
        {
            List<GameModel> games = db.GetAvailableGames();
            GamesGrid.ItemsSource = games;
        }
    }
}
