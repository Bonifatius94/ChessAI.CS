using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// An extension for conversions between a hash code string and a chess board instance (both directions).
    /// </summary>
    public static class ChessBoardHashEx
    {
        #region Methods

        /// <summary>
        /// Help converting a chess board instance to a hash string.
        /// </summary>
        /// <param name="board">the chess board to be converted</param>
        /// <returns>a hash string containing the data from the chess board</returns>
        public static string ToHash(this ChessBoard board)
        {
            // convert chess board into a hex stzing
            string hex = board.ToBitboard().BinaryData.BytesToHexString();
            return hex;
        }

        /// <summary>
        /// Help converting a hash string to a chess board instance.
        /// </summary>
        /// <param name="hash">the hash string to be converted</param>
        /// <returns>a new chess board instance containing the data from the hash</returns>
        public static ChessBoard HashToBoard(this string hash)
        {
            // convert hash into a chess board
            var board = new Bitboard(hash.HexStringToBytes()).ToBoard();
            return board;
        }

        #endregion Methods
    }
}
