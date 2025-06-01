using Microsoft.Maui.Controls;

namespace Minesweeper.Decorators
{
    public class RevealDecorator : BaseCellDecorator
    {
        private readonly string? _text;

        public RevealDecorator(string? text = null, ICellDecorator? inner = null) : base(inner)
        {
            _text = text;
        }

        public override void Apply(Button button)
        {
            base.Apply(button);
            button.BackgroundColor = Colors.DarkGray;
            button.Text = _text ?? string.Empty;
        }
    }
}
