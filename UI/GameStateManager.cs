using GameOfLife.Models;
using GameOfLife.Services;
using System.Text.Json;

namespace GameOfLife.UI
{
    public class GameState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Rules { get; set; }
        public string NeighborhoodType { get; set; }
        public bool[,] Grid { get; set; }
    }

    public class GameStateManager
    {
        private readonly string _saveDirectory;

        public GameStateManager()
        {
            _saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameOfLife",
                "Saves"
            );
            Directory.CreateDirectory(_saveDirectory);
        }

        public void SaveState(GameEngine gameEngine, string name)
        {
            var state = new GameState
            {
                Width = gameEngine.Width,
                Height = gameEngine.Height,
                Rules = gameEngine.GetCurrentRules(),
                NeighborhoodType = gameEngine.GetCurrentNeighborhood(),
                Grid = new bool[gameEngine.Width, gameEngine.Height]
            };

            var grid = gameEngine.GetGrid();
            for (int x = 0; x < gameEngine.Width; x++)
            {
                for (int y = 0; y < gameEngine.Height; y++)
                {
                    state.Grid[x, y] = grid[x, y].IsAlive;
                }
            }

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(Path.Combine(_saveDirectory, $"{name}.json"), json);
        }

        public GameState LoadState(string filename)
        {
            var json = File.ReadAllText(Path.Combine(_saveDirectory, filename));
            return JsonSerializer.Deserialize<GameState>(json);
        }

        public string[] GetSavedStates()
        {
            return Directory.GetFiles(_saveDirectory, "*.json")
                          .Select(Path.GetFileNameWithoutExtension)
                          .ToArray();
        }
    }
} 