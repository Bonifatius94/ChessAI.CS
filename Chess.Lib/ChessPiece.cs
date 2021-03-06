﻿/*
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

using System;
using System.Runtime.CompilerServices;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration of all chess piece types.
    /// </summary>
    public enum ChessPieceType
    {
        /// <summary>
        /// Representing an invalid chess piece type (this is used for nullable chess piece value).
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Representing a king chess piece.
        /// </summary>
        King = 1,

        /// <summary>
        /// Representing a queen chess piece.
        /// </summary>
        Queen = 2,

        /// <summary>
        /// Representing a rook chess piece.
        /// </summary>
        Rook = 3,

        /// <summary>
        /// Representing a bishop chess piece.
        /// </summary>
        Bishop = 4,

        /// <summary>
        /// Representing a knigh chess piece.
        /// </summary>
        Knight = 5,

        /// <summary>
        /// Representing a peasant chess piece.
        /// </summary>
        Peasant = 6
    }
    
    /// <summary>
    /// An enumeration of all chess piece colors.
    /// </summary>
    public enum ChessColor
    {
        /// <summary>
        /// Representing white chess pieces, white chess fields, etc.
        /// </summary>
        White = 0,

        /// <summary>
        /// Representing black chess pieces, black chess fields, etc.
        /// </summary>
        Black = 1,
    }
    
    /// <summary>
    /// Represents a chess piece by its color, chess piece type and whether it was already moved.
    /// </summary>
    public struct ChessPiece : ICloneable
    {
        #region Constants

        // define the trailing bits after the data bits
        private const byte COLOR_TRAILING_BITS     = 4;
        private const byte WAS_MOVED_TRAILING_BITS = 3;
        
        // define which bits of the hash code store the data
        private const byte BITS_OF_COLOR          = 0b_10000;   // bits: 10000
        private const byte BITS_OF_WAS_MOVED_FLAG = 0b_01000;   // bits: 01000
        private const byte BITS_OF_TYPE           = 0b_00111;   // bits: 00111

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Creates a chess piece instance with the given parameters.
        /// </summary>
        /// <param name="type">The type of the chess piece</param>
        /// <param name="color">The color of the chess piece</param>
        /// <param name="wasMoved">Indicates whether the chess piece was already moved</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPiece(ChessPieceType type, ChessColor color, bool wasMoved)
        {
            byte colorBits = (byte)(((int)color) << COLOR_TRAILING_BITS);
            byte wasMovedBits = (byte)((wasMoved ? 1 : 0) << WAS_MOVED_TRAILING_BITS);
            byte typeBits = (byte)type;

            // fuse the bit patterns to the hash code (with bitwise OR)
            _hashCode = (byte)(colorBits | wasMovedBits | typeBits);
        }

        /// <summary>
        /// Creates a chess piece instance from hash code.
        /// </summary>
        /// <param name="hashCode">The hash code containing the chess piece data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPiece(int hashCode)
        {
            // make sure the hash code is within the expected value range
            if (hashCode < 0 || hashCode >= 0b_100000) { throw new ArgumentException("invalid hash code detected (expected a number of set { 0, 1, ..., 31 })"); }

            _hashCode = (byte)hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The binary representation containing the chess piece data.
        /// 
        /// The code consists of 5 bits: 
        /// 3 bits for piece type and another 1 bit for color / was moved flag
        /// 
        /// | unused | color | was moved | type |
        /// |    xxx |     x |         x |  xxx |
        /// </summary>
        private byte _hashCode;
        
        /// <summary>
        /// The color of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessColor Color
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (ChessColor)((_hashCode & BITS_OF_COLOR) >> COLOR_TRAILING_BITS); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_COLOR) | (((byte)value) << COLOR_TRAILING_BITS)); }
        }

        /// <summary>
        /// Indicates whether the chess piece was already drawn. (calculated from hash code)
        /// </summary>
        public bool WasMoved
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ((_hashCode & BITS_OF_WAS_MOVED_FLAG) >> WAS_MOVED_TRAILING_BITS) == 1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_WAS_MOVED_FLAG) | (((byte)(value ? 1 : 0)) << WAS_MOVED_TRAILING_BITS)); }
        }

        /// <summary>
        /// The type of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessPieceType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (ChessPieceType)(_hashCode & BITS_OF_TYPE); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_TYPE) | ((byte)value)); }
        }

        /// <summary>
        /// Indicates whether the chess piece is not null.
        /// </summary>
        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Type != ChessPieceType.Invalid; }
        }

        ///// <summary>
        ///// The value of this nullable chess piece implementation.
        ///// </summary>
        //public ChessPiece Value { get { return _piece; } }

        /// <summary>
        /// The chess piece value representing null value. The hash value is chosen as 0, so an empty array of chess pieces automatically contains only null values.
        /// </summary>
        public static readonly ChessPiece NULL = new ChessPiece(0);

        #endregion Members

        #region Methods

        /// <summary>
        /// Determine in an optimized manner whether the chess piece is not null and of the given side.
        /// </summary>
        /// <param name="side">The side used to compare to.</param>
        /// <returns>a boolean indicating whether the chess piece is not null and of the given side</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNonNullablePieceOfSide(ChessColor side)
        {
            // optimized bitwise condition computation (check if type code is other than ChessPieceType.Invalid,
            // then check for the color bit to be the given side and biswise AND both conditions)
            // cond. 1: Get the type bits (last 3 bits), invert them and add +1, so 000 bits would run into an overflow.
            //          Then, check if the overflow occurred and flip bits again, so all bits are set if the piece is not null
            // cond. 2: Get the color bit, shift it to the lowest bit and compare it to the given side by bitwise XOR.
            //          Sides being equal results into the lowest bit being 0, which is flipped to 1.

            int isNotNull = ~(((~_hashCode & BITS_OF_TYPE) + 1) >> 3);
            int isOfSide = ~((byte)side ^ ((_hashCode & BITS_OF_COLOR) >> 4)) & 0x1;
            return (isNotNull & isOfSide) > 0;
        }

        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Clone()
        {
            return new ChessPiece(_hashCode);
        }

        /// <summary>
        /// Retrieve a string representing this chess piece.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string color = Color.ToString().ToLower();
            string type = Type.ToString().ToLower();

            return Type == ChessPieceType.Invalid ? "piece-null" : $"{ color } { type }";
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the properties.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns>a boolean indicating whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(ChessPiece)) && (((ChessPiece)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore the hash code of the instance is returned.
        /// </summary>
        /// <returns>a hash code that is unique for each chess piece</returns>
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
        public static bool operator ==(ChessPiece c1, ChessPiece c2)
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
        public static bool operator !=(ChessPiece c1, ChessPiece c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }
        
        #endregion Methods
    }

    /// <summary>
    /// This is an extension class providing the conversion from a chess piece type enumeration to a character value.
    /// </summary>
    public static class ChessPieceTypeAsChar
    {
        /// <summary>
        /// Retrieve the character representing the given chess piece type.
        /// </summary>
        /// <param name="type">The chess piece type to be represented</param>
        /// <returns>a character representing the given chess piece type</returns>
        public static char ToChar(this ChessPieceType type)
        {
            switch (type)
            {
                case ChessPieceType.King:    return 'K';
                case ChessPieceType.Queen:   return 'Q';
                case ChessPieceType.Rook:    return 'R';
                case ChessPieceType.Bishop:  return 'B';
                case ChessPieceType.Knight:  return 'N';
                case ChessPieceType.Peasant: return 'P';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    /// <summary>
    /// This is an extension class providing the conversion from a chess color enumeration to a character value.
    /// </summary>
    public static class ChessPieceColorAsChar
    {
        /// <summary>
        /// Retrieve the character representing the given chess color.
        /// </summary>
        /// <param name="color">The chess color to be represented</param>
        /// <returns>a character representing the given chess color</returns>
        public static char ToChar(this ChessColor color)
        {
            switch (color)
            {
                case ChessColor.White: return 'W';
                case ChessColor.Black: return 'B';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    /// <summary>
    /// This is an extension class providing the conversion from a chess color enumeration to its' complementary color (white vs. black).
    /// </summary>
    public static class ChessPieceColorOpponent
    {
        /// <summary>
        /// Retrieve the opponent's chess color according to the given allied chess color. (complementary)
        /// </summary>
        /// <param name="color">The allied chess color</param>
        /// <returns>the opponent's chess color</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ChessColor Opponent(this ChessColor color)
        {
            return (ChessColor)(((int)color) ^ 1);
        }
    }
}
