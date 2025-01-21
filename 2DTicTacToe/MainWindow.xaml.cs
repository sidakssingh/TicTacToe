using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace _2DTicTacToe
{
    public partial class MainWindow : Window
    {
        private Point m_point;
        private Vector m_vector;
        private List<Border> borders = new List<Border>();

        /* The current state of the game and where all game pieces are is kept in the
         * state array. State is a two dimensional array whose dimensions match those
         * of the tic tac toe grid of the corresponding square.
         * 0 = no piece, 1 = X, 2 = O. */
        private int[,] state = new int[3, 3];
        private bool isOnImage;
        private bool isTurn = true;     // X starts first
        private bool gameIsOver = false;

        // Scoreboard tracking
        private int xWinsCount = 0;
        private int oWinsCount = 0;
        private int tiesCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Add the 9 borders to our list
            borders.Add(b00);
            borders.Add(b01);
            borders.Add(b02);
            borders.Add(b10);
            borders.Add(b11);
            borders.Add(b12);
            borders.Add(b20);
            borders.Add(b21);
            borders.Add(b22);
        }

        /* Whenever a game piece is about to be moved, we need to cature two pieces of
         * initial information:
         *     1.  The current position of the mouse when the dragging begins
         *     2.  The current position on the canvas of the target game piece */
        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            GamePiece piece = sender as GamePiece;
            // If it has already been placed or the game is over, do not allow movement.
            if (!piece.IsPlaced && !gameIsOver)
            {
                m_point = Mouse.GetPosition(piece);
                m_vector = VisualTreeHelper.GetOffset(piece);
            }
            isOnImage = true;
        }

        /* When a game piece is dragged around the main window, we render this visually
         * by doing a transform on its X and Y coordinate position. */
        private void Image_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            GamePiece piece = sender as GamePiece;
            if (piece.IsPlaced || gameIsOver)
                return;

            // Only allow the correct player's piece to move
            bool correctPiece =
                (piece.Source.Contains("LetterX.png") && isTurn) ||
                (piece.Source.Contains("LetterO.png") && !isTurn);

            if (e.LeftButton == MouseButtonState.Pressed && isOnImage && correctPiece)
            {
                Mouse.Capture(piece);
                Point temp = Mouse.GetPosition((FrameworkElement)(piece.Parent));
                TranslateTransform transform = new TranslateTransform
                {
                    X = temp.X - m_vector.X - m_point.X,
                    Y = temp.Y - m_vector.Y - m_point.Y
                };
                piece.RenderTransform = transform;

                /* We want any available square to "turn gold" if a game piece is dragged over it and it is valid */
                Border b = OverGridSquare(temp);
                if (b != null && SquareNotTaken(b))
                {
                    ResetBorderColors();
                    b.BorderBrush = Brushes.Gold;
                }
                else
                {
                    ResetBorderColors();
                }
            }
        }

        /* When a game piece is "let go" it must either float to the center of the chosen
         * grid quare, or float back to its starting position */
        private void Image_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            GamePiece piece = sender as GamePiece;

            if (piece.IsPlaced || gameIsOver)
                return;

            bool placedOnBoard = false;
            foreach (Border border in borders)
            {
                if (border.BorderBrush == Brushes.Gold)
                {
                    // Determine whether this piece is an X or O
                    int val = piece.Source.Contains("LetterX.png") ? 1 : 2;

                    // Identify the square
                    int row = border.Name[1] - '0';
                    int col = border.Name[2] - '0';
                    state[row, col] = val;

                    // Animate to center of the chosen border
                    Point topLeft = piece.TranslatePoint(new Point(0, 0), myCanvas);
                    piece.RenderTransform = null;

                    double newLeft = 175 + col * 100;
                    double newTop = 175 + row * 100;

                    DoubleAnimation animLeft = new DoubleAnimation(topLeft.X, newLeft, TimeSpan.FromMilliseconds(500));
                    animLeft.Completed += new EventHandler(da_Completed);
                    piece.BeginAnimation(Canvas.LeftProperty, animLeft);

                    DoubleAnimation animTop = new DoubleAnimation(topLeft.Y, newTop, TimeSpan.FromMilliseconds(500));
                    animTop.Completed += new EventHandler(da_Completed);
                    piece.BeginAnimation(Canvas.TopProperty, animTop);

                    piece.IsPlaced = true;
                    isTurn = !isTurn; // switch turn
                    placedOnBoard = true;
                    break;
                }
            }

            if (!placedOnBoard)
            {
                // Animate back to start if not placed on the board
                AnimatePieceBack(piece);
            }

            // Check if the move ended the game
            if (placedOnBoard)
            {
                CheckForWinOrTie();
            }

            isOnImage = false;
        }

        /* This is a helper function which will determine if a square is taken already */
        private bool SquareNotTaken(Border border)
        {
            int row = border.Name[1] - '0';
            int col = border.Name[2] - '0';
            return (state[row, col] == 0);
        }

        /* Determine if the mouse is over a particular grid square, by looking at the bounding boxes */
        private Border OverGridSquare(Point point)
        {
            if (point.X >= 150 && point.X <= 250 && point.Y >= 150 && point.Y <= 250) return b00;
            if (point.X >= 250 && point.X <= 350 && point.Y >= 150 && point.Y <= 250) return b01;
            if (point.X >= 350 && point.X <= 450 && point.Y >= 150 && point.Y <= 250) return b02;
            if (point.X >= 150 && point.X <= 250 && point.Y >= 250 && point.Y <= 350) return b10;
            if (point.X >= 250 && point.X <= 350 && point.Y >= 250 && point.Y <= 350) return b11;
            if (point.X >= 350 && point.X <= 450 && point.Y >= 250 && point.Y <= 350) return b12;
            if (point.X >= 150 && point.X <= 250 && point.Y >= 350 && point.Y <= 450) return b20;
            if (point.X >= 250 && point.X <= 350 && point.Y >= 350 && point.Y <= 450) return b21;
            if (point.X >= 350 && point.X <= 450 && point.Y >= 350 && point.Y <= 450) return b22;

            return null;
        }

        /* Once animations complete, reset all borders to black. */
        private void da_Completed(object sender, EventArgs e)
        {
            ResetBorderColors();
        }

        /* Animate the piece back to its starting position. */
        private void AnimatePieceBack(GamePiece piece)
        {
            Point topLeft = piece.TranslatePoint(new Point(0, 0), myCanvas);
            piece.RenderTransform = null;

            // Cancel any existing animations so we can forcibly set positions
            piece.BeginAnimation(Canvas.LeftProperty, null);
            piece.BeginAnimation(Canvas.TopProperty, null);

            double startLeft = Canvas.GetLeft(piece);
            double startTop = Canvas.GetTop(piece);

            DoubleAnimation daX = new DoubleAnimation(topLeft.X, startLeft, TimeSpan.FromMilliseconds(500));
            piece.BeginAnimation(Canvas.LeftProperty, daX);

            DoubleAnimation daY = new DoubleAnimation(topLeft.Y, startTop, TimeSpan.FromMilliseconds(500));
            piece.BeginAnimation(Canvas.TopProperty, daY);
        }

        /* Check if 'X' or 'O' has won, or if there is a tie. */
        private void CheckForWinOrTie()
        {
            // Check X = 1
            if (CheckPlayerWin(1))
            {
                tempLabel.Content = "X Wins!";
                xWinsCount++;
                xWinsLabel.Content = "Number of games won by X: " + xWinsCount;
                gameIsOver = true;
                return;
            }
            // Check O = 2
            if (CheckPlayerWin(2))
            {
                tempLabel.Content = "O Wins!";
                oWinsCount++;
                oWinsLabel.Content = "Number of games won by O: " + oWinsCount;
                gameIsOver = true;
                return;
            }

            // If neither X nor O has won, check for tie
            bool boardFull = true;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (state[r, c] == 0)
                    {
                        boardFull = false;
                        break;
                    }
                }
            }

            // If full but no winner, it's a tie
            if (boardFull)
            {
                tempLabel.Content = "Tie!";
                tiesCount++;
                tieLabel.Content = "Number of ties: " + tiesCount;
                gameIsOver = true;
            }
        }

        /* Check all lines for the given player's number (1: X, 2: O). */
        private bool CheckPlayerWin(int player)
        {
            // Rows
            if (state[0, 0] == player && state[0, 1] == player && state[0, 2] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawHorizontalLine(200);
                return true;
            }
            if (state[1, 0] == player && state[1, 1] == player && state[1, 2] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawHorizontalLine(300);
                return true;
            }
            if (state[2, 0] == player && state[2, 1] == player && state[2, 2] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawHorizontalLine(400);
                return true;
            }

            // Columns
            if (state[0, 0] == player && state[1, 0] == player && state[2, 0] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawVerticalLine(200);
                return true;
            }
            if (state[0, 1] == player && state[1, 1] == player && state[2, 1] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawVerticalLine(300);
                return true;
            }
            if (state[0, 2] == player && state[1, 2] == player && state[2, 2] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawVerticalLine(400);
                return true;
            }

            // Diagonals
            if (state[0, 0] == player && state[1, 1] == player && state[2, 2] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawDiagonalMain();
                return true;
            }
            if (state[0, 2] == player && state[1, 1] == player && state[2, 0] == player)
            {
                tempLabel.Content = (player == 1) ? "X Wins!" : "O Wins!";
                DrawDiagonalOther();
                return true;
            }

            return false;
        }

        /* Draw lines for the winning combinations. */
        private void DrawHorizontalLine(double y)
        {
            Line myLine = new Line();
            myLine.Stroke = Brushes.Blue;
            myLine.X1 = 150;
            myLine.X2 = 450;
            myLine.Y1 = y;
            myLine.Y2 = y;
            myLine.StrokeThickness = 2;
            myCanvas.Children.Add(myLine);
        }

        private void DrawVerticalLine(double x)
        {
            Line myLine = new Line();
            myLine.Stroke = Brushes.Blue;
            myLine.X1 = x;
            myLine.X2 = x;
            myLine.Y1 = 150;
            myLine.Y2 = 450;
            myLine.StrokeThickness = 2;
            myCanvas.Children.Add(myLine);
        }

        private void DrawDiagonalMain()
        {
            Line myLine = new Line();
            myLine.Stroke = Brushes.Blue;
            myLine.X1 = 150;
            myLine.X2 = 450;
            myLine.Y1 = 150;
            myLine.Y2 = 450;
            myLine.StrokeThickness = 2;
            myCanvas.Children.Add(myLine);
        }

        private void DrawDiagonalOther()
        {
            Line myLine = new Line();
            myLine.Stroke = Brushes.Blue;
            myLine.X1 = 150;
            myLine.X2 = 450;
            myLine.Y1 = 450;
            myLine.Y2 = 150;
            myLine.StrokeThickness = 2;
            myCanvas.Children.Add(myLine);
        }

        /* Reset all borders to black so none remain gold. */
        private void ResetBorderColors()
        {
            b00.BorderBrush = Brushes.Black;
            b01.BorderBrush = Brushes.Black;
            b02.BorderBrush = Brushes.Black;
            b10.BorderBrush = Brushes.Black;
            b11.BorderBrush = Brushes.Black;
            b12.BorderBrush = Brushes.Black;
            b20.BorderBrush = Brushes.Black;
            b21.BorderBrush = Brushes.Black;
            b22.BorderBrush = Brushes.Black;
        }

        /* Clicking "Play Again" to reset the board (but keep the scoreboard). */
        private void tempButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear out the state array
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    state[r, c] = 0;
                }
            }

            // Remove any winning lines from the canvas
            for (int i = myCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (myCanvas.Children[i] is Line)
                {
                    myCanvas.Children.RemoveAt(i);
                }
            }

            // Clear label
            tempLabel.Content = "";

            // Reset each piece to its original location and remove all animations
            ResetGamePiece(x1, 30, 180);
            ResetGamePiece(x2, 30, 260);
            ResetGamePiece(x3, 30, 340);
            ResetGamePiece(x4, 30, 420);
            ResetGamePiece(x5, 30, 500);

            ResetGamePiece(o1, 530, 260);
            ResetGamePiece(o2, 530, 340);
            ResetGamePiece(o3, 530, 420);
            ResetGamePiece(o4, 530, 500);

            // X always starts first again
            isTurn = true;
            gameIsOver = false;
            ResetBorderColors();
        }

        /* Helper method to reset a single piece back to its "starting" location on the left or right. */
        private void ResetGamePiece(GamePiece piece, double left, double top)
        {
            // Cancel any leftover animations
            piece.BeginAnimation(Canvas.LeftProperty, null);
            piece.BeginAnimation(Canvas.TopProperty, null);

            // Remove any RenderTransform
            piece.RenderTransform = null;

            // Mark that it's no longer placed on the board
            piece.IsPlaced = false;

            // Move it to the original location
            Canvas.SetLeft(piece, left);
            Canvas.SetTop(piece, top);
        }
    }
}
