using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class GameTurnRequest
    {
        public string game_id { get; set; }

        public int col { get; set; }

        public int row { get; set; }
    }
}
