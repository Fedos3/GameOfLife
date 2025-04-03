using GameOfLife.Models;

namespace GameOfLife.UI
{
    public interface IRenderer
    {
        void Render(ICell[,] grid);
        void Clear();
        void ShowMenu();
        void ShowStatus(string status);
    }
} 