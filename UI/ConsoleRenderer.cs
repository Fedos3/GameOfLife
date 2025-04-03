using GameOfLife.Models;
using System;

namespace GameOfLife.UI
{
    public class ConsoleRenderer : IRenderer
    {
        private const char ALIVE_CELL = '█';
        private const char DEAD_CELL = '░';
        private const char BORDER_H = '═';
        private const char BORDER_V = '║';
        private const char BORDER_CORNER_TL = '╔';
        private const char BORDER_CORNER_TR = '╗';
        private const char BORDER_CORNER_BL = '╚';
        private const char BORDER_CORNER_BR = '╝';
        private const ConsoleColor ALIVE_COLOR = ConsoleColor.Green;
        private const ConsoleColor DEAD_COLOR = ConsoleColor.DarkGray;
        private const ConsoleColor BORDER_COLOR = ConsoleColor.DarkCyan;
        private const ConsoleColor MENU_COLOR = ConsoleColor.White;
        private const ConsoleColor STATUS_COLOR = ConsoleColor.Yellow;
        private const ConsoleColor INFO_COLOR = ConsoleColor.Cyan;

        public void Render(ICell[,] grid)
        {
            Console.SetCursorPosition(0, 0);
            
            // Отрисовка верхней рамки
            Console.ForegroundColor = BORDER_COLOR;
            Console.Write(BORDER_CORNER_TL);
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Console.Write(BORDER_H);
            }
            Console.WriteLine(BORDER_CORNER_TR);

            // Отрисовка игрового поля
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Console.Write(BORDER_V);
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    var cell = grid[x, y];
                    Console.ForegroundColor = cell.IsAlive ? ALIVE_COLOR : DEAD_COLOR;
                    Console.Write(cell.IsAlive ? ALIVE_CELL : DEAD_CELL);
                }
                Console.ForegroundColor = BORDER_COLOR;
                Console.WriteLine(BORDER_V);
            }

            // Отрисовка нижней рамки
            Console.Write(BORDER_CORNER_BL);
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Console.Write(BORDER_H);
            }
            Console.WriteLine(BORDER_CORNER_BR);
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void ShowMenu()
        {
            Console.ForegroundColor = MENU_COLOR;
            Console.WriteLine("\nУправление:");
            Console.WriteLine("┌─────────────────────────────────────┐");
            Console.WriteLine("│ Space - Пауза/Продолжить            │");
            Console.WriteLine("│ R - Перезапуск                       │");
            Console.WriteLine("│ Q - Выход                           │");
            Console.WriteLine("│ N - Изменить тип соседства          │");
            Console.WriteLine("│ S - Изменить правила                │");
            Console.WriteLine("└─────────────────────────────────────┘");
        }

        public void ShowStatus(string status)
        {
            Console.ForegroundColor = STATUS_COLOR;
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write($"Статус: {status}");
        }

        public void ShowInfo(string rules, string neighborhood)
        {
            Console.ForegroundColor = INFO_COLOR;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.Write($"Правила: {rules} | Соседство: {neighborhood}");
        }
    }
} 
