using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class GameTurnRequest
    {
        [JsonProperty("game_id")]
        public string game_id { get; set; }

        [JsonProperty("col")]
        public int Col { get; set; }

        [JsonProperty("row")]
        public int Row { get; set; }
    }
}
