using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace Minesweeper
{
    public partial class MainPage : ContentPage
    {
        private Label _mineCountLabel;
        private Button[,] _buttons;
        private bool[,] _mines;
        private int _rows, _cols, _mineCount, _maxMines;
        private bool _minesGenerated = false;
        private bool _gameOver = false;


        public MainPage()
        {
            InitializeComponent();
        }

        private void StartGame(int rows, int cols, int mines)
        {
            _rows = rows;
            _cols = cols;
            _mineCount = mines;
            _maxMines = mines;
            _buttons = new Button[rows, cols];
            _mines = new bool[rows, cols];
            _minesGenerated = false;
            _gameOver = false;

            Grid gameBoard = new()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = 10
            };

            for (int i = 0; i < rows; i++)
                gameBoard.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            for (int j = 0; j < cols; j++)
                gameBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button cell = new()
                    {
                        BackgroundColor = Colors.LightGray,
                        Margin = 1,
                        WidthRequest = 40,
                        HeightRequest = 40,
                        CommandParameter = (i, j)
                    };
                    cell.Clicked += OnButtonClicked;

                    var pointerGesture = new PointerGestureRecognizer();
                    pointerGesture.PointerPressed += (s, e) => OnRightClicked(cell);

                    cell.GestureRecognizers.Add(pointerGesture);

                    _buttons[i, j] = cell;

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    gameBoard.Children.Add(cell);
                }
            }

            Button backButton = new()
            {
                Text = "Back to Menu",
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            backButton.Clicked += (s, e) => ResetToMainMenu();

            _mineCountLabel = new Label
            {
                Text = $"Mines left: {_mineCount}",
                HorizontalOptions = LayoutOptions.Center
            };

            VerticalStackLayout gameLayout = new()
            {
                Children = { backButton, _mineCountLabel, gameBoard }
            };

            ScrollView scrollView = new()
            {
                Content = gameLayout
            };

            Content = scrollView;
        }

        private void OnRightClicked(Button button)
        {
            if (button.CommandParameter is (int row, int col))
            {
                if (button.BackgroundColor == Colors.DarkGray)
                {
                    HashSet<(int, int)> cellsToReveal = GetSurroundingCells(row, col);

                    int mineCount = int.Parse(button.Text);

                    if (mineCount > 0)
                    {
                        int flaggedMines = 0;
                        foreach ((int newRow, int newCol) in cellsToReveal)
                        {
                            if (_buttons[newRow, newCol].BackgroundColor == Colors.DarkOrange)
                                flaggedMines++;
                        }
                        if (flaggedMines != mineCount)
                            return;
                    }

                    foreach ((int newRow, int newCol) in cellsToReveal)
                    {
                        RevealCells(newRow, newCol, true);
                    }
                }
                else if (button.BackgroundColor == Colors.LightGray)
                {
                    button.BackgroundColor = Colors.DarkOrange;
                    button.Text = "🚩";
                    _mineCount--;
                }
                else if (button.BackgroundColor == Colors.DarkOrange)
                {
                    button.BackgroundColor = Colors.LightBlue;
                    button.Text = "?";
                    _mineCount++;
                }
                else if (button.BackgroundColor == Colors.LightBlue)
                {
                    button.BackgroundColor = Colors.LightGray;
                    button.Text = "";
                }

                _mineCountLabel.Text = $"Mines left: {_mineCount}";
            }
        }


        private void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is (int row, int col))
            {
                if (button.BackgroundColor == Colors.DarkGray || button.BackgroundColor == Colors.DarkOrange)
                    return;
                if (button.BackgroundColor == Colors.LightBlue)
                    button.Text = "";

                if (!_minesGenerated)
                {
                    GenerateMines(row, col);
                    _minesGenerated = true;
                }

                RevealCells(row, col, false);
            }
        }

        private async void RevealCells(int row, int col, bool isRevealedClick)
        {
            Queue<(int, int)> queue = new();
            HashSet<(int, int)> visited = [];
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                (int currentRow, int currentCol) = queue.Dequeue();

                if (visited.Contains((currentRow, currentCol)))
                    continue;

                Button button = _buttons[currentRow, currentCol];

                if (isRevealedClick && (button.BackgroundColor == Colors.LightBlue || button.BackgroundColor == Colors.DarkOrange))
                    continue;
                if (_mines[currentRow, currentCol])
                {
                    if (button.BackgroundColor == Colors.DarkOrange)
                        continue;

                    await EndGame(false);
                    return;
                }

                visited.Add((currentRow, currentCol));

                if (button.BackgroundColor == Colors.LightBlue)
                    button.Text = "";

                button.BackgroundColor = Colors.DarkGray;

                HashSet<(int, int)> surroundingCells = GetSurroundingCells(currentRow, currentCol);
                int mineCount = GetMineCount(surroundingCells);

                if (mineCount > 0)
                {
                    button.Text = mineCount.ToString();

                }
                else
                {
                    foreach ((int newRow, int newCol) in surroundingCells)
                    {
                        Button neighbor = _buttons[newRow, newCol];

                        if (neighbor.BackgroundColor == Colors.DarkOrange)
                        {
                            _mineCount++;
                            neighbor.Text = "";
                        }

                        if (neighbor.BackgroundColor == Colors.LightBlue)
                            neighbor.Text = "";


                        neighbor.BackgroundColor = Colors.DarkGray;

                        if (!visited.Contains((newRow, newCol)))
                        {
                            queue.Enqueue((newRow, newCol));
                        }
                    }
                }
            }
            CheckForWin();
        }

        private async void CheckForWin()
        {
            int revealedCells = 0;

            foreach (Button button in _buttons)
            {
                if (button.BackgroundColor == Colors.DarkGray)
                    revealedCells++;
            }

            if (revealedCells == _buttons.Length - _maxMines)
            {
                await EndGame(true);
            }
        }




        private async Task EndGame(bool isWin)
        {
            if (_gameOver) return;
            _gameOver = true;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    Button button = _buttons[i, j];

                    if (_mines[i, j])
                    {
                        if (button.BackgroundColor == Colors.DarkOrange)
                        {
                            continue;
                        }
                        else
                        {
                            button.BackgroundColor = Colors.Red;
                            button.Text = "💣";
                        }
                    }
                }
            }

            string title = isWin ? "🎉 YOU WIN!" : "💥 GAME OVER!";
            string message = isWin ? "You cleared all safe spots!" : "You hit a mine!";

            await DisplayAlert(title, message, "OK");

            ResetToMainMenu();
        }


        private void ResetToMainMenu()
        {
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Minesweeper", FontSize = 24, HorizontalOptions = LayoutOptions.Center, Margin = 10 },
                    new Button { Text = "Easy (9x9, 10 mines)", Command = new Command(() => StartGame(9, 9, 10)) },
                    new Button { Text = "Medium (16x16, 40 mines)", Command = new Command(() => StartGame(16, 16, 40)) },
                    new Button { Text = "Hard (30x16, 99 mines)", Command = new Command(() => StartGame(30, 16, 99)) }
                }
            };
        }

        private void GenerateMines(int safeRow, int safeCol)
        {
            Random random = new();
            int minesPlaced = 0;

            HashSet<(int, int)> excludedCells = GetSurroundingCells(safeRow, safeCol);
            excludedCells.Add((safeRow, safeCol));

            while (minesPlaced < _maxMines)
            {
                int row = random.Next(_rows);
                int col = random.Next(_cols);

                if (_mines[row, col] || excludedCells.Contains((row, col)))
                    continue;

                _mines[row, col] = true;
                minesPlaced++;
            }
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

                if (newRow >= 0 && newRow < _rows && newCol >= 0 && newCol < _cols)
                {
                    neighbors.Add((newRow, newCol));
                }
            }

            return neighbors;
        }

        private int GetMineCount(HashSet<(int, int)> surroundingCells)
        {
            int mineCount = 0;

            foreach ((int newRow, int newCol) in surroundingCells)
            {
                if (_mines[newRow, newCol])
                    mineCount++;
            }

            return mineCount;
        }

        private void OnEasyClicked(object sender, EventArgs e) => StartGame(9, 9, 10);
        private void OnMediumClicked(object sender, EventArgs e) => StartGame(16, 16, 40);
        private void OnHardClicked(object sender, EventArgs e) => StartGame(30, 16, 99);
    }
}
