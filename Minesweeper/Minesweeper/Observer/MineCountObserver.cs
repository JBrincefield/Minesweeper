using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Observer
{
    public class MineCountObserver : IGameObserver
    {
        private readonly Label _mineCountLabel;

        public MineCountObserver(Label mineCountLabel)
        {
            _mineCountLabel = mineCountLabel;
        }

        public void OnGameStateChanged(GameState state)
        {
            _mineCountLabel.Text = $"Mines left: {state.MinesRemaining}";
        }
    }
}
