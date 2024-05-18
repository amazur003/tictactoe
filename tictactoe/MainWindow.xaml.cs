using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        private enum Player { None, X, O };
        private Player[,] board = new Player[3, 3];
        private Player currentPlayer;
        private bool vsComputer;
        private DispatcherTimer timer;
        private DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            TimerTextBlock.Text = elapsed.ToString(@"mm\:ss");
        }

        private void ResetGame()
        {
            currentPlayer = Player.X;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    board[i, j] = Player.None;

            foreach (UIElement element in GameGrid.Children)
            {
                if (element is Border border && border.Child is Button button)
                {
                    button.Content = string.Empty;
                    button.IsEnabled = true;
                }
            }

            CurrentPlayerTextBlock.Text = currentPlayer.ToString();
            startTime = DateTime.Now;
            timer.Start();
        }

        private void PlayerVsPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            vsComputer = false;
            ModeSelectionPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Visible;
            ResetGame();
        }

        private void PlayerVsComputerButton_Click(object sender, RoutedEventArgs e)
        {
            vsComputer = true;
            ModeSelectionPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Visible;
            ResetGame();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int index = GameGrid.Children.IndexOf((UIElement)button.Parent);
            int row = index / 3;
            int col = index % 3;

            if (board[row, col] == Player.None)
            {
                board[row, col] = currentPlayer;
                button.Content = currentPlayer.ToString();
                button.IsEnabled = false;

                if (CheckForWin())
                {
                    timer.Stop();
                    MessageBox.Show($"Player {currentPlayer} wins!");
                    ResetGame();
                    return;
                }

                if (IsBoardFull())
                {
                    timer.Stop();
                    MessageBox.Show("It's a draw!");
                    ResetGame();
                    return;
                }

                currentPlayer = (currentPlayer == Player.X) ? Player.O : Player.X;
                CurrentPlayerTextBlock.Text = currentPlayer.ToString();

                if (vsComputer && currentPlayer == Player.O)
                {
                    ComputerMove();
                }
            }
        }

        private void ComputerMove()
        {
            var bestMove = FindBestMove();
            board[bestMove.Item1, bestMove.Item2] = Player.O;
            foreach (UIElement element in GameGrid.Children)
            {
                if (element is Border border && border.Child is Button button)
                {
                    int index = GameGrid.Children.IndexOf(border);
                    int row = index / 3;
                    int col = index % 3;
                    if (row == bestMove.Item1 && col == bestMove.Item2)
                    {
                        button.Content = Player.O.ToString();
                        button.IsEnabled = false;
                        break;
                    }
                }
            }

            if (CheckForWin())
            {
                timer.Stop();
                MessageBox.Show("Computer wins!");
                ResetGame();
                return;
            }

            if (IsBoardFull())
            {
                timer.Stop();
                MessageBox.Show("It's a draw!");
                ResetGame();
                return;
            }

            currentPlayer = Player.X;
            CurrentPlayerTextBlock.Text = currentPlayer.ToString();
        }

        private Tuple<int, int> FindBestMove()
        {
            int bestVal = int.MinValue;
            Tuple<int, int> bestMove = null;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == Player.None)
                    {
                        board[i, j] = Player.O;
                        int moveVal = Minimax(board, 0, false);
                        board[i, j] = Player.None;

                        if (moveVal > bestVal)
                        {
                            bestMove = Tuple.Create(i, j);
                            bestVal = moveVal;
                        }
                    }
                }
            }
            return bestMove;
        }

        private int Minimax(Player[,] board, int depth, bool isMax)
        {
            int score = Evaluate(board);

            if (score == 10 || score == -10)
                return score;

            if (IsBoardFull())
                return 0;

            if (isMax)
            {
                int best = int.MinValue;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == Player.None)
                        {
                            board[i, j] = Player.O;
                            best = Math.Max(best, Minimax(board, depth + 1, !isMax));
                            board[i, j] = Player.None;
                        }
                    }
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == Player.None)
                        {
                            board[i, j] = Player.X;
                            best = Math.Min(best, Minimax(board, depth + 1, !isMax));
                            board[i, j] = Player.None;
                        }
                    }
                }
                return best;
            }
        }

        private int Evaluate(Player[,] b)
        {
            for (int row = 0; row < 3; row++)
            {
                if (b[row, 0] == b[row, 1] && b[row, 1] == b[row, 2])
                {
                    if (b[row, 0] == Player.O)
                        return +10;
                    else if (b[row, 0] == Player.X)
                        return -10;
                }
            }

            for (int col = 0; col < 3; col++)
            {
                if (b[0, col] == b[1, col] && b[1, col] == b[2, col])
                {
                    if (b[0, col] == Player.O)
                        return +10;
                    else if (b[0, col] == Player.X)
                        return -10;
                }
            }

            if (b[0, 0] == b[1, 1] && b[1, 1] == b[2, 2])
            {
                if (b[0, 0] == Player.O)
                    return +10;
                else if (b[0, 0] == Player.X)
                    return -10;
            }

            if (b[0, 2] == b[1, 1] && b[1, 1] == b[2, 0])
            {
                if (b[0, 2] == Player.O)
                    return +10;
                else if (b[0, 2] == Player.X)
                    return -10;
            }

            return 0;
        }

        private bool IsBoardFull()
        {
            foreach (var cell in board)
            {
                if (cell == Player.None)
                    return false;
            }
            return true;
        }

        private bool CheckForWin()
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == currentPlayer && board[i, 1] == currentPlayer && board[i, 2] == currentPlayer)
                    return true;
                if (board[0, i] == currentPlayer && board[1, i] == currentPlayer && board[2, i] == currentPlayer)
                    return true;
            }
            if (board[0, 0] == currentPlayer && board[1, 1] == currentPlayer && board[2, 2] == currentPlayer)
                return true;
            if (board[0, 2] == currentPlayer && board[1, 1] == currentPlayer && board[2, 0] == currentPlayer)
                return true;

            return false;
        }
    }
}
