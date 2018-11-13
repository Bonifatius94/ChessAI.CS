using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess field on a chess board.
    /// </summary>
    public class ChessField
    {
        #region Members

        /// <summary>
        /// The position of the chess field.
        /// </summary>
        public ChessFieldPosition Position { get; set; }

        private ChessPiece _piece = null;
        /// <summary>
        /// The piece that currently captures the chess field (value is null if there is no chess piece).
        /// </summary>
        public ChessPiece Piece
        {
            get { return _piece; }
            set
            {
                // apply the new piece and update the position of the piece
                _piece = value;
                if (_piece != null) { _piece.Position = Position; }
            }
        }

        /// <summary>
        /// Indicates whether the chess field is captured by a chess piece.
        /// </summary>
        public bool IsCapturedByPiece { get { return Piece != null; } }

        #endregion Members
    }
}
