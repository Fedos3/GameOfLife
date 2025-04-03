using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife.Models
{
    /// <summary>
    /// Содержит различные шаблоны клеток для игры "Жизнь"
    /// </summary>
    public static class Patterns
    {
        /// <summary>
        /// Неподвижные фигуры, которые не меняются со временем
        /// </summary>
        public static class StillLifes
        {
            /// <summary>
            /// Блок (2x2 квадрат)
            /// </summary>
            public static bool[,] Block = new bool[,]
            {
                { true, true },
                { true, true }
            };

            /// <summary>
            /// Улей (шестиклеточная структура)
            /// </summary>
            public static bool[,] Beehive = new bool[,]
            {
                { false, true, true, false },
                { true, false, false, true },
                { false, true, true, false }
            };

            /// <summary>
            /// Буханка (7-клеточная структура)
            /// </summary>
            public static bool[,] Loaf = new bool[,]
            {
                { false, true, true, false },
                { true, false, false, true },
                { false, true, false, true },
                { false, false, true, false }
            };
        }

        /// <summary>
        /// Осцилляторы - фигуры, которые периодически возвращаются к своему начальному состоянию
        /// </summary>
        public static class Oscillators
        {
            /// <summary>
            /// Мигалка (период 2) - простейший осциллятор
            /// </summary>
            public static bool[,] Blinker = new bool[,]
            {
                { false, false, false },
                { true, true, true },
                { false, false, false }
            };

            /// <summary>
            /// Жаба (период 2)
            /// </summary>
            public static bool[,] Toad = new bool[,]
            {
                { false, false, false, false },
                { false, true, true, true },
                { true, true, true, false },
                { false, false, false, false }
            };

            /// <summary>
            /// Маяк (период 2)
            /// </summary>
            public static bool[,] Beacon = new bool[,]
            {
                { true, true, false, false },
                { true, true, false, false },
                { false, false, true, true },
                { false, false, true, true }
            };
        }

        /// <summary>
        /// Космические корабли - структуры, которые перемещаются по полю
        /// </summary>
        public static class Spaceships
        {
            /// <summary>
            /// Планер - простейший космический корабль
            /// </summary>
            public static bool[,] Glider = new bool[,]
            {
                { false, true, false },
                { false, false, true },
                { true, true, true }
            };

            /// <summary>
            /// Легкий космический корабль (ЛК)
            /// </summary>
            public static bool[,] LightweightSpaceship = new bool[,]
            {
                { false, true, false, false, true },
                { true, false, false, false, false },
                { true, false, false, false, true },
                { true, true, true, true, false }
            };
        }

        /// <summary>
        /// Долгожители - структуры, которые эволюционируют длительное время
        /// </summary>
        public static class Methuselahs
        {
            /// <summary>
            /// R-пентамино - долгоживущая конфигурация
            /// </summary>
            public static bool[,] RPentomino = new bool[,]
            {
                { false, true, true },
                { true, true, false },
                { false, true, false }
            };

            /// <summary>
            /// Diehard - вымирает через 130 поколений
            /// </summary>
            public static bool[,] Diehard = new bool[,]
            {
                { false, false, false, false, false, false, true, false },
                { true, true, false, false, false, false, false, false },
                { false, true, false, false, false, true, true, true }
            };
        }
    }
} 