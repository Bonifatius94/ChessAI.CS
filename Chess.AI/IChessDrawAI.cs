using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.AI
{
    /// <summary>
    /// An interface providing operations for optimal chess draw computation.
    /// </summary>
    public interface IChessDrawAI
    {
        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">The opponent's last draw (null on white-side's first draw)</param>
        /// <param name="searchDepth">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
        ChessDraw GetNextDraw(IChessBoard board, ChessDraw? precedingEnemyDraw, int searchDepth);

        /// <summary>
        /// Compute the score of the given draw using the rating technique of the chosen implementation.
        /// </summary>
        /// <param name="board">The chess game situation before the draw to be evaluated</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <param name="searchDepth">The minimax search depth (higher level = deeper search = better decisions, but more computation time)</param>
        /// <returns>a score that rates the quality of the given chess draw</returns>
        double RateDraw(IChessBoard board, ChessDraw draw, int searchDepth);

        // TODO: rework! why specifying difficulty here???
    }

    ///// <summary>
    ///// Extension class for ChessDifficultyLevel.
    ///// </summary>
    //public static class ChessDifficultyLevelEx
    //{
    //    #region Methods

    //    /// <summary>
    //    /// Transform the given difficulty level into the corresponding search depth.
    //    /// </summary>
    //    /// <param name="difficulty">The difficulty level to transform.</param>
    //    /// <returns>the corresponding search depth</returns>
    //    public static int ToSearchDepth(this ChessDifficultyLevel difficulty)
    //    {
    //        return ((int)difficulty) * 2 + 1;
    //    }

    //    #endregion Methods
    //}
}
