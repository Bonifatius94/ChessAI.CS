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
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.AI.Score
{
    /// <summary>
    /// An interface for chess score estimators to evaluate game situations.
    /// </summary>
    public interface IChessScoreEstimator
    {
        #region Methods

        /// <summary>
        /// Computes the estimated score of the given game situation from the drawing side's view.
        /// The result value should be higher the better the drawing player's situation is (and vice versa).
        /// </summary>
        /// <param name="board">The chess board representing the situation to be evaluated.</param>
        /// <param name="sideToDraw">The drawing side.</param>
        /// <returns>the estimated score of the given game situation</returns>
        double GetScore(IChessBoard board, ChessColor sideToDraw);

        #endregion Methods
    }
}
