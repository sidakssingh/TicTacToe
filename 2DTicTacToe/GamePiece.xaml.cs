using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace _2DTicTacToe
{
    /// <summary>
    /// Interaction logic for GamePiece.xaml
    /// </summary>
    public partial class GamePiece : UserControl
    {
        public GamePiece()
        {
            InitializeComponent();
        }

        /* This is needed so that we can set the 'Source' in the XAML or code-behind.
         * Whenever 'Source' is assigned, we load that image into the 'imgPiece' control. */
        private string source;
        public string Source
        {
            get { return source; }
            set
            {
                source = value;
                // Load the image from a relative URI (e.g. "/img/LetterX.png")
                imgPiece.Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }

        /* Indicates whether this game piece has been placed on the board yet.
         * If true, we no longer allow it to be dragged. */
        private bool isPlaced = false;
        public bool IsPlaced
        {
            get { return isPlaced; }
            set { isPlaced = value; }
        }
    }
}
