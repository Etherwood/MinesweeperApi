

using Microsoft.AspNetCore.Mvc;
using MinesweeperApi.Models;
using Newtonsoft.Json;
using System.Linq;

namespace MinesweeperApi.Controllers
{
    [ApiController]
    public class MinesweeperController : ControllerBase
    {
        static Dictionary<string, GameData> games = new Dictionary<string, GameData>();

        [HttpPost("new")]
        public ActionResult<GameInfoResponse> CreateGame(NewGameRequest newGameRequest)
        {
            try
            {
                var field = new char[newGameRequest.height, newGameRequest.width];

                for (int i = 0; i < newGameRequest.height; i++)
                    for (int j = 0; j < newGameRequest.width; j++)
                        field[i, j] = ' ';
                var gameId = Guid.NewGuid().ToString("D");

                var game = new GameData()
                {
                    Response = new GameInfoResponse()
                    {
                        GameId = gameId,
                        Width = newGameRequest.width,
                        Height = newGameRequest.height,
                        MinesCount = newGameRequest.mines_count,
                        IsCompleted = false,
                        Field = field
                    }
                };

                games.Add(gameId, game);

                game.MemoryArray = new bool[newGameRequest.height, newGameRequest.width];
                game.MinesCount = newGameRequest.mines_count;
                game.HiddenField = new char[newGameRequest.height, newGameRequest.width];


                return Ok(JsonConvert.SerializeObject(games[gameId].Response));
            }
            catch
            {
                return null;
            }
        }

        [HttpPost("turn")]
        public async Task<ActionResult<GameInfoResponse>> Turn(GameTurnRequest gameTurnRequest)
        {
            try
            {
                var game = games[gameTurnRequest.game_id];
                var col = gameTurnRequest.Col;
                var row = gameTurnRequest.Row;

                if (game.Response.IsCompleted)
                {
                    throw new Exception("Игра завершена");
                }

                if (game.HiddenField[row, col] == 'X')
                {
                    game.Response.IsCompleted = true;
                    EndGame(game);
                    game.Response.Field = game.HiddenField;
                    return Ok(JsonConvert.SerializeObject(game.Response));
                }


                if (game.IsNewGame)
                {
                    GenerateBombs(game.Response.Height, game.Response.Width, game, row, col);
                    game.IsNewGame = false;
                }

                if (game.Response.Field[row, col] != ' ')
                {
                    throw new Exception("Уже открытая ячейка");
                }

                if (gameTurnRequest.game_id == game.Response.GameId)
                {
                    Turn(row, col, game);


                    Array.Copy(game.HiddenField, game.Response.Field, game.HiddenField.Length);
                    for (int i = 0; i < game.Response.Height; i++)
                        for (int j = 0; j < game.Response.Width; j++)
                        {
                            if (game.Response.Field[i, j] == 'X')
                                game.Response.Field[i, j] = ' ';
                        }

                    CheckWinGame(game);

                    return Ok(JsonConvert.SerializeObject(game.Response));
                }
                else
                    throw new Exception("GameId Error");
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new ErrorResponse(ex.Message)));
            }
        }

        void Turn(int row, int col, GameData game)
        {
            if (row < 0 || col < 0 || row > game.Response.Height - 1 || col > game.Response.Width - 1)
                return;
            if (game.MemoryArray[row, col])
                return;
            game.MemoryArray[row, col] = true;

            if (game.HiddenField[row, col] == 'X')
                return;

            var bombsCount = CalculateNearbyBombs(row, col, game);
            if (bombsCount == 0)
            {
                game.HiddenField[row, col] = Convert.ToChar(bombsCount.ToString());
                Turn(row, col + 1, game);
                Turn(row + 1, col, game);
                Turn(row - 1, col, game);
                Turn(row, col - 1, game);
                Turn(row - 1, col - 1, game);
                Turn(row + 1, col + 1, game);
                Turn(row + 1, col - 1, game);
                Turn(row - 1, col + 1, game);
                return;
            }
            else
            {
                game.HiddenField[row, col] = Convert.ToChar(bombsCount.ToString());
                return;
            }
        }

        int CalculateNearbyBombs(int row, int col, GameData game)
        {
            int bombQty = 0;

            if (CheckBomb(row, col + 1, game))
                bombQty++;
            if (CheckBomb(row + 1, col, game))
                bombQty++;
            if (CheckBomb(row - 1, col, game))
                bombQty++;
            if (CheckBomb(row, col - 1, game))
                bombQty++;
            if (CheckBomb(row - 1, col - 1, game))
                bombQty++;
            if (CheckBomb(row + 1, col + 1, game))
                bombQty++;
            if (CheckBomb(row + 1, col - 1, game))
                bombQty++;
            if (CheckBomb(row - 1, col + 1, game))
                bombQty++;

            return bombQty;
        }

        bool CheckBomb(int row, int col, GameData game)
        {
            return row >= 0 && col >= 0 && row <= game.Response.Height - 1 && col <= game.Response.Width - 1 && game.HiddenField[row, col] == 'X';
        }

        void GenerateBombs(int height, int width, GameData game, int row, int col)
        {
            int counter = 0;

            var randomArray = new int[width * height];
            for (int i = 0; i < width * height; i++)
            {
                randomArray[i] = i;
            }
            var rnd = new Random();
            rnd.Shuffle(randomArray);

            var randomBombsIndexes = new HashSet<int>();
            var minesCount = game.MinesCount;

            for (int i = 0; i < minesCount; i++)
            {
                randomBombsIndexes.Add(randomArray[i]);
            }

            if (randomBombsIndexes.Contains((int)(col + row * width)))
            {
                randomBombsIndexes.Remove((int)(col + row * width));
                randomBombsIndexes.Add(randomArray[minesCount++]);
            }

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    game.MemoryArray[i, j] = false;
                    if (randomBombsIndexes.Contains(counter))
                    {
                        game.HiddenField[i, j] = 'X';
                    }
                    if (game.HiddenField[i, j] != 'X')
                        game.HiddenField[i, j] = ' ';
                    counter++;
                }
        }

        void EndGame(GameData game)
        {
            for (int i = 0; i < game.Response.Height; i++)
                for (int j = 0; j < game.Response.Width; j++)
                {
                    if (i < 0 || j < 0 || i > game.Response.Height - 1 || j > game.Response.Width - 1)
                        continue;
                    if (game.MemoryArray[i, j])
                        continue;
                    game.MemoryArray[i, j] = true;
                    var bombsCount = CalculateNearbyBombs(i, j, game);
                    if (game.HiddenField[i, j] != 'X')
                        game.HiddenField[i, j] = Convert.ToChar(bombsCount.ToString());
                }
        }

        void CheckWinGame(GameData game)
        {
            var endGame = true;
            for (int i = 0; i < game.Response.Height; i++)
                for (int j = 0; j < game.Response.Width; j++)
                {
                    if (game.HiddenField[i, j] == ' ')
                        endGame = false;
                }
            if (endGame)
            {
                EndGame(game);
                for (int i = 0; i < game.Response.Height; i++)
                    for (int j = 0; j < game.Response.Width; j++)
                    {
                        if (game.HiddenField[i, j] == 'X')
                            game.HiddenField[i, j] = 'M';
                    }
                game.Response.Field = game.HiddenField;
                game.Response.IsCompleted = true;
                return;
            }
        }
    }
}
