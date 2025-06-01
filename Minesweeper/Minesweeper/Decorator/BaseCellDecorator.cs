using Microsoft.Maui.Controls;
using Minesweeper.Decorators;

namespace Minesweeper.Decorators
{
    public class BaseCellDecorator : ICellDecorator
    {
        protected readonly ICellDecorator? _inner;

        public BaseCellDecorator(ICellDecorator? inner = null)
        {
            _inner = inner;
        }

        public virtual void Apply(Button button)
        {
            _inner?.Apply(button);
        }
    }
}
