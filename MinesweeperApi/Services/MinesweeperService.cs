using MinesweeperApi.Models;
using Newtonsoft.Json;

namespace MinesweeperApi.MinesweeperServices
{
    public class MinesweeperService
    {
        private static Dictionary<string, GameData> _games = new Dictionary<string, GameData>();
        public GameInfoResponse NewGame(NewGameRequest newGameRequest)
        {
            if (newGameRequest.width < 2 || newGameRequest.width > 30)
                throw new Exception("Ширина поля должна быть не менее 2 и не более 30");
            if (newGameRequest.height < 2 || newGameRequest.height > 30)
                throw new Exception("Высота поля должна быть не менее 2 и не более 30");
            if (newGameRequest.mines_count < 1 || newGameRequest.mines_count > newGameRequest.width * newGameRequest.height - 1)
                throw new Exception($"Количество мин должно быть не менее 1 и не более {newGameRequest.width * newGameRequest.height - 1}");

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
                },
                MemoryArray = new bool[newGameRequest.height, newGameRequest.width],
                MinesCount = newGameRequest.mines_count,
                HiddenField = new char[newGameRequest.height, newGameRequest.width]
            };

            _games.Add(gameId, game);

            return game.Response;
        }

        public GameInfoResponse Turn(GameTurnRequest gameTurnRequest)
        {
            var game = _games[gameTurnRequest.game_id];
            var col = gameTurnRequest.col;
            var row = gameTurnRequest.row;

            if (game.Response.IsCompleted)
            {
                throw new Exception("Игра завершена");
            }

            if (game.HiddenField[row, col] == 'X')
            {
                game.Response.IsCompleted = true;
                EndGame(game);
                game.Response.Field = game.HiddenField;
                return game.Response;
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

                return game.Response;
            }
            else
                throw new Exception("GameId Error");
        }

        private void Turn(int row, int col, GameData game)
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

        private int CalculateNearbyBombs(int row, int col, GameData game)
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

        private bool CheckBomb(int row, int col, GameData game)
        {
            return row >= 0 && col >= 0 && row <= game.Response.Height - 1 && col <= game.Response.Width - 1 && game.HiddenField[row, col] == 'X';
        }

        private void GenerateBombs(int height, int width, GameData game, int row, int col)
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

        private void EndGame(GameData game)
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

        private void CheckWinGame(GameData game)
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
