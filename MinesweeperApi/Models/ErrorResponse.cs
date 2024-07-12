using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class ErrorResponse
    {
        public ErrorResponse(string err) 
        { 
            error = err; 
        }

        [JsonProperty("error")]
        private string error { get; set; }
    }
}
