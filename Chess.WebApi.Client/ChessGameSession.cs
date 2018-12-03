using Chess.Lib;
using Chess.WebApi.Server.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chess.WebApi.Client
{
    /// <summary>
    /// Helps to play chess against other players by initial matchmaking and submitting / retrieving draws.
    /// </summary>
    public class ChessGameSession
    {
        #region Constructor

        /// <summary>
        /// Create a new chess game session with the given server.
        /// </summary>
        /// <param name="baseAddress">the hostname of the server (IP or DNS name) and optional a port (e.g. 'www.someserver.com:77777')</param>
        public ChessGameSession(string baseAddress)
        {
            httpHelper = new ChessHttpHelper(baseAddress);
        }

        #endregion Constructor

        #region Members

        private ChessHttpHelper httpHelper;
        private StartGameResponse _gameInfo;

        /// <summary>
        /// The chess game instance containing all the gameplay information.
        /// </summary>
        public ChessGame Game { get; } = new ChessGame();

        /// <summary>
        /// Indicates whether the local player has to draw.
        /// </summary>
        public bool HasToDraw { get { return Game.SideToDraw == (_gameInfo.IsFirst ? ChessColor.White : ChessColor.Black); } }

        #endregion Members

        #region Methods

        /// <summary>
        /// Try to find a chess match. Init the session accordingly if successful. Otherwise retry up to 3 times.
        /// </summary>
        /// <param name="timeout">the timeout in seconds</param>
        /// <returns>a boolean indicating whether the matchmaking was successful</returns>
        public bool FindMatch(int timeout = 300)
        {
            bool ret = false;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var task = httpHelper.TryStartNewGame();

                    if (task.Wait(timeout * 1000))
                    {
                        _gameInfo = task.Result;
                        ret = true;
                        break;
                    }
                }
                catch (Exception) { /* nothing to do here ... */ }
            }

            return ret;
        }
        
        /// <summary>
        /// Try to submit the next chess draw.
        /// </summary>
        /// <param name="draw">the chess draw to be submitted</param>
        /// <param name="timeout">the timeout in seconds</param>
        /// <returns>a boolean indicating whether the submission was successful</returns>
        public bool SubmitDraw(ChessDraw draw, int timeout = 300)
        {
            bool success = false;
            
            // try to submit out chess draw
            try
            {
                var submitDrawTask = httpHelper.TrySubmitDraw(_gameInfo.GameId, draw);
                success = (submitDrawTask.Wait(timeout * 1000) && submitDrawTask.Result);

                // apply the draw to the chess game instance if transmission was successfuls
                if (success) { Game.ApplyDraw(draw); }
            }
            catch (Exception) { /* nothing to do here ... */ }

            return success;
        }

        /// <summary>
        /// Try to get the opponent's next draw.
        /// </summary>
        /// <param name="timeout">the timeout in seconds</param>
        /// <returns>the opponent's chess draw (if successful) or null (if not successful)</returns>
        public ChessDraw? GetOpponentDraw(int timeout = 300)
        {
            ChessDraw? draw = null;

            try
            {
                var getDrawTask = httpHelper.TryGetOpponentDraw(_gameInfo.GameId);

                if (getDrawTask.Wait(timeout * 1000))
                {
                    draw = getDrawTask.Result;

                    // apply the draw to the chess game instance if transmission was successful
                    if (draw != null) { Game.ApplyDraw(draw.Value); }
                }
            }
            catch (Exception) { /* nothing to do here */ }

            return draw;
        }

        #endregion Methods
    }
}
