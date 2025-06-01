using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Observer
{
    public class GameState
    {
        public int MinesRemaining { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }

        public GameState(int minesRemaining, bool isGameOver = false, bool isWin = false)
        {
            MinesRemaining = minesRemaining;
            IsGameOver = isGameOver;
            IsWin = isWin;
        }
    }
}
