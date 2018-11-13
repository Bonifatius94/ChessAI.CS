using System;
using System.Collections.Generic;
using System.Text;

namespace ChessAI.Lib
{
    public class ChessDraw
    {
        #region Members

        /// <summary>
        /// The chess piece to be moved.
        /// </summary>
        public ChessPiece Piece { get; set; }

        /// <summary>
        /// The old position of the chess piece to be moved.
        /// </summary>
        public ChessFieldPosition OldPosition { get; set; }

        /// <summary>
        /// The new position of the chess piece to be moved.
        /// </summary>
        public ChessFieldPosition NewPosition { get; set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Compute whether the draw is valid or not.
        /// </summary>
        /// <param name="board">the chess board where the draw should be applied to</param>
        /// <returns>boolean whether the draw is valid</returns>
        public bool IsValid(ChessBoard board)
        {
            throw new NotImplementedException("Please implement draw validation logic!");
        }

        #endregion Methods
    }
}
