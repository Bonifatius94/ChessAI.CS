using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// Represents an efficient type for storing up to 10 chess positions as a single unsigned long (64-bit).
    /// </summary>
    // TODO: implement IEnumerable<ChessPositions> to efficiently iterate over chess positions
    public readonly struct CachedChessPositions
    {
        #region Constructor

        public CachedChessPositions(ChessPosition[] positions)
        {
            // make sure that the overloaded array does not contain more than 10 positions 
            if (positions.Length > 10) { throw new NotSupportedException("Invalid argument! Positions cannot store more than 10 chess positions!"); }

            // assign positions
            _hash = 0;
            _hash = deserializeFromArray(positions);
        }

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

                // clear the identified piece on bitboard (this ensures termination)
                bitboard ^= 0x1uL << pos;
            }

            // make sure that no result with a bit overflow is returned (an overflow occurs with bitboards having more than 10 bits set)
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
