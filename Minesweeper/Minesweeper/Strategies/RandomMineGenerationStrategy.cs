using Minesweeper.Strategies;
using System;
using System.Collections.Generic;

namespace Minesweeper {
    public class RandomMineGenerationStrategy : IMineGenerationStrategy
    {
        public bool[,] GenerateMines(int rows, int cols, int mineCount, (int, int) safeCell)
        {
            bool[,] mines = new bool[rows, cols];
            Random random = new();
            HashSet<(int, int)> excludedCells = GetSurroundingCells(safeCell.Item1, safeCell.Item2);
            excludedCells.Add(safeCell);
            int placed = 0;

            while (placed < mineCount)
            {
                int r = random.Next(rows);
                int c = random.Next(cols);

                if (mines[r, c] || excludedCells.Contains((r, c)))
                    continue;

                mines[r, c] = true;
                placed++;
            }

            return mines;
        }

        private HashSet<(int, int)> GetSurroundingCells(int row, int col)
        {
            int[] dx = [-1, -1, -1, 0, 0, 1, 1, 1];
            int[] dy = [-1, 0, 1, -1, 1, -1, 0, 1];
            HashSet<(int, int)> neighbors = [];

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dx[i];
                int newCol = col + dy[i];

                if (newRow >= 0 && newRow < 30 && newCol >= 0 && newCol < 30)
                    neighbors.Add((newRow, newCol));
            }

            return neighbors;
        }
    }
}
