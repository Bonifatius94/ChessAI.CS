/*
 * MIT License
 * 
 * Copyright(c) 2020 Marco Tröster
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Chess.GameLib;
using Chess.GameLib.Player;
using Chess.GameLib.Session;
using Chess.Lib;
using Chess.UI.Board;
using Chess.UI.Extensions;
using Chess.UI.Menu;
using Chess.UI.NewGame;
using Chess.UI.Status;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Chess.UI.Main
{
    // TODO: add XML Doc comments

    public class MainViewModel
    {
        #region Constructor

        public MainViewModel()
        {
            // init the chess board
            Board = new ChessBoardViewModel(onChessFieldClicked);
            Menu = new MenuViewModel(this);
            Status = new StatusBarViewModel();

            // try to restore the last game
            ReloadLastGame();
        }

        #endregion Constructor

        #region Members

        public ChessBoardViewModel Board { get; private set; }
        public MenuViewModel Menu { get; private set; }
        public StatusBarViewModel Status { get; private set; }

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
                Board.UpdatePieces(ChessBitboard.StartFormation);

                // tell the player which side he draws if he choose random side
                if (dlgContext.SelectedDrawingSideMode == DrawingSideMode.Random)
                {
                    MessageBox.Show($"You draw the { dlgContext.DrawingSide.ToString() } pieces!", "Random Side Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // update status bar
                Status.InitNewGame();

                // start the game session in a background thread (async)
                Task.Run(() => {

                    // play game (until one player loses)
                    var final = _session.ExecuteGame();

                    // update status bar
                    var finalStatus = ChessDrawSimulator.Instance.GetCheckGameStatus(_session.Board, _session.Game.LastDraw);
                    Status.FinishGame(finalStatus);

                    // reset game session
                    Board.UpdateIsEnabled(false);
                });
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

        private void boardChanged(IChessBoard newBoard)
        {
            // apply the draw to the UI
            Board.UpdatePieces(newBoard);

            // reset highlightings
            foreach (var field in Board.Fields) { field.UpdateHighlight(false); }

            // update status bar
            Status.UpdateGameLog(_session.Game.LastDrawOrDefault);
        }

        #endregion DrawInput

        #endregion Methods
    }
}
