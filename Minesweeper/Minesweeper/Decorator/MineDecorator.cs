using Microsoft.Maui.Controls;

namespace Minesweeper.Decorators
{
    public class MineDecorator : BaseCellDecorator
    {
        public MineDecorator(ICellDecorator? inner = null) : base(inner) { }

        public override void Apply(Button button)
        {
            base.Apply(button);
            button.BackgroundColor = Colors.Red;
            button.Text = "💣";
        }
    }
}
