using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.DataTools;
using Chess.DataTools.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
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

        #endregion Singleton

        #region Members

        /// <summary>
        /// The cache used for getting good draws.
        /// </summary>
        private static readonly WinRateDataContext _cache = new WinRateDataContext(Path.Combine("Data", "win_rates.db"));

        #endregion Members

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="precedingEnemyDraw"></param>
        /// <param name="searchDepth"></param>
        /// <returns></returns>
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? precedingEnemyDraw, int searchDepth)
        {
            // always use the cache if possible, otherwise compute the best draw with minimax algorithm
            return _cache.GetBestDraw(board, precedingEnemyDraw) ?? MinimaxChessDrawAI.Instance.GetNextDraw(board, precedingEnemyDraw, searchDepth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="draw"></param>
        /// <param name="searchDepth"></param>
        /// <returns></returns>
        public double RateDraw(IChessBoard board, ChessDraw draw, int searchDepth)
        {
            return MinimaxChessDrawAI.Instance.RateDraw(board, draw, searchDepth);
        }

        #endregion Methods
    }
}
