using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess field on a chess board.
    /// </summary>
    public class ChessField : ICloneable
    {
        #region Members

        /// <summary>
        /// The position of the chess field.
        /// </summary>
        public ChessFieldPosition Position { get; set; }
        
        /// <summary>
        /// The piece that currently captures the chess field (value is null if there is no chess piece).
        /// </summary>
        public ChessPiece Piece { get; set; }

        /// <summary>
        /// Indicates whether the chess field is captured by a chess piece.
        /// </summary>
        public bool IsCapturedByPiece { get { return Piece != null; } }

        #endregion Members

        #region Methods

        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        public object Clone()
        {
            var field = new ChessField() {
                Position = (ChessFieldPosition)Position.Clone(),
                Piece = Piece.Clone() as ChessPiece
            };

            return field;
        }

        #endregion Methods
    }
}
