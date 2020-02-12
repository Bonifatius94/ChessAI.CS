using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.AI
{
    /// <summary>
    /// An enumeration of chess game difficulties from easy to godlike. The enumeration values are assigned by numbers between 1 (easy) and 5 (godlike).
    /// </summary>
    public enum ChessDifficultyLevel
    {
        /// <summary>
        /// Representing a completely stupid playstyle (random draws).
        /// </summary>
        Stupid = 0,

        /// <summary>
        /// Representing a somewhat easy playstyle (taking one future draw in consideration).
        /// </summary>
        Easy = 1,

        /// <summary>
        /// Representing a beginner playstyle (taking two future draws in consideration).
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Representing an experienced playstyle (taking three future draws in consideration).
        /// </summary>
        Hard = 3,

        /// <summary>
        /// Representing a very experienced playstyle (taking four future draws in consideration).
        /// </summary>
        Extreme = 4,

        /// <summary>
        /// Representing a master's playstyle (taking five future draws in consideration).
        /// </summary>
        Godlike = 5
    }

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
        /// <param name="level">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level);

        /// <summary>
        /// Compute the score of the given draw using the rating technique of the chosen implementation.
        /// </summary>
        /// <param name="board">The chess game situation before the draw to be evaluated</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <param name="level">The minimax search depth (higher level = deeper search = better decisions)</param>
        /// <returns>a score that rates the quality of the given chess draw</returns>
        double RateDraw(ChessBoard board, ChessDraw draw, ChessDifficultyLevel level);

        // TODO: rework! why specifying difficulty here???
    }
}
