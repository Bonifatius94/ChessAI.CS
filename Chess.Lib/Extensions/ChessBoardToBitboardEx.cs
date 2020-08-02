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
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// An extension for conversions between a hash code string and a chess board instance (both directions).
    /// </summary>
    public static class ChessBoardToBitboardEx
    {
        #region Methods

        /// <summary>
        /// Help converting a chess board instance to a hash string.
        /// </summary>
        /// <param name="board">the chess board to be converted</param>
        /// <returns>a hash string containing the data from the chess board</returns>
        public static Bitboard ToBitboard(this IChessBoard board)
        {
            // init a new bitboard with 40 byte
            var bitboard = new Bitboard(40 * 8);

            // loop through all chess positions
            for (byte pos = 0; pos < 64; pos++)
            {
                // get the chess piece bits
                var position = new ChessPosition(pos);
                byte pieceCode = (byte)(board.GetPieceAt(position).GetHashCode() << 3);

                // apply bits to bitboard
                bitboard.SetBitsAt(pos * 5, pieceCode, 5);
            }

            return bitboard;
        }

        /// <summary>
        /// Help converting binary data (bitboard) into a chess board instance.
        /// </summary>
        /// <param name="bitboard">the bitboard containing the binary data to be converted</param>
        /// <returns>a new chess board instance containing the data from the bitboard</returns>
        public static ChessBoard ToBoard(this Bitboard bitboard)
        {
            var pieces = new ChessPiece[64];

            for (byte posHash = 0; posHash < 64; posHash++)
            {
                byte pieceHash = (byte)(bitboard.GetBitsAt(posHash * 5, 5) >> 3);
                var piece = new ChessPiece(pieceHash);
                pieces[posHash] = piece;
            }

            return new ChessBoard(pieces);
        }

        /// <summary>
        /// Help converting binary data (bitboard) into a chess board instance.
        /// </summary>
        /// <param name="bytes">the binary data to be converted</param>
        /// <returns>a new chess board instance containing the data from the given bytes</returns>
        public static IChessBoard ToBoard(this byte[] bytes)
        {
            var bitboard = new Bitboard(bytes);
            var pieces = new ChessPiece[64];

            for (byte posHash = 0; posHash < 64; posHash++)
            {
                byte pieceHash = (byte)(bitboard.GetBitsAt(posHash * 5, 5) >> 3);
                var piece = new ChessPiece(pieceHash);
                pieces[posHash] = piece;
            }

            return new ChessBoard(pieces);
        }

        #endregion Methods
    }
}
