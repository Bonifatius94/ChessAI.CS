using Chess.GameLib;
using Chess.GameLib.Player;
using Chess.GameLib.Session;
using Chess.Lib;
using Chess.UI.Board;
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
            //// init draw mutex (this needs to be first !!!)
            //initDrawInputMutex();

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
        private ChessColor _drawingSide;

        #endregion GameSession

        #region DrawInput

        private Mutex _drawInputMutex = new Mutex(true);
        private ChessPieceAtPos? _drawingPiece = null;
        private List<ChessDraw> _potentialDraws;
        private ChessDraw _drawToMake;

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
                _session = ChessGameSessionSerializer.Instance.Deserialize(SESSION_CACHE_FILE);
                _drawingSide = new List<IChessPlayer>() { _session.WhitePlayer as UIChessPlayer, _session.BlackPlayer as UIChessPlayer }.First(x => x != null).Side;
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
                var humanPlayer = new UIChessPlayer(dlgContext.DrawingSide, getNextDraw);
                var artificialPlayer = new ArtificialChessPlayer(dlgContext.DrawingSide.Opponent(), dlgContext.Difficulty);

                _drawingSide = dlgContext.DrawingSide;
                _session = dlgContext.DrawingSide == ChessColor.White ? new ChessGameSession(humanPlayer, artificialPlayer) : new ChessGameSession(artificialPlayer, humanPlayer);

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
            bool isAEnabled = _session != null;
            Board.UpdateIsEnabled(isAEnabled);
        }

        #endregion GameSession

        #region DrawInput

        //private void initDrawInputMutex()
        //{
        //    _mutex = new Mutex(false);
        //    _mutex.WaitOne();
        //}

        private void onChessFieldClicked(object parameter)
        {
            // identify the field that was clicked
            string fieldname = parameter as string;
            var position = new ChessPosition(fieldname);

            // determine the chess piece that was clicked (default: null)
            var targetPiece = _session.Board.IsCapturedAt(position) ? (ChessPieceAtPos?)new ChessPieceAtPos(position, _session.Board.GetPieceAt(position)) : null;

            // set drawing piece (if an allied piece is selected)
            if (targetPiece?.Piece.Color == _drawingSide)
            {
                // remember the drawing piece
                _drawingPiece = targetPiece;

                // get all potantial draws for the piece
                _potentialDraws = _session.Game.GetDraws(true).Where(x => x.OldPosition == _drawingPiece?.Position).ToList();

                // highlight the target fields of the draws (first reset all highlightings, just for good measure)
                foreach (var field in Board.Fields) { field.UpdateHighlight(false); }
                foreach (var field in Board.Fields) { if (_potentialDraws.Any(x => x.NewPosition == field.Position)) { field.UpdateHighlight(true); }  }
                Board.Fields[position.GetHashCode()].UpdateHighlight(true);
            }
            // only continue if an allied piece was selected before
            else if (_drawingPiece != null && _potentialDraws?.Count > 0 && _potentialDraws.Any(x => x.NewPosition == position))
            {
                // get the selected draw
                _drawToMake = _potentialDraws.First(x => x.NewPosition == position);

                // 1) release the mutex, so the draw gets passed to the chess session (this automatically applies the draw to the game session)
                // 2) wait 50 ms to ensure that the mutex can be captured by the other thread
                // 3) recapture the mutex, so the wait mechanism can be repeated
                _drawInputMutex.ReleaseMutex();
                Thread.Sleep(100);
                _drawInputMutex.WaitOne();

                // apply the draw to the UI
                Board.UpdatePieces(_session.Game.Board);

                // reset the temp variables and highlightings
                _drawingPiece = null;
                _potentialDraws = null;
                foreach (var field in Board.Fields) { field.UpdateHighlight(false); }
            }
        }

        private ChessDraw getNextDraw(ChessBoard board, ChessDraw? predecedingDraw)
        {
            // update the board (show opponent's draw)
            Board.UpdatePieces(_session.Game.Board);

            // wait until the player put his draw via UI
            _drawInputMutex.WaitOne();
            var draw = _drawToMake;
            _drawInputMutex.ReleaseMutex();

            // return the draw
            return draw;
        }

        // TODO: add a menu bar
        // TODO: implement start game logic etc.

        #endregion DrawInput

        #endregion Methods
    }
}
