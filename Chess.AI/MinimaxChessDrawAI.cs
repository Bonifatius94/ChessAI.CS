using Chess.AI.Score;
using Chess.Lib;
using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chess.AI
{
    /// <summary>
    /// An implementation providing the optimal chess draw using the minimax algorithm with alpha / beta prune.
    /// </summary>
    public class MinimaxChessDrawAI : IChessDrawAI
    {
        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private MinimaxChessDrawAI() { }

        /// <summary>
        /// Get the singleton object reference.
        /// </summary>
        public static readonly IChessDrawAI Instance = new MinimaxChessDrawAI();

        /// <summary>
        /// The estimator used to evaluate chess boards.
        /// </summary>
        private static readonly IChessScoreEstimator _estimator = HeuristicChessScoreEstimator.Instance;

        #endregion Singleton

        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">The opponent's last draw (null on white-side's first draw)</param>
        /// <param name="searchDepth">The difficulty level</param>
        /// <returns>The 'best' possible chess draw</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? precedingEnemyDraw, int searchDepth)
        {
            // TODO: check whether the optimal draw was already found (-> database query)

            // make sure the game is not over yet
            if (precedingEnemyDraw != null && ChessDrawSimulator.Instance.GetCheckGameStatus(board, precedingEnemyDraw.Value).IsGameOver())
            {
                throw new ArgumentException("the game is already over.");
            }

            // retrieve the optimal draw by iterative deepening
            var bestDraw = iterativeDeepening(board, precedingEnemyDraw, searchDepth);
            
            return bestDraw;
        }

        /// <summary>
        /// Compute the score of the given draw according to minimax algorithm
        /// </summary>
        /// <param name="board">The chess game situation before the draw to be evaluated</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <param name="searchDepth">The minimax search depth (higher level = deeper search = better decisions)</param>
        /// <returns>a score that rates the quality of the given chess draw</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double RateDraw(IChessBoard board, ChessDraw draw, int searchDepth)
        {
            // simulate the given draw
            var simulatedBoard = board.ApplyDraw(draw);

            // use the minimax algorithm to rate the score of the given draw
            double score = negamax(simulatedBoard, draw, searchDepth - 1, double.MinValue, double.MaxValue, false) * -1;

            return score;
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

        //private ChessDraw? lookupDrawTable(ChessBoard board, ChessColor drawingSide)
        //{
        //    // TODO: test logic
        //    string gameSituationHash = board.ToHash();
        //    return _context.Draws.Where(x => x.GameSituationHash.Equals(gameSituationHash)).FirstOrDefault()?.OptimalDraw;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChessDraw[] generateDraws(IChessBoard board, ChessDraw? lastDraw = null)
        {
            ChessDraw[] draws = new ChessDraw[0];
            var drawingSide = lastDraw?.DrawingSide.Opponent() ?? ChessColor.White;

            if (board?.GetType() == typeof(ChessBoard))
            {
                var alliedPieces = board.GetPiecesOfColor(drawingSide);
                draws = alliedPieces.SelectMany(x => ChessDrawGenerator.Instance.GetDraws(board, x.Position, lastDraw, true)).ToArray();
            }
            else if (board?.GetType() == typeof(ChessBitboard))
            {
                draws = ((ChessBitboard)board).GetAllDraws(drawingSide, lastDraw, true);
            }

            return draws;
        }

        /// <summary>
        /// Determine the 'best' chess draw by iterative deepening and aspiration window technique (based on minimax game tree algorithm).
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="last">The previous chess draw made by the opponent</param>
        /// <param name="depth">The recursion depth for minimax algorithm</param>
        /// <returns>the 'best' chess draw</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChessDraw iterativeDeepening(IChessBoard board, ChessDraw? last, int depth)
        {
            // compute all possible draws
            var draws = generateDraws(board, last);

            // make sure there are chess draws to evaluate
            if (draws?.Count() <= 0) { throw new ArgumentException("the given player cannot draw. game is already over."); }

            // randomize draws on first run
            IEnumerable<ChessDrawScore> window = draws.Shuffle().Select(x => new ChessDrawScore() { Draw = x, Score = 0 }).ToArray();
            IEnumerable<ChessDrawScore> drawsScores = null;

            double maxScore = 0;
            int simDepth = 0;

            // increase search depth step by step
            do
            {
                // calculate the score for all draws inside the aspiration window
                drawsScores = getRatedDraws(board, window, simDepth).ToArray();

                // select a new reasonable aspiration window according to the additional information
                // moreover make sure that the best draw of the previous iteration is at least considered
                double newMaxScore = drawsScores.Select(x => x.Score).Max();
                var previousBestDraws = drawsScores.Where(x => x.Score == newMaxScore).ToArray();
                window = selectAspirationWindow(drawsScores).Concat(previousBestDraws).Distinct().ToArray();

                // update variables
                maxScore = newMaxScore;
                simDepth = (simDepth == depth) ? depth + 1 : ((simDepth < depth - 1) ? simDepth + 2 : depth);
            }
            // termination conditions: 
            //   1. max search depth reached
            //   2. only one good draw left
            //   3. draw found leading to victory
            while (simDepth <= depth && window.Count() > 1 && maxScore < double.MaxValue);
            
            // determine the draw with the highest score (the 'best' draw)
            var bestDraw = (window.Count() == 1) 
                ? window.Select(x => x.Draw).First() 
                : drawsScores.Where(x => x.Score == maxScore).Select(x => x.Draw).ChooseRandom();

            return bestDraw;
        }

        /// <summary>
        /// Select a reasonable aspiration window from the given chess draws and their scores according to statistic mathods.
        /// </summary>
        /// <param name="drawsScores">The chess draws and their previous scores.</param>
        /// <returns>a list of chess draws</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<ChessDrawScore> selectAspirationWindow(IEnumerable<ChessDrawScore> drawsScores)
        {
            // TODO: replace this by a real logging tool that uses different run configs for Debug/Release mode

            #if DEBUG
                // write aspiration window to console
                Console.WriteLine($"computed new draws with ratings:");
                drawsScores.ToList().ForEach(x => Console.WriteLine($" - { x.ToString() }"));
                Console.WriteLine();
            #endif

            // handle draws leading to win / loss of game because associated scores double.MaxValue / double.MinValue ruin the relevance of deviation / expectation

            // abort if there is at least one draw that wins the game (and return only one of the winning draws to save calculation time)
            if (drawsScores.Any(x => x.Score == double.MaxValue)) { return new List<ChessDrawScore>() { drawsScores.Where(x => x.Score == double.MaxValue).First() }; }

            // remove draws leading to defeat (and if there are only losing draws, just draw rather randomly because the game is lost anyways)
            if (drawsScores.All(x => x.Score == double.MinValue)) { return new List<ChessDrawScore>() { drawsScores.First() }; }
            else { drawsScores = drawsScores.Where(x => x.Score > double.MinValue); }

            // compute standard deviation of the scores and the average score
            double drawProbability = 1.0 / drawsScores.Count();
            var valueXProbTuples = drawsScores.Select(x => new Tuple<double, double>(x.Score, drawProbability)).ToArray();
            double stdDeviation = valueXProbTuples.StandardDeviation();
            double expectation = valueXProbTuples.Expectation();

            // init loop variables
            double deviationFactor = 0.2;
            ChessDrawScore[] window;

            do
            {
                // select a reasonable aspiration window (make sure the last 'best draw' is always included)
                double minScore = expectation - (stdDeviation * deviationFactor);
                window = drawsScores.Where(x => x.Score >= minScore/* && x.Score <= maxScore*/).ToArray();
                deviationFactor *= 2;

                // TODO: make sure that there is optimized performance at the cost of minimal error in draw selection
            }
            while (window.Count() <= 0);

            #if DEBUG
                // write aspiration window to console
                Console.WriteLine($"computed new aspiration window:");
                window.ToList().ForEach(x => Console.WriteLine($" - { x.ToString() }"));
                Console.WriteLine();
            #endif

            return window;
        }

        /// <summary>
        /// Retrieve a list of the given chess draws and their scores of the minimax algorithm of the given depth.
        /// </summary>
        /// <param name="board">The chess board before the chess draw is applied</param>
        /// <param name="drawScores">The chess draws to evaluate</param>
        /// <param name="depth">The depth of the minimax algorithm</param>
        /// <returns>a list of (chess draw, minimax score) tuples, ordered by the score (desc)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<ChessDrawScore> getRatedDraws(IChessBoard board, IEnumerable<ChessDrawScore> drawScores, int depth)
        {
            // make sure that the ally can draw
            if (drawScores?.Count() == 0) { throw new ArgumentException("draws list is empty"); }

            // get the best draw
            #if DEBUG
                drawScores = drawScores./*AsParallel().*/Select(x => new ChessDrawScore() { Draw = x.Draw, Score = RateDraw(board, x.Draw, depth) }).ToArray();
            #else
                drawScores = drawScores.AsParallel().Select(x => new ChessDrawScore() { Draw = x.Draw, Score = RateDraw(board, x.Draw, depth) }).ToArray();
            #endif

            // return the draws and their scores
            return drawScores;
        }

        private struct ChessDrawScore
        {
            #region Members

            public ChessDraw Draw { get; set; }
            public double Score { get; set; }
            //public ChessDraw?[] Cache { get; set; }

            #endregion Members

            #region Methods

            public override string ToString()
            {
                return $"{ Draw.ToString() } | { Math.Round(Score, 2) }";
            }

            #endregion Methods
        }

        /// <summary>
        /// An implementation of the minimax game tree algorithm. Returns the best score to be expected for the maximizing player.
        /// </summary>
        /// <param name="board">The chess board representing the game situation data</param>
        /// <param name="lastDraw">The last chess draw made by the opponent</param>
        /// <param name="depth">The recursion depth (is decremented step-by-step, so the recursion stops eventually when it has reached depth=0)</param>
        /// <param name="alpha">The lower bound of the already computed game scores</param>
        /// <param name="beta">The upper bound of the already computed game scores</param>
        /// <param name="isMaximizing">Indicates whether the side to draw is maximizing or minimizing</param>
        /// <returns>The best score to be expected for the maximizing player</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double negamax(IChessBoard board, ChessDraw? lastDraw, int depth, double alpha, double beta, bool isMaximizing = true)
        {
            var drawingSide = lastDraw?.DrawingSide.Opponent() ?? ChessColor.White;

            // recursion anchor: depth <= 0
            if (depth <= 0) { return _estimator.GetScore(board, drawingSide); }

            // init max score
            double maxScore = alpha;

            // compute possible draws
            var draws = generateDraws(board, lastDraw);

            // order draws by possible gain, so the alpha-beta prune can achieve fast cut-offs 
            // at high-level game tree nodes which ideally saves lots of computation effort
            var preorderedDraws = getPreorderedDrawsByPossibleGain(board, draws, drawingSide);

            // loop through all possible draws
            for (int i = 0; i < preorderedDraws.Length; i++)
            {
                // simulate draw
                var simDraw = preorderedDraws[i];
                var simBoard = board.ApplyDraw(simDraw);

                // start recursion
                double score = negamax(simBoard, simDraw, depth - 1, -beta, -maxScore, !isMaximizing) * -1;

                // update max score
                if (score > maxScore) { maxScore = score; }

                // check for beta cut-off
                if (maxScore >= beta) { break; }
            }

            return maxScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChessDraw[] getPreorderedDrawsByPossibleGain(IChessBoard board, IList<ChessDraw> draws, ChessColor drawingSide)
        {
            // compute the score
            var drawsXScoreTuples = new ChessDrawScore[draws.Count()];

            for (int i = 0; i < draws.Count(); i++)
            {
                // simulate draw
                var simDraw = draws[i];
                var simBoard = board.ApplyDraw(simDraw);

                // compute the new score of the resulting position
                double score = _estimator.GetScore(simBoard, drawingSide);

                // add the (draw, score) tuple to the list
                drawsXScoreTuples[i] = new ChessDrawScore() { Draw = simDraw, Score = score };
            }

            // TODO: check if this can be implemented more efficiently, e.g. with radix-sort
            return drawsXScoreTuples.OrderByDescending(x => x.Score).Select(x => x.Draw).ToArray();
        }

        ///// <summary>
        ///// An implementation of the minimax game tree algorithm. Returns the best score to be expected for the maximizing player.
        ///// </summary>
        ///// <param name="board">The chess board representing the game situation data</param>
        ///// <param name="precedingEnemyDraw">The last chess draw made by the opponent</param>
        ///// <param name="depth">The recursion depth (is decremented step-by-step, so the recursion stops eventually when it has reached depth=0)</param>
        ///// <param name="alpha">The lower bound of the already computed game scores</param>
        ///// <param name="beta">The upper bound of the already computed game scores</param>
        ///// <param name="isMaximizing">Indicates whether the side to draw is maximizing or minimizing</param>
        ///// <returns>The best score to be expected for the maximizing player</returns>
        //private double minimax(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth, double alpha, double beta, bool isMaximizing = true)
        //{
        //    // TODO: make sure that the minimax algo actually makes clever decisions instead of rather random ones

        //    double score;
        //    var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;

        //    // recursion anchor: depth <= 0
        //    if (depth <= 0)
        //    {
        //        score = ChessScoreGenerator.Instance.GetScore(board/*, drawingSide*/);
        //    }
        //    // recursion call: depth > 0
        //    else
        //    {
        //        // get all draws (child nodes) and shuffle them to randomize the algorithm
        //        var alliedPieces = board.GetPiecesOfColor(drawingSide);
        //        var draws = alliedPieces.SelectMany(x => ChessDrawGenerator.Instance.GetDraws(board, x.Position, precedingEnemyDraw, true)).Shuffle().ToArray();

        //        // TODO: remember 'good' draws, so the alpha/beta prune works best (-> some data needs to be cached, e.g. to draws of the last run)
        //        // TODO: enhance alpha/beta prune efficiency by an efficient way to order the computed draws (-> data needs to be cached)

        //        // init score with negative infinity when maximizing / positive infinity when minimizing
        //        score = isMaximizing ? double.MinValue : double.MaxValue;

        //        // simulate each draw and recurse (if player is checkmate => draw.count == 0 => recursion anchor)
        //        for (int i = 0; i < draws.Length; i++)
        //        {
        //            // simulate chess draw
        //            var simDraw = draws[i];
        //            var simBoard = new ChessBoard(board.Pieces);
        //            simBoard.ApplyDraw(simDraw);

        //            // update the minimax score of recursions
        //            double minimaxScore = minimax(simBoard, simDraw, depth - 1, alpha, beta, !isMaximizing);
        //            score = isMaximizing ? Math.Max(score, minimaxScore) : Math.Min(score, minimaxScore);

        //            // update alpha / beta
        //            if (isMaximizing) { alpha = Math.Max(score, alpha); }
        //            else              { beta  = Math.Min(score, beta);  }

        //            // cut-off when alpha and beta overlap
        //            if (alpha >= beta) { break; }
        //        }
        //    }

        //    return score;
        //}

        #endregion Methods
    }
}
