using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.AI
{
    /// <summary>
    /// An enumeration of chess game difficulties from easy to godlike. The enumeration values are assigned by numbers between 1 (easy) and 5 (godlike).
    /// </summary>
    public enum ChessDifficultyLevel
    {
        Stupid  = 0,
        Easy    = 1,
        Medium  = 2,
        Hard    = 3,
        Extreme = 4,
        Godlike = 5
    }

    public interface IChessDrawAI
    {
        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level);
    }

    public class ChessDrawAI : IChessDrawAI
    {
        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level)
        {
            // prepare draws to be analyzed
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
            var alliedPieces = board.GetPiecesOfColor(drawingSide);
            var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true));

            // make sure that the ally can draw
            if (draws.Count() == 0) { throw new ArgumentException("the given side cannot draw."); }

            // prepare minimax depth / alpha / beta
            int depth = ((int)level) * 2;
            double alpha = double.MinValue;
            double beta = double.MaxValue;

            // get the best draw
            var drawsWithScores = draws.AsParallel().Select(draw => new Tuple<ChessDraw, double>(draw, minimax(board, draw, depth, alpha, beta)));
            double maxScore = drawsWithScores.Max(x => x.Item2);
            var bestDraw = drawsWithScores.Where(x => x.Item2 == maxScore).First().Item1;
            
            return bestDraw;
        }
        
        private double minimax(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth, double alpha, double beta, bool maximize = true)
        {
            // TODO: optimize this code, so it runs really fast
            
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
            double score = new ChessScoreHelper().GetScore(board, drawingSide);
            
            if (depth > 0)
            {
                // TODO: implement cut-off when the score is too bad

                // init score with negative infinity when maximizing / positive infinity when minimizing
                score = maximize ? double.MinValue : double.MaxValue;

                // get all draws (child nodes)
                var alliedPieces = board.GetPiecesOfColor(drawingSide);
                var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true)).Shuffle().ToArray();
                
                for (int i = 0; i < draws.Length; i++)
                {
                    // simulate the draw
                    var simDraw = draws[i];
                    var simBoard = (ChessBoard)board.Clone();
                    simBoard.ApplyDraw(simDraw);

                    // calculate the score of the simulated draw
                    var maximizingSide = maximize ? drawingSide : drawingSide.Opponent();
                    double scoreOfDraw = new ChessScoreHelper().GetScore(simBoard, maximizingSide);

                    // only enter recursion if the score stays at least somehow neutral
                    if (score - scoreOfDraw >= -3)
                    {
                        // update the minimax score of recursions
                        double minimaxScore = minimax(simBoard, simDraw, depth - 1, alpha, beta, !maximize);
                        score = maximize ? Math.Max(score, minimaxScore) : Math.Min(score, minimaxScore);

                        // update alpha / beta
                        if (maximize) { alpha = Math.Max(score, alpha); }
                        else          { beta  = Math.Min(score, beta);  }

                        // cut-off when alpha and beta overlap
                        if (alpha >= beta) { break; }
                    }
                    else if (!maximize)
                    {

                    }
                }
            }

            return score;
        }
        
        #endregion Methods
    }
}
