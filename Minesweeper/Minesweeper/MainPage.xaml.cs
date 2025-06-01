using Microsoft.Maui.Controls;
using Minesweeper.Observer;
using Minesweeper.Strategies;
using Minesweeper.Decorators;
using System;
using System.Collections.Generic;

namespace Minesweeper
{
    public partial class MainPage : ContentPage
    {
        private IMineGenerationStrategy _mineStrategy = new RandomMineGenerationStrategy();

        private readonly List<IGameObserver> _observers = new();


        private Label _mineCountLabel;
        private Button[,] _buttons;
        private bool[,] _mines;
        private int _rows, _cols, _mineCount, _maxMines;
        private bool _minesGenerated = false;
        private bool _gameOver = false;
        private bool _isWin = false;


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
            _isWin = false;

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

            RegisterObserver(new MineCountObserver(_mineCountLabel));

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
                    new FlagDecorator().Apply(button);
                    _mineCount--;
                    NotifyObservers();
                }
                else if (button.BackgroundColor == Colors.DarkOrange)
                {
                    new QuestionMarkDecorator().Apply(button);
                    _mineCount++;
                    NotifyObservers();
                }
                else if (button.BackgroundColor == Colors.LightBlue)
                {
                    button.BackgroundColor = Colors.LightGray;
                    button.Text = "";
                }

                NotifyObservers();
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

                    await EndGame();
                    return;
                }

                visited.Add((currentRow, currentCol));

                if (button.BackgroundColor == Colors.LightBlue)
                    button.Text = "";


                HashSet<(int, int)> surroundingCells = GetSurroundingCells(currentRow, currentCol);
                int mineCount = GetMineCount(surroundingCells);

                if (mineCount > 0)
                {
                    new RevealDecorator(mineCount > 0 ? mineCount.ToString() : "").Apply(button);

                }
                else
                {
                    foreach ((int newRow, int newCol) in surroundingCells)
                    {
                        Button neighbor = _buttons[newRow, newCol];

                        if (neighbor.BackgroundColor == Colors.DarkOrange)
                        {
                            _mineCount++;
                            NotifyObservers();
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
                _isWin = true;
                await EndGame();
            }
        }




        private async Task EndGame()
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
                            new MineDecorator().Apply(button);
                        }
                    }
                }
            }

            string title = _isWin ? "🎉 YOU WIN!" : "💥 GAME OVER!";
            string message = _isWin ? "You cleared all safe spots!" : "You hit a mine!";

            await DisplayAlert(title, message, "OK");
            NotifyObservers();

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
            _mines = _mineStrategy.GenerateMines(_rows, _cols, _maxMines, (safeRow, safeCol));
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

        public void RegisterObserver(IGameObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void RemoveObserver(IGameObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            var state = new GameState(_mineCount, _gameOver, _gameOver && _isWin);
            foreach (var observer in _observers)
            {
                observer.OnGameStateChanged(state);
            }
        }
    }
}