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
        double GetScore(ChessBoard board, ChessColor sideToDraw);

        #endregion Methods
    }
}
