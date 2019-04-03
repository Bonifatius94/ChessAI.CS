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
            var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, precedingEnemyDraw, false));

            // calculate the predictions according to the difficulty level
            var drawsWithPrediction = possibleDraws.Select(draw => new { Draw = draw, Score = predictScore(board, draw, level) });

            // get the draw with the highest prediction (best draw)
            double maxScore = drawsWithPrediction.Select(x => x.Score).Max();
            var bestDraw = drawsWithPrediction.Where(x => x.Score == maxScore).First().Draw;

            return bestDraw;
        }

        private double predictScore(ChessBoard board, ChessDraw draw, ChessDifficultyLevel level)
        {
            // TODO: implement prediction logic
            throw new NotImplementedException();
        }
    }
}
