using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chess.Lib;

namespace Chess.AI
{
    /// <summary>
    /// An implementation providing the optimal chess draw using score prediction functions measured by deep learning regression.
    /// </summary>
    public class DeepLearningChessDrawAI : IChessDrawAI
    {
        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private DeepLearningChessDrawAI() { }

        /// <summary>
        /// Get of singleton object reference.
        /// </summary>
        public static readonly IChessDrawAI Instance = new DeepLearningChessDrawAI();

        #endregion Singleton

        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">The opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level)
        {
            // get all possible allied draws
            var alliedPieces = board.GetPiecesOfColor(precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White);
            var possibleDraws = alliedPieces.SelectMany(piece => ChessDrawGenerator.Instance.GetDraws(board, piece.Position, precedingEnemyDraw, false));

            // calculate the predictions according to the difficulty level
            var drawsWithPrediction = possibleDraws.Select(draw => new { Draw = draw, Score = RateDraw(board, draw, level) });

            // get the draw with the highest prediction (best draw)
            double maxScore = drawsWithPrediction.Select(x => x.Score).Max();
            var bestDraw = drawsWithPrediction.Where(x => x.Score == maxScore).First().Draw;

            return bestDraw;
        }

        /// <summary>
        /// Predict the score of the given draw using the deep learning technology.
        /// </summary>
        /// <param name="board">The chess game situation before the draw to be evaluated</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <param name="level">The minimax search depth (higher level = deeper search = better decisions)</param>
        /// <returns>a score that rates the quality of the given chess draw</returns>
        public double RateDraw(ChessBoard board, ChessDraw draw, ChessDifficultyLevel level)
        {
            // TODO: implement AI rating function
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
