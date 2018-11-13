using Chess.Lib;
using System;

namespace Chess.AI
{
    /// <summary>
    /// An enumeration of chess game difficulties from easy to godlike. The enumeration values are assigned by numbers between 1 (easy) and 5 (godlike).
    /// </summary>
    public enum ChessDifficultyLevel
    {
        Easy    = 1,
        Medium  = 2,
        Hard    = 3,
        Extreme = 4,
        Godlike = 5
    }

    public interface IChessDrawHelper
    {
        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="activePlayer">the side that has to draw</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessPieceColor activePlayer, ChessDifficultyLevel level);
    }

    public class ChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="activePlayer">the side that has to draw</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessPieceColor activePlayer, ChessDifficultyLevel level)
        {
            throw new NotImplementedException("Please implement GetNextDraw() function!");
        }

        #endregion Methods
    }
}
