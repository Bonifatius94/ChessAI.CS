using System;
using System.Collections.Generic;
using System.Text;

namespace ChessAI.Lib
{
    public class ChessField
    {
        #region Members

        /// <summary>
        /// The position of the chess field.
        /// </summary>
        public ChessFieldPosition Position { get; set; }

        /// <summary>
        /// The piece that is currently onto the chess field (value is null if there is no chess piece).
        /// </summary>
        public ChessPiece Piece { get; set; } = null;

        #endregion Members
    }
}
