using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class NewGameRequest
    {
        public int width { get; set; }

        public int height { get; set; }

        public int mines_count { get; set; }
    }
}
