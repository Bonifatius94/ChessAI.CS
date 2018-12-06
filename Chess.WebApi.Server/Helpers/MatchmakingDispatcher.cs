using Chess.WebApi.Server.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.WebApi.Server.Helpers
{
    /// <summary>
    /// Dispatch players looking to start a new game and store match data.
    /// </summary>
    public class MatchmakingDispatcher
    {
        #region Members

        // the game id counter
        private static int _maxId = 0;

        /// <summary>
        /// A dictionary of all matches that have been played.
        /// </summary>
        public static Dictionary<int, ChessMatchSession> Matches { get; } = new Dictionary<int, ChessMatchSession>();

        // some multi-threading synchronization tools
        private static Semaphore _semaphoreMatchmaking = new Semaphore(0, 2);
        private static Mutex _mutexGameCreation = new Mutex();
        private static Mutex _mutexIdIncrement = new Mutex();
        private static Barrier _barrierGameCreationSync = new Barrier(2);

        #endregion Members

        #region Methods

        /// <summary>
        /// Try to find a game.
        /// </summary>
        /// <returns>required data for starting the game</returns>
        public StartGameResponse FindGame()
        {
            StartGameResponse ret;

            // make sure only 2 players enter the game creation
            _semaphoreMatchmaking.WaitOne();

            // determine the game id (same id for both players)
            int gameId = _maxId + 1;

            // wait until both players have set the game id (this makes sure they have the same id)
            _barrierGameCreationSync.SignalAndWait();

            // make sure only the first of the two players creates the game
            _mutexGameCreation.WaitOne();
            bool isFirst = !Matches.ContainsKey(gameId);

            if (isFirst)
            {
                // first player's part: create a new game 
                Matches.Add(gameId, new ChessMatchSession(gameId));
                _maxId++;

                // now that the game is created, let the second player enter the section
                _mutexGameCreation.ReleaseMutex();
            }
            else
            {
                // let the next 2 players enter the game creation section
                _mutexGameCreation.ReleaseMutex();
                _semaphoreMatchmaking.Release(2);
            }

            // return the required response data
            ret = new StartGameResponse() { GameId = gameId, IsFirst = isFirst };
            return ret;
        }

        #endregion Methods
    }
}
