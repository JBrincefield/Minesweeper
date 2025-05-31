namespace Minesweeper.Strategies
{
    public interface IMineGenerationStrategy
    {
        bool[,] GenerateMines(int rows, int cols, int mineCount, (int, int) safeCell);
    }
}
