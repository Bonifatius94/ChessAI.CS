using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.AI
{
    /// <summary>
    /// 
    /// </summary>
    public class CachedChessDrawAI : IChessDrawAI
    {
        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private CachedChessDrawAI() { }

        /// <summary>
        /// Get the singleton object reference.
        /// </summary>
        public static readonly IChessDrawAI Instance = new CachedChessDrawAI();

        #region Members

        /// <summary>
        /// The cache used for getting good draws.
        /// </summary>
        private static readonly IDictionary<ChessBoard, IEnumerable<ChessDraw>> _cache = initCache();

        #endregion Members

        #region Methods

        private static IDictionary<ChessBoard, IEnumerable<ChessDraw>> initCache(double minWinPercentage = 0.4)
        {
            // TODO: make the cache more efficient

            var winRates = new WinRateInfoSerializer().Deserialize("win_percentages_of_draws.xml");

            // init draws cache
            var cache = new Dictionary<ChessBoard, IEnumerable<ChessDraw>>();

            winRates.Where(x => x.WinRate >= minWinPercentage).GroupBy(x => x.Situation.Item1).AsParallel().Select(group => {

                var board = (ChessBoard)group.Key.Clone();
                double maxWinRate = group.Max(x => x.WinRate);
                var drawsWithMaxWinRate = group.Where(x => x.WinRate >= maxWinRate).Select(x => x.Situation.Item2).ToList();

                return new Tuple<ChessBoard, IEnumerable<ChessDraw>>(board, drawsWithMaxWinRate);
            })
            .ToList()
            .ForEach(x => cache.Add(x.Item1, x.Item2));

            GC.Collect();
            return cache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="precedingEnemyDraw"></param>
        /// <param name="searchDepth"></param>
        /// <returns></returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, int searchDepth)
        {
            ChessDraw draw;

            // always use the cache if possible, otherwise compute the best draw with minimax algorithm
            if (_cache.ContainsKey(board)) { draw = _cache[board].ChooseRandom(); }
            else { draw = MinimaxChessDrawAI.Instance.GetNextDraw(board, precedingEnemyDraw, searchDepth); }

            return draw;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="draw"></param>
        /// <param name="searchDepth"></param>
        /// <returns></returns>
        public double RateDraw(ChessBoard board, ChessDraw draw, int searchDepth)
        {
            return MinimaxChessDrawAI.Instance.RateDraw(board, draw, searchDepth);
        }

        #endregion Methods
    }
}
