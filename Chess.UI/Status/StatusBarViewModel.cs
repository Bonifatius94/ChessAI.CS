using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.UI.Status
{
    public class StatusBarViewModel : PropertyChangedBase
    {
        #region Members

        // TODO: implement all those features in ChessGame / ChessGameSession class and just show them here ...
        private ChessDraw? _lastDraw = null;
        private CancellationTokenSource _clockToken;
        private DateTime _drawStart;
        private int _drawIndex = 0;
        private ChessBitboard _tempBoard = ChessBitboard.StartFormation;

        public string StatusText { get; private set; }
        public string GameLog { get; private set; } = string.Empty;

        #endregion Members

        #region Methods

        // TODO: add logic to reload a game status of a stored game

        public void InitNewGame()
        {
            GameLog = "new game started\r\n================================================\r\n" + GameLog;
            NotifyPropertyChanged(nameof(GameLog));
            restartPlayclock();
        }

        public void FinishGame(ChessGameStatus status)
        {
            // TODO: refactor printing the game status as extension function

            // write final game status to game log
            if (status == ChessGameStatus.Checkmate)
            {
                GameLog = $"checkmate, { _lastDraw?.DrawingSide.Opponent() } player won!\r\n{ GameLog }";
            }
            else if (status == ChessGameStatus.Stalemate)
            {
                GameLog = $"stalemate!\r\n{ GameLog }";
            }
            else if (status == ChessGameStatus.Tie || status == ChessGameStatus.UnsufficientPieces)
            {
                GameLog = $"unsifficient pieces, { _lastDraw?.DrawingSide.Opponent() } player cannot win anymore!\r\n{ GameLog }";
            }

            // reset local game cache variables
            _drawIndex = 0;
            _drawStart = DateTime.Now;
            _lastDraw = null;
            _tempBoard = ChessBitboard.StartFormation;
            _clockToken.Cancel();

            // update view
            StatusText = string.Empty;
            NotifyPropertyChanged(nameof(StatusText));
            NotifyPropertyChanged(nameof(GameLog));
        }

        public void UpdateGameLog(ChessDraw? newDraw)
        {
            // update draw cache
            _lastDraw = newDraw;
            _drawIndex++;

            if (newDraw != null)
            {
                // write the draw to the game log
                var elapsedTime = DateTime.Now - _drawStart;
                GameLog = $"{ _drawIndex }: { newDraw.Value.DrawingSide } player drew { newDraw.ToString() }, took { elapsedTime.ToString(@"mm\:ss") }\r\n{ GameLog }";
                NotifyPropertyChanged(nameof(GameLog));
            }

            restartPlayclock();
        }

        private void restartPlayclock()
        {
            // stop last draw's playclock
            _clockToken?.Cancel();

            // restart playclock for draw
            _clockToken = new CancellationTokenSource();
            Task.Run(() => updatePlayclock(_clockToken.Token));
        }

        private void updatePlayclock(CancellationToken cancel)
        {
            _drawStart = DateTime.Now;

            while (!cancel.IsCancellationRequested)
            {
                var drawingSide = _lastDraw?.DrawingSide.Opponent() ?? ChessColor.White;
                var elapsedTime = DateTime.Now - _drawStart;

                StatusText = $"{ drawingSide } player to draw ... { elapsedTime.ToString(@"mm\:ss") }";
                NotifyPropertyChanged(nameof(StatusText));

                System.Threading.Thread.Sleep(1000);
            }
        }

        #endregion Methods
    }
}
