using Chess.AI.Data;
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
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level);
    }

    /// <summary>
    /// An implementation providing the optimal chess draw using the minimax algorithm with alpha / beta prune.
    /// </summary>
    public class ChessDrawAI : IChessDrawAI
    {
        #region Members
        
        private static readonly ChessAIContext _context = new ChessAIContext();

        #endregion Members

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
            // TODO: check whether the optimal draw was already found (-> database query)
            
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
            var drawsWithScores = draws.Shuffle().AsParallel().Select(draw => new Tuple<ChessDraw, double>(draw, minimax(board, draw, depth, alpha, beta)));
            double maxScore = drawsWithScores.Max(x => x.Item2);
            var bestDraw = drawsWithScores.Where(x => x.Item2 == maxScore).First().Item1;
            
            return bestDraw;
        }

        /*
         * techniques to implement:
         * =======================
         * 
         * alpha-beta prune: https://www.chessprogramming.org/Alpha-Beta
         * iterative deepening: https://www.chessprogramming.org/Iterative_Deepening
         * transposition table: https://www.chessprogramming.org/Transposition_Table
         */
        
        private double minimax(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth, double alpha, double beta, bool maximize = true)
        {
            // TODO: optimize this code, so it runs really fast
            
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
            double score = new ChessScoreHelper().GetScore(board, drawingSide);
            
            if (depth > 0)
            {
                // TODO: implement early cut-off when the score is too bad
                
                // get all draws (child nodes)
                var alliedPieces = board.GetPiecesOfColor(drawingSide);
                var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true));

                // calculate the scores for each draw
                var maximizingSide = maximize ? drawingSide : drawingSide.Opponent();
                var drawsXScores = draws.Select(simDraw =>
                {
                    // simulate the draw
                    var simBoard = (ChessBoard)board.Clone();
                    simBoard.ApplyDraw(simDraw);

                    // calculate the score and return it
                    double simScore = new ChessScoreHelper().GetScore(simBoard, maximizingSide);
                    return new ChessDrawSimulationResult() { Draw = simDraw, ScoreBefore = score, ScoreAfter = simScore, SimBoard = simBoard };
                }
                // put most promising draws on top, so the alpha-beta prune works best
                ).OrderByDescending(x => x.ScoreAfter).ToArray();

                // init score with negative infinity when maximizing / positive infinity when minimizing
                score = maximize ? double.MinValue : double.MaxValue;

                for (int i = 0; i < drawsXScores.Length; i++)
                {
                    var drawMetadata = drawsXScores[i];

                    // only enter recursion if the score stays at least somehow neutral
                    if (maximize || drawMetadata.Diff >= -1)
                    {
                        // update the minimax score of recursions
                        double minimaxScore = minimax(drawMetadata.SimBoard, drawMetadata.Draw, depth - 1, alpha, beta, !maximize);
                        score = maximize ? Math.Max(score, minimaxScore) : Math.Min(score, minimaxScore);

                        // update alpha / beta
                        if (maximize) { alpha = Math.Max(score, alpha); }
                        else          { beta  = Math.Min(score, beta);  }

                        // cut-off when alpha and beta overlap
                        if (alpha >= beta) { break; }
                    }
                }
            }

            return score;
        }

        private class ChessDrawSimulationResult
        {
            #region Members

            public ChessDraw Draw { get; set; }
            public ChessBoard SimBoard { get; set; }
            public double ScoreBefore { get; set; }
            public double ScoreAfter { get; set; }

            public double Diff { get { return ScoreAfter - ScoreBefore; } }

            #endregion Members
        }
        
        #endregion Methods
    }
}
