using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingThroughVoiceRecognitionSystem.Models
{
    public class GameModel
    {
        public int GameId { get; set; }        // ID of the game in the database
        public string GameName { get; set; }       // Name of the game
        public string FilePath { get; set; }
    }
}
