using GameOfLife.Models;
using System;

namespace GameOfLife.Services
{
    /// <summary>
    /// Интерфейс для движка игры "Жизнь"
    /// </summary>
    public interface IGameEngine
    {
        void Start(bool reverse = false);
        void Pause();
        void Resume();
        void Stop();
        void Reset();
        void Update();
        void SetRules(string rules);
        string GetCurrentRules();
        void SetNeighborhoodType(string type);
        string GetCurrentNeighborhood();
        ICell[,] GetGrid();
        int GetCurrentGeneration();
        bool IsRunning();
        bool IsReverseTime();
        void Initialize(int width, int height);
        void Initialize(int width, int height, bool[,] pattern);
        void PlacePattern(int startX, int startY, bool[,] pattern);
        void Randomize(double density = 0.3);
        void GoToPreviousGeneration();
        void GoToNextGeneration();

        /// <summary>
        /// Устанавливает состояние клетки по указанным координатам.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="isAlive">Новое состояние клетки (true - жива, false - мертва).</param>
        void SetCellState(int x, int y, bool isAlive);

        /// <summary>
        /// Событие, возникающее при достижении стабильного состояния (поле больше не меняется).
        /// </summary>
        event EventHandler StableStateReached;
    }
} 