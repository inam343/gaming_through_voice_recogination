using GamingThroughVoiceRecognitionSystem.Database;
using GamingThroughVoiceRecognitionSystem.Models;
using System.Windows.Controls;

namespace GamingThroughVoiceRecognitionSystem.Views
{
    public partial class VoiceCommandsControl : UserControl
    {
        private readonly UserModel currentUser;
        private readonly DbConn db;

        public VoiceCommandsControl(UserModel user, DbConn database)
        {
            InitializeComponent();
            currentUser = user;
            db = database;
        }
    }
}
