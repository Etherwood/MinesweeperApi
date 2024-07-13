using Newtonsoft.Json;

namespace MinesweeperApi.Models
{
    public class ErrorResponse
    {
        public ErrorResponse(string err) 
        { 
            _error = err; 
        }

        [JsonProperty("error")]
        private string _error { get; set; }
    }
}
