using Chess.GameLib;
using Chess.GameLib.Player;
using Chess.GameLib.Session;
using Chess.Lib;
using Chess.UI.Board;
using Chess.UI.Extensions;
using Chess.UI.Field;
using Chess.UI.Menu;
using Chess.UI.NewGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Chess.UI.Main
{
    public class MainViewModel
    {
        #region Constructor

        public MainViewModel()
        {
            // init the chess board
            Board = new ChessBoardViewModel(onChessFieldClicked);
            Menu = new MenuViewModel(this);

            // try to restore the last game
            ReloadLastGame();
        }

        #endregion Constructor

        #region Members

        public ChessBoardViewModel Board { get; private set; }
        public MenuViewModel Menu { get; private set; }

        #region GameSession

        private const string SESSION_CACHE_FILE = "temp-session.cgs";
        private ChessGameSession _session = null;
        private UIChessPlayer _player;

        #endregion GameSession

        #region DrawInput

        private ChessPieceAtPos? _drawingPiece = null;
        private List<ChessDraw> _potentialDraws;

        #endregion DrawInput

        #endregion Members

        #region Methods

        #region GameSession

        public void ReloadLastGame()
        {
            // check if a session cache is existing
            if (File.Exists(SESSION_CACHE_FILE))
            {
                // reload last session from cache
                var session = ChessGameSessionSerializer.Instance.Deserialize(SESSION_CACHE_FILE);
                var humanPlayer = new List<UIChessPlayer>() { session.WhitePlayer as UIChessPlayer, session.BlackPlayer as UIChessPlayer }.First(x => x != null);

                _session = session;
                _player = humanPlayer;

                _session.BoardChanged += boardChanged;
                updateIsBoardActive();
            }
        }

        public void NewGame()
        {
            // TODO: check why new game dialog opens when the main window closes

            // delete cache of last game
            if (File.Exists(SESSION_CACHE_FILE)) { File.Delete(SESSION_CACHE_FILE); }

            // show new game dialog
            var dlg = new NewGameView();
            var dlgContext = new NewGameViewModel(dlg);
            dlg.DataContext = dlgContext;
            var result = dlg.ShowDialog();

            if (result == true)
            {
                // init new game session with user inputs from dialog
                var humanPlayer = new UIChessPlayer(dlgContext.DrawingSide);
                var artificialPlayer = new ArtificialChessPlayer(dlgContext.DrawingSide.Opponent(), dlgContext.Difficulty);

                _session = dlgContext.DrawingSide == ChessColor.White ? new ChessGameSession(humanPlayer, artificialPlayer) : new ChessGameSession(artificialPlayer, humanPlayer);
                _player = humanPlayer;
                _session.BoardChanged += boardChanged;
                
                // tell the player which side he draws if he choose random side
                if (dlgContext.SelectedDrawingSideMode == DrawingSideMode.Random)
                {
                    MessageBox.Show($"You draw the { dlgContext.DrawingSide.ToString() } pieces!", "Random Side Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // start the game session in a background thread (async)
                Task.Run(() => _session.ExecuteGame());
            }

            // make the chess board inactive unless a game session is active
            updateIsBoardActive();
        }

        private void updateIsBoardActive()
        {
            // only enable chess board for receiving user inputs if there is an active game session
            bool isEnabled = _session != null;
            Board.UpdateIsEnabled(isEnabled);
        }

        #endregion GameSession

        #region DrawInput

        private void onChessFieldClicked(object parameter)
        {
            // identify the field that was clicked
            string fieldname = parameter as string;
            var position = new ChessPosition(fieldname);

            // determine the chess piece that was clicked (default: null)
            var targetPiece = _session.Board.IsCapturedAt(position) ? (ChessPieceAtPos?)new ChessPieceAtPos(position, _session.Board.GetPieceAt(position)) : null;

            // set drawing piece (if an allied piece is selected)
            if (targetPiece?.Piece.Color == _player.Side)
            {
                // remember the drawing piece
                _drawingPiece = targetPiece;

                // get all potantial draws for the piece
                _potentialDraws = _session.Game.GetDraws(true).Where(x => x.OldPosition == _drawingPiece?.Position).ToList();

                // highlight the target fields of the draws (first reset all highlightings, just for good measure)
                foreach (var field in Board.Fields) { field.UpdateHighlight(false); }
                foreach (var field in Board.Fields) { if (_potentialDraws.Any(x => x.NewPosition == field.Position)) { field.UpdateHighlight(true); }  }
                if (_potentialDraws?.Count > 0) { Board.Fields[position.GetHashCode()].UpdateHighlight(true); }
            }
            // only continue if an allied piece was selected before
            else if (_drawingPiece != null && _potentialDraws?.Count > 0 && _potentialDraws.Any(x => x.NewPosition == position))
            {
                // get the selected draw
                var drawToMake = _potentialDraws.First(x => x.NewPosition == position);
                _player.PassDraw(drawToMake);
            }
        }

        private void boardChanged(ChessBoard newBoard)
        {
            // apply the draw to the UI
            Board.UpdatePieces(newBoard);

            // reset highlightings
            foreach (var field in Board.Fields) { field.UpdateHighlight(false); }
        }

        #endregion DrawInput

        #endregion Methods
    }
}
