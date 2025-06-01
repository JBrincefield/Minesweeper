using Microsoft.Maui.Controls;

namespace Minesweeper.Decorators
{
    public class FlagDecorator : BaseCellDecorator
    {
        public FlagDecorator(ICellDecorator? inner = null) : base(inner) { }

        public override void Apply(Button button)
        {
            base.Apply(button);
            button.Text = "🚩";
            button.BackgroundColor = Colors.DarkOrange;
        }
    }
}
