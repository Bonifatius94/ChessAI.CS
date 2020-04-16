using Chess.AI;
using Chess.Lib;
using Chess.DataTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.CLI.Player
{
    /// <summary>
    /// Representing an artificial chess player that uses AI techniques to determine his next draw.
    /// </summary>
    public class ArtificialChessPlayer : IChessPlayer
    {
        #region Constructor

        /// <summary>
        /// Initialize a new artificial chess player of the given level.
        /// </summary>
        /// <param name="level">The difficulty level of the new chess player.</param>
        public ArtificialChessPlayer(int level)
        {
            _level = level;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The level of the artificial player.
        /// </summary>
        private int _level;

        #endregion Members

        #region Methods

        /// <summary>
        /// Use AI techniques to determine the next draw.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? previousDraw)
        {
            return CachedChessDrawAI.Instance.GetNextDraw(board, previousDraw, _level);
        }

        #endregion Methods
    }
}
