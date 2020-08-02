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

using Chess.AI;
using Chess.Lib;
using Chess.DataTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Chess.GameLib.Player
{
    /// <summary>
    /// Representing an artificial chess player that uses AI techniques to determine his next draw.
    /// </summary>
    public class ArtificialChessPlayer : IChessPlayer
    {
        #region Constructor

        /// <summary>
        /// Initialize a new artificial chess player of the given level.
        /// </summary>
        /// <param name="level">The difficulty level of the new chess player.</param>
        public ArtificialChessPlayer(ChessColor side, ChessDifficultyLevel level)
        {
            Side = side;
            _level = level;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The level of the artificial player.
        /// </summary>
        private ChessDifficultyLevel _level;

        /// <summary>
        /// The side the the player is drawing.
        /// </summary>
        public ChessColor Side { get; private set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Use AI techniques to determine the next draw.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? previousDraw)
        {
            return CachedChessDrawAI.Instance.GetNextDraw(board, previousDraw, (int)_level);
        }

        #endregion Methods
    }
}
