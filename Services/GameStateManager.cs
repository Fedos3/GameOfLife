using GameOfLife.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace GameOfLife.Services
{
    // Структура для хранения состояний игры при сохранении в файл
    internal struct SavedGameState
    {
        public int Generation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool[,] Grid { get; set; }
    }

    public class GameStateManager
    {
        private const string DEFAULT_EXTENSION = ".gol";
        private readonly string _saveDirectory;

        public GameStateManager(string saveDirectory = null)
        {
            _saveDirectory = saveDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GameOfLife"
            );

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public void SaveState(string fileName, ICell[,] grid, int generation)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));

            string filePath = GetFilePath(fileName);

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            var state = new SavedGameState
            {
                Generation = generation,
                Width = width,
                Height = height,
                Grid = new bool[width, height]
            };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    state.Grid[x, y] = grid[x, y].IsAlive;
                }
            }

            string jsonString = JsonSerializer.Serialize(state);
            File.WriteAllText(filePath, jsonString);
        }

        public (int Generation, bool[,] Grid) LoadState(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));

            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл {filePath} не найден");

            string jsonString = File.ReadAllText(filePath);
            var state = JsonSerializer.Deserialize<SavedGameState>(jsonString);

            return (state.Generation, state.Grid);
        }

        public string[] GetSavedGames()
        {
            if (!Directory.Exists(_saveDirectory))
                return Array.Empty<string>();

            return Directory.GetFiles(_saveDirectory, $"*{DEFAULT_EXTENSION}")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        public bool DeleteSavedGame(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }

        private string GetFilePath(string fileName)
        {
            if (!fileName.EndsWith(DEFAULT_EXTENSION))
                fileName += DEFAULT_EXTENSION;

            return Path.Combine(_saveDirectory, fileName);
        }
    }
} 