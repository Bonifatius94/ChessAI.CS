using Chess.AI;
using Chess.Lib;
using Chess.DataTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Chess.GameLib.Player
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
        public ArtificialChessPlayer(ChessColor side, ChessDifficultyLevel level)
        {
            Side = side;
            _level = level;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The level of the artificial player.
        /// </summary>
        private ChessDifficultyLevel _level;

        /// <summary>
        /// The side the the player is drawing.
        /// </summary>
        public ChessColor Side { get; private set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Use AI techniques to determine the next draw.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? previousDraw)
        {
            // TODO: implement difficulty to search depth translation logic
            return CachedChessDrawAI.Instance.GetNextDraw(board, previousDraw, (int)_level);
        }

        #endregion Methods
    }
}
