using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This struct represents a chess piece and its position as an immutable data type. (required because position property was removed from chess piece struct)
    /// </summary>
    public readonly struct ChessPieceAtPos
    {
        #region Constructor

        /// <summary>
        /// Create a new instance of (chess position, chess piece) tuple.
        /// </summary>
        /// <param name="position">The position of the chess piece</param>
        /// <param name="piece">The chess piece data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPieceAtPos(ChessPosition position, ChessPiece piece)
        {
            _hashCode = (short)((position.GetHashCode() << 5) | piece.GetHashCode());
        }

        /// <summary>
        /// Create a new instance of hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the new instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPieceAtPos(int hashCode)
        {
            _hashCode = (short)hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The hash code of a chess piece and its position on a chess board.
        /// The data is stored as a short value with 6 bits for the position (X) and 5 bits for the chess piece (Y), the leading 5 bits are usused (?????XXXXXXYYYYY).
        /// </summary>
        private readonly short _hashCode;

        /// <summary>
        /// The chess piece data.
        /// </summary>
        public ChessPiece Piece
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new ChessPiece((byte)(_hashCode & 0b_11111)); }
        }

        /// <summary>
        /// The chess position of the chess piece.
        /// </summary>
        public ChessPosition Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new ChessPosition((byte)(_hashCode >> 5)); }
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Implements the '==' operator for comparing chess pieces.
        /// </summary>
        /// <param name="c1">The first chess piece to compare</param>
        /// <param name="c2">The second chess piece to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ChessPieceAtPos c1, ChessPieceAtPos c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }
        
        #endregion Methods
    }
}
