using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Chess.GameLib.Player
{
    /// <summary>
    /// Representing a generic chess player.
    /// </summary>
    public interface IChessPlayer
    {
        #region Members

        /// <summary>
        /// The side that the player is drawing.
        /// </summary>
        ChessColor Side { get; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Get the next draw from the chess player.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw? previousDraw);

        #endregion Methods
    }
}
