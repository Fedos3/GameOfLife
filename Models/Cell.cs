using System;

namespace GameOfLife.Models
{
    public interface ICell
    {
        bool IsAlive { get; set; }
    }

    /// <summary>
    /// Представляет клетку в игре "Жизнь"
    /// </summary>
    public struct Cell : ICell
    {
        /// <summary>
        /// Состояние клетки (жива/мертва)
        /// </summary>
        public bool IsAlive { get; set; }
        
        /// <summary>
        /// Координата X клетки
        /// </summary>
        public int X { get; }
        
        /// <summary>
        /// Координата Y клетки
        /// </summary>
        public int Y { get; }
        
        /// <summary>
        /// Создает новую клетку с указанным состоянием и координатами
        /// </summary>
        public Cell(bool isAlive, int x, int y)
        {
            IsAlive = isAlive;
            X = x;
            Y = y;
        }
        
        /// <summary>
        /// Создает копию клетки
        /// </summary>
        public Cell Clone() => new Cell(IsAlive, X, Y);
    }
} 
