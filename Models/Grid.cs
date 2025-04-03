using System;
using System.Collections.Generic;
using GameOfLife.Models;

namespace GameOfLife.Models
{
    /// <summary>
    /// Представляет игровую сетку с клетками
    /// </summary>
    public class Grid
    {
        private readonly Cell[,] _cells;
        
        /// <summary>
        /// Ширина сетки
        /// </summary>
        public virtual int Width { get; }
        
        /// <summary>
        /// Высота сетки
        /// </summary>
        public virtual int Height { get; }
        
        /// <summary>
        /// Индексатор для доступа к клеткам по координатам
        /// </summary>
        public virtual ICell this[int x, int y]
        {
            get => _cells[x, y];
        }
        
        /// <summary>
        /// Возвращает массив клеток
        /// </summary>
        public virtual Cell[,] Cells => _cells;
        
        /// <summary>
        /// Создает новую сетку с указанными размерами
        /// </summary>
        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new Cell[width, height];
            
            // Инициализируем все клетки с их координатами
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new Cell(false, x, y);
                }
            }
        }
        
        /// <summary>
        /// Создает копию сетки
        /// </summary>
        public virtual Grid Clone()
        {
            Grid clone = new Grid(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cell newCell = new Cell(_cells[x, y].IsAlive, x, y);
                    clone.SetCell(x, y, newCell);
                }
            }
            return clone;
        }
        
        /// <summary>
        /// Устанавливает состояние клетки по координатам
        /// </summary>
        public virtual void SetCell(int x, int y, Cell cell)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _cells[x, y] = cell;
            }
        }
        
        /// <summary>
        /// Устанавливает состояние клетки по координатам
        /// </summary>
        public virtual void SetCellState(int x, int y, bool isAlive)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _cells[x, y] = new Cell(isAlive, x, y);
            }
        }
        
        /// <summary>
        /// Проверяет, находятся ли координаты в пределах сетки
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
        
        /// <summary>
        /// Подсчитывает количество живых клеток в сетке
        /// </summary>
        public int CountAliveCells()
        {
            int count = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_cells[x, y].IsAlive) count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// Сбрасывает состояние всех клеток
        /// </summary>
        public void Reset()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SetCellState(x, y, false);
                }
            }
        }
    }
} 

