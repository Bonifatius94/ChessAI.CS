using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This struct represents a chess piece and its position. (required because position property was removed from chess piece struct)
    /// </summary>
    public readonly struct ChessPieceAtPos
    {
        #region Constructor

        /// <summary>
        /// Create a new instance of (chess position, chess piece) tuple.
        /// </summary>
        /// <param name="position">The position of the chess piece</param>
        /// <param name="piece">The chess piece data</param>
        public ChessPieceAtPos(ChessPosition position, ChessPiece piece)
        {
            Position = position;
            Piece = piece;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The chess position of the chess piece.
        /// </summary>
        public readonly ChessPosition Position;

        /// <summary>
        /// The chess piece data.
        /// </summary>
        public readonly ChessPiece Piece;

        #endregion Members

        #region Methods

        /// <summary>
        /// Retrieves a string representation of this instance.
        /// </summary>
        /// <returns>a string representation of this instance</returns>
        public override string ToString()
        {
            return $"{ Piece.ToString() } ({ Position.ToString() })";
        }

        // TODO: implement other overrides Equals(), GetHashCode(), ==(ChessPieceAtPos, ChessPieceAtPos), ...

        #endregion Methods
    }
}
