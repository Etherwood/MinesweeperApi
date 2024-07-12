namespace MinesweeperApi.Models
{
    public class GameData
    {
        public GameInfoResponse Response {  get; set; }
        public char[,] HiddenField { get; set; }
        public bool[,] MemoryArray { get; set; }
        public int MinesCount { get; set; }
        public bool IsNewGame { get; set; } = true;
    }
}
