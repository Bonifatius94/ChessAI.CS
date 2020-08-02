/*
 * MIT License
 *
 * Copyright(c) 2020 Marco Tröster
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
    /// An implementation providing the optimal chess draw using the minimax algorithm with alpha / beta prune, 
    /// which is supported by pre-calculated draws from a cache database.
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
