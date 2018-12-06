using Chess.Lib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.WebApi.Server.Helpers
{
    public class ChessMatchSession
    {
        #region Constructor

        public ChessMatchSession(int gameId)
        {
            _gameId = gameId;
            _mutexWait.WaitOne();
        }

        #endregion Constructor

        #region Members
        
        // game session data
        private int _gameId;
        private ChessGame _game = new ChessGame();

        // handle 'wait for answer' logic
        private const int WAIT_TIMEOUT_MS = 250000;
        private readonly Mutex _mutexWait = new Mutex();
        private bool _isWaiting = false;

        #endregion Members

        #region Methods

        /// <summary>
        /// Wait until the opponent submits his answer.
        /// </summary>
        /// <returns>the opponent's answer</returns>
        public async Task<ChessDraw?> WaitForAnswer()
        {
            ChessDraw? answer = null;

            try
            {
                // wait for opponent's next draw
                _isWaiting = true;
                await Task.Run(() => _mutexWait.WaitOne(WAIT_TIMEOUT_MS));
                answer = _game.LastDraw;
            }
            catch (Exception /*ex*/)
            {
                _isWaiting = false;
                _mutexWait.ReleaseMutex();
            }

            return answer;
        }

        /// <summary>
        /// Try to submit the a chess draw if it is valid.
        /// </summary>
        /// <param name="draw">The draw to be submitted</param>
        /// <returns>a boolean indicating whether the submitted chess draw is valid</returns>
        public bool TrySubmitDraw(ChessDraw draw)
        {
            // try to apply the chess draw
            bool success = _game.ApplyDraw(draw, true);

            // release the mutex of the waiting 
            if (success && _isWaiting)
            {
                _isWaiting = false;
                _mutexWait.ReleaseMutex();
            }

            return success;
        }
        
        #endregion Methods
    }
}
