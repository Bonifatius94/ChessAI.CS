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

        /// <summary>
        /// Check whether the two objects are equal.
        /// </summary>
        /// <param name="obj">the instance to be compared to 'this'</param>
        /// <returns>a boolean indicating whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(ChessPieceAtPos)) && (((ChessPieceAtPos)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Retrieve a unique hash code representing a chess piece and its position.
        /// </summary>
        /// <returns>a unique hash code representing a chess piece and its position</returns>
        public override int GetHashCode()
        {
            // combine unique hash codes of chess piece (5 bits) and chess position (6 bits)
            return (Piece.GetHashCode() << 6) | Position.GetHashCode();
        }

        /// <summary>
        /// Implements the '==' operator for comparing chess pieces.
        /// </summary>
        /// <param name="c1">The first chess piece to compare</param>
        /// <param name="c2">The second chess piece to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are equal</returns>
        public static bool operator ==(ChessPieceAtPos c1, ChessPieceAtPos c2)
        {
            return c1.GetHashCode() == c2.GetHashCode();
        }

        /// <summary>
        /// Implements the '!=' operator for comparing chess pieces.
        /// </summary>
        /// <param name="c1">The first chess piece to compare</param>
        /// <param name="c2">The second chess piece to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are not equal</returns>
        public static bool operator !=(ChessPieceAtPos c1, ChessPieceAtPos c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }
        
        #endregion Methods
    }
}
