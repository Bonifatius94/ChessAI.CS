/*
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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// <para>Represents an efficient type for storing up to 10 chess positions as a single unsigned long (64-bit).</para>
    /// This type is meant to retrieve positions from sparse bitboards.
    /// </summary>
    // TODO: implement IEnumerable<ChessPositions> to efficiently iterate over chess positions
    public readonly struct CachedChessPositions
    {
        #region Constructor

        /// <summary>
        /// Create a new instance of cached chess positions using the given positions.
        /// </summary>
        /// <param name="positions">The positions to be applied to this instance.</param>
        public CachedChessPositions(ChessPosition[] positions)
        {
            // make sure that the overloaded array does not contain more than 10 positions 
            if (positions.Length > 10) { throw new NotSupportedException("Invalid argument! Positions cannot store more than 10 chess positions!"); }

            // assign positions
            _hash = 0;
            _hash = deserializeFromArray(positions);
        }

        /// <summary>
        /// Create a new instance of cached chess positions using all positions of bits that are set the given bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard containing the positions to be applied to this instance.</param>
        public CachedChessPositions(ulong bitboard)
        {
            _hash = 0;
            _hash = deserializeFromBitboard(bitboard);
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// <para>The positions are encoded as 64-bit unsigned long (max. 10 positions).
        /// The lowest 4 bits contain the amount of positions -> count can be retrieved using 'value &amp; 0xFuL' statement.
        /// The remaining lower bytes contain the normalized ChessPosition bits concatenated (6 bits per position).</para>
        /// <para>e.g. a code for 3 positions: ... xxxxxx|xxxxxx|xxxxxx|011</para>
        /// </summary>
        private readonly ulong _hash;

        /// <summary>
        /// The chess positions of this instance (computed operation).
        /// </summary>
        public ChessPosition[] Positions => serialize();

        #endregion Members

        #region Methods

        private ulong deserializeFromArray(ChessPosition[] positions)
        {
            // apply length
            ulong result = (ulong)positions.Length;

            // apply position data by bitwise OR
            for (byte i = 0; i < positions.Length; i++)
            {
                result |= (ulong)positions[i].GetHashCode() << (i * 6 + 1);
            }

            return result;
        }

        /// <summary>
        /// Retrieve all positions of the given bitboard.
        /// </summary>
        /// <param name="bitboard">The bitboard the be evaluated.</param>
        /// <returns>a 64-bit integer containing encoded positions of the pieces onto the input board</returns>
        private ulong deserializeFromBitboard(ulong bitboard)
        {
            ulong positions = 0uL;
            byte offset = 4;
            byte count = 0;

            // as long as there are unidentified pieces onto the bitboard (max. 10 pieces)
            while (bitboard > 0 && count++ < 10)
            {
                // find highest piece's position (2^pos)
                byte pos = (byte)BitOperations.Log2(bitboard);
                // TODO: check if just shifting through all 64 bits of the board would be more efficient

                // update positions result
                positions |= (ulong)pos << offset;
                positions++;
                offset += 6;

                // clear the identified piece on bitboard (this ensures termination)
                bitboard ^= 0x1uL << pos;
            }

            // make sure that bitboards are rejected if they have more than 10 bits set
            if (count > 10) { throw new ArgumentException("Invalid bitboard! The bitboard contains more than 10 chess pieces!"); }

            return positions;
        }

        private ChessPosition[] serialize()
        {
            ulong posCache = _hash;

            // extract count and initialize result array
            byte count = (byte)(posCache & 0xFuL);
            var positions = new ChessPosition[count];

            // shift the cache to the first position
            posCache >>= 4;

            // loop through positions
            for (byte i = 0; i < count; i++)
            {
                // extract the position and apply it to the result array
                byte pos = (byte)(posCache & 0x3FuL);
                positions[i] = new ChessPosition(pos);

                // shift the cache to the next position
                posCache >>= 6;
            }

            return positions;
        }

        #endregion Methods
    }
}
