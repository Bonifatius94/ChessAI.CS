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
    /// This class represents a position on a chess board. Therefore the data is stored as a single byte of two 3-bit numbers for row and column (??RRRCCC), the leading 2 bits are unused.
    /// Moreover this is an immutable data type optimized for high performance use.
    /// </summary>
    public readonly struct ChessPosition : ICloneable
    {
        #region Constructor

        /// <summary>
        /// Create a new field position instance from the given field name.
        /// </summary>
        /// <param name="fieldName">the chess field name (e.g. E5)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPosition(string fieldName)
        {
            // parse row and column
            int row = fieldName[1] - '1';
            int column = char.ToUpper(fieldName[0]) - 'A';
            
            // check if the field name format is correct (otherwise throw argument exception)
            if (!AreCoordsValid(row, column)) { throw new ArgumentException($"invalid field name { fieldName }!"); }
            
            // set hash code
            _hashCode = (byte)((row << 3) | column);
        }

        /// <summary>
        /// Create a new field position instance from the given (row, column) tuple.
        /// </summary>
        /// <param name="coords">The (row, column) tuple</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPosition(Tuple<int, int> coords) : this(coords.Item1, coords.Item2) { }

        /// <summary>
        /// Create a new field position instance from the given row and column.
        /// </summary>
        /// <param name="row">The row of the new chess position instance</param>
        /// <param name="column">The column of the new chess position instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPosition(int row, int column) : this((byte)((row << 3) | column)) { }

        /// <summary>
        /// Create a new field position instance from the given hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the new chess position instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChessPosition(byte hashCode)
        {
            // make sure the given hash code is within value range
            if (hashCode < 0 || hashCode >= 64) { throw new ArgumentException("invalid hash code (value of set { 0, 1, ..., 63 } expected)"); }

            _hashCode = hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The byte representing a position on a chess board.
        /// The data is stored as a single byte of two 3-bit numbers for row and column (??RRRCCC), the leading 2 bits are unused.
        /// </summary>
        private readonly byte _hashCode;
        
        /// <summary>
        /// The row index of the chess position, starting with 0.
        /// </summary>
        public int Row
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _hashCode >> 3; }
        }

        /// <summary>
        /// The column index of the chess position, starting with 0.
        /// </summary>
        public int Column
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _hashCode & 7; }
        }

        /// <summary>
        /// Generates the chess field name out of Row and Column property (e.g. 'A1', 'H8').
        /// </summary>
        public string FieldName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return $"{ (char)('A' + Column) }{ (char)('1' + Row) }"; }
        }

        /// <summary>
        /// Generates the color of the chess field at the given position on the chess board.
        /// </summary>
        public ChessColor ColorOfField
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (((Row + Column) % 2) == 1) ? ChessColor.White : ChessColor.Black; }
        }

        #endregion Members

        #region Methods

        /// <summary>
        /// Check whether the given coords are in bounds of the chess board.
        /// </summary>
        /// <param name="fieldName">The name of the field (with the coords)</param>
        /// <returns>a boolean whether the coords are valid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreCoordsValid(string fieldName)
        {
            bool ret = false;

            if (fieldName.Length == 2)
            {
                // parse row and column
                int row = fieldName[1] - '1';
                int column = char.ToUpper(fieldName[0]) - 'A';

                ret = row >= 0 && row < 8 && column >= 0 && column < 8;
            }

            return ret;
        }

        /// <summary>
        /// Check whether the given coords are in bounds of the chess board.
        /// </summary>
        /// <param name="coords">The (row, column) tuple to be checked</param>
        /// <returns>a boolean whether the coords are valid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreCoordsValid(Tuple<int, int> coords)
        {
            return AreCoordsValid(coords.Item1, coords.Item2);
        }

        /// <summary>
        /// Check whether the given coords are in bounds of the chess board.
        /// </summary>
        /// <param name="row">The row to be checked</param>
        /// <param name="column">The column to be checked</param>
        /// <returns>a boolean whether the coords are valid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreCoordsValid(int row, int column)
        {
            return row >= 0 && row < 8 && column >= 0 && column < 8;
        }

        /// <summary>
        /// Retrieve a string representing this chess position.
        /// </summary>
        /// <returns>a string representing this chess position</returns>
        public override string ToString()
        {
            return FieldName;
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the row and column propoerty.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns>a boolean indicating whether the positions are equal</returns>
        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof(ChessPosition)) && (((ChessPosition)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore the hash code of the instance is returned.
        /// </summary>
        /// <returns>a hash code that is unique for each (row, column) tuple</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Clone()
        {
            return new ChessPosition(_hashCode);
        }

        /// <summary>
        /// Implements the '==' operator for comparing positions.
        /// </summary>
        /// <param name="c1">The first position to compare</param>
        /// <param name="c2">The second position to compare</param>
        /// <returns>a boolean that indicates whether the positions are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ChessPosition c1, ChessPosition c2)
        {
            return c1.GetHashCode() == c2.GetHashCode();
        }

        /// <summary>
        /// Implements the '!=' operator for comparing positions.
        /// </summary>
        /// <param name="c1">The first position to compare</param>
        /// <param name="c2">The second position to compare</param>
        /// <returns>a boolean that indicates whether the positions are not equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ChessPosition c1, ChessPosition c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }

        #endregion Methods
    }
}
