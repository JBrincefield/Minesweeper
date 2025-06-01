using Microsoft.Maui.Controls;

namespace Minesweeper.Decorators
{
    public class QuestionMarkDecorator : BaseCellDecorator
    {
        public QuestionMarkDecorator(ICellDecorator? inner = null) : base(inner) { }

        public override void Apply(Button button)
        {
            base.Apply(button);
            button.Text = "?";
            button.BackgroundColor = Colors.LightBlue;
        }
    }
}
