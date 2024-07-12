using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class GameInfoResponse
    {
        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("mines_count")]
        public int MinesCount { get; set; }

        [JsonProperty("completed")]
        public bool IsCompleted { get; set; }

        [JsonProperty("field")]
        public char[,] Field { get; set; }
    }
}
