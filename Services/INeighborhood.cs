using GameOfLife.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Services
{
    public interface INeighborhood
    {
        IEnumerable<ICell> GetNeighbors(Grid grid, int x, int y);
    }

    public class MooreNeighborhood : INeighborhood
    {
        public IEnumerable<ICell> GetNeighbors(Grid grid, int x, int y)
        {
            var neighbors = new List<ICell>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int newX = x + dx;
                    int newY = y + dy;

                    if (newX >= 0 && newX < grid.Width && newY >= 0 && newY < grid.Height)
                    {
                        neighbors.Add(grid[newX, newY]);
                    }
                }
            }
            return neighbors;
        }
    }

    public class VonNeumannNeighborhood : INeighborhood
    {
        public IEnumerable<ICell> GetNeighbors(Grid grid, int x, int y)
        {
            var neighbors = new List<ICell>();
            var directions = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

            foreach (var (dx, dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (newX >= 0 && newX < grid.Width && newY >= 0 && newY < grid.Height)
                {
                    neighbors.Add(grid[newX, newY]);
                }
            }
            return neighbors;
        }
    }
} 