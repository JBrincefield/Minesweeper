namespace Minesweeper.Observer
{
    public interface IGameObserver
    {
        void OnGameStateChanged(GameState state);
    }
}
