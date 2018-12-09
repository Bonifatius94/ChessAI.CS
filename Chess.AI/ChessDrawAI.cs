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
        /// <param name="board">The chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">The opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
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
        /// <param name="board">The chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">The opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level)
        {
            // TODO: check whether the optimal draw was already found (-> database query)

            // make sure the game is not over yet
            if (precedingEnemyDraw != null && new ChessDrawSimulator().GetCheckGameStatus(board, precedingEnemyDraw.Value).IsGameOver())
            {
                throw new ArgumentException("the game is already over.");
            }

            // retrieve the optimal draw by iterative deepening
            int depth = ((int)level) * 2;
            var bestDraw = iterativeDeepening(board, precedingEnemyDraw, depth);
            
            return bestDraw;
        }

        /*
         * AI algorithm techniques:
         * ========================
         * 
         * minimax: https://en.wikipedia.org/wiki/Minimax
         * alpha-beta prune: https://www.chessprogramming.org/Alpha-Beta
         * iterative deepening: https://www.chessprogramming.org/Iterative_Deepening
         * aspiration windows: https://www.chessprogramming.org/Aspiration_Windows
         * transposition table: https://www.chessprogramming.org/Transposition_Table
         */

        private ChessDraw? lookupDrawTable(ChessBoard board, ChessColor drawingSide)
        {
            // TODO: test logic
            string gameSituationHash = board.ToHash();
            return _context.Draws.Where(x => x.GameSituationHash.Equals(gameSituationHash)).FirstOrDefault()?.OptimalDraw;
        }

        /// <summary>
        /// Determine the 'best' chess draw by iterative deepening and aspiration window technique (based on minimax game tree algorithm).
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="precedingEnemyDraw">The previous chess draw made by the opponent</param>
        /// <param name="depth">The recursion depth for minimax algorithm</param>
        /// <returns>the 'best' chess draw</returns>
        private ChessDraw iterativeDeepening(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth)
        {
            // compute all possible draws
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
            var alliedPieces = board.GetPiecesOfColor(drawingSide);
            var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true)).ToArray();

            // make sure there are chess draws to evaluate
            if (draws?.Count() <= 0) { throw new ArgumentException("the given player cannot draw. game is already over."); }

            // randomize draws on first run
            IEnumerable<ChessDraw> window = draws.Shuffle().ToArray();
            IEnumerable<Tuple<ChessDraw, double>> drawsXScores = null;

            double maxScore = 0;
            int simDepth = 0;

            // increase search depth step by step, trying to go the optimal way first, so alpha/beta prune works best
            do
            {
                // calculate the score for all draws inside the aspiration window
                drawsXScores = getDrawsWithScores(board, window, simDepth);

                // select a new reasonable aspiration window according to the additional information
                double newMaxScore = drawsXScores.Select(x => x.Item2).Max();
                window = selectAspirationWindow(drawsXScores, Math.Max(maxScore, newMaxScore));
                
                // update variables
                maxScore = newMaxScore;
                simDepth += 2;
            }
            while (simDepth <= depth && window.Count() > 1);
            
            // determine the draw with the highest score (the 'best' draw)
            var bestDraw = (window.Count() == 1) ? window.First() : drawsXScores.OrderByDescending(x => x.Item2).Select(x => x.Item1).First();

            return bestDraw;
        }

        /// <summary>
        /// Select a reasonable aspiration window from the given chess draws and their scores according to statistic mathods.
        /// </summary>
        /// <param name="drawsXScores">The chess draws and their scores </param>
        /// <param name="maxScore">The maximum score to be included inside the aspiration window</param>
        /// <returns>a list of chess draws</returns>
        private IEnumerable<ChessDraw> selectAspirationWindow(IEnumerable<Tuple<ChessDraw, double>> drawsXScores, double maxScore)
        {
            ChessDraw[] window;

            double stdDeviation = drawsXScores.Select(x => x.Item2).StandardDeviation();
            double avgScore = drawsXScores.Select(x => x.Item2).Average();
            double deviationFactor = 0.5;
            
            do
            {
                // select a reasonable aspiration window (make sure the last 'best draw' is always included)
                window = drawsXScores.Where(x => x.Item2 >= (avgScore - (stdDeviation * deviationFactor)) && x.Item2 <= maxScore).Select(x => x.Item1).ToArray();
                deviationFactor *= 2;
            }
            while (window.Count() <= 0);

            return window;
        }
        
        /// <summary>
        /// Retrieve a list of the given chess draws and their scores of the minimax algorithm of the given depth.
        /// </summary>
        /// <param name="board">The chess board before the chess draw is applied</param>
        /// <param name="draws">The chess draws to evaluate</param>
        /// <param name="depth">The depth of the minimax algorithm</param>
        /// <returns>a list of (chess draw, minimax score) tuples, ordered by the score (desc)</returns>
        private IEnumerable<Tuple<ChessDraw, double>> getDrawsWithScores(ChessBoard board, IEnumerable<ChessDraw> draws, int depth)
        {
            // make sure that the ally can draw
            if (draws.Count() == 0) { throw new ArgumentException("draws list is empty"); }
            
            // get the best draw
            var drawsWithScores = draws.AsParallel().Select(simDraw =>
            {
                // simulate draw
                var simBoard = new ChessBoard(board.Pieces);
                simBoard.ApplyDraw(simDraw);
                
                // compute minimax score (needs to be started as minimizing player)
                double simScore = (depth <= 0)
                    ? new ChessScoreHelper().GetScore(simBoard, simDraw.DrawingSide)                    // no minimax algo required. just get the score after applying the draw.
                    : minimax(simBoard, simDraw, depth - 1, double.MinValue, double.MaxValue, false);   // use minimax algo to determine the score

                // return the simulated draw and its score
                return new Tuple<ChessDraw, double>(simDraw, simScore);
            });

            // return the draws and their scores
            return drawsWithScores.ToArray();
        }

        /// <summary>
        /// An implementation of the minimax game tree algorithm. Returns the best score to be expected for the maximizing player.
        /// </summary>
        /// <param name="board">The chess board representing the game situation data</param>
        /// <param name="precedingEnemyDraw">The last chess draw made by the opponent</param>
        /// <param name="depth">The recursion depth (is decremented step-by-step, so the recursion stops eventually when it has reached depth=0)</param>
        /// <param name="alpha">The lower bound of the already computed game scores</param>
        /// <param name="beta">The upper bound of the already computed game scores</param>
        /// <param name="isMaximizing">Indicates whether the side to draw is maximizing or minimizing</param>
        /// <returns>The best score to be expected for the maximizing player</returns>
        private double minimax(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth, double alpha, double beta, bool isMaximizing = true)
        {
            double score;
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;

            // recursion anchor: depth == 0
            if (depth == 0)
            {
                score = new ChessScoreHelper().GetScore(board, drawingSide);
            }
            // recursion call: depth > 0
            else
            {
                // get all draws (child nodes) and shuffle them to randomize the algorithm
                var alliedPieces = board.GetPiecesOfColor(drawingSide);
                var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true)).Shuffle().ToArray();

                // init score with negative infinity when maximizing / positive infinity when minimizing
                score = isMaximizing ? double.MinValue : double.MaxValue;

                // simulate each draw and recurse (if player is checkmate => draw.count == 0 => recursion anchor)
                for (int i = 0; i < draws.Length; i++)
                {
                    // simulate chess draw
                    var simDraw = draws[i];
                    var simBoard = new ChessBoard(board.Pieces);
                    simBoard.ApplyDraw(simDraw);

                    // update the minimax score of recursions
                    double minimaxScore = minimax(simBoard, simDraw, depth - 1, alpha, beta, !isMaximizing);
                    score = isMaximizing ? Math.Max(score, minimaxScore) : Math.Min(score, minimaxScore);

                    // update alpha / beta
                    if (isMaximizing) { alpha = Math.Max(score, alpha); }
                    else { beta = Math.Min(score, beta); }

                    // cut-off when alpha and beta overlap
                    if (alpha >= beta) { break; }
                }
            }

            return score;
        }

        // TODO: implement minimax algorithm in a way that the best draw and its following draws can also be retrieved (not only score)
        // => this would allow some caching optimizations during the next minimax procedure
        
        #endregion Methods
    }

    /// <summary>
    /// An extension providing standard deviation computation functionality.
    /// </summary>
    public static class StandardDeviationEx
    {
        #region Methods

        /// <summary>
        /// Compute the standard deviation of the given double values.
        /// </summary>
        /// <param name="values">The list of double values to be evaluated</param>
        /// <returns>The standard deviation of the given double values</returns>
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            double variance = values.Select(x => Math.Pow((x - avg), 2)).Sum() / values.Count();
            return Math.Sqrt(variance);
        }

        #endregion Methods
    }
}
