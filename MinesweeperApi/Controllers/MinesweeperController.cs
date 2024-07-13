

using Microsoft.AspNetCore.Mvc;
using MinesweeperApi.MinesweeperServices;
using MinesweeperApi.Models;
using Newtonsoft.Json;
using System.Linq;

namespace MinesweeperApi.Controllers
{
    [ApiController]
    public class MinesweeperController : ControllerBase
    {
        MinesweeperService _minesweeperService;

        public MinesweeperController(MinesweeperService minesweeperService)
        {
            _minesweeperService = minesweeperService;
        }

        [HttpPost("new")]
        public ActionResult<GameInfoResponse> CreateGame(NewGameRequest newGameRequest)
        {
            try
            {
                return Ok(JsonConvert.SerializeObject(_minesweeperService.NewGame(newGameRequest)));
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new ErrorResponse(ex.Message)));
            }
        }

        [HttpPost("turn")]
        public async Task<ActionResult<GameInfoResponse>> Turn(GameTurnRequest gameTurnRequest)
        {
            try
            {
                return Ok(JsonConvert.SerializeObject(_minesweeperService.Turn(gameTurnRequest)));
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new ErrorResponse(ex.Message)));
            }
        }
    }
}
