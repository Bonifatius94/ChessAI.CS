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
        public static Bitboard ToBitboard(this ChessBoard board)
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
            var board = ChessBoard.StartFormation;

            for (byte posHash = 0; posHash < 64; posHash++)
            {
                byte pieceHash = (byte)(bitboard.GetBitsAt(posHash * 5, 5) >> 3);
                var piece = new ChessPiece(pieceHash);
                board.UpdatePieceAt(new ChessPosition(posHash), piece);
            }

            return board;
        }

        /// <summary>
        /// Help converting binary data (bitboard) into a chess board instance.
        /// </summary>
        /// <param name="bytes">the binary data to be converted</param>
        /// <returns>a new chess board instance containing the data from the given bytes</returns>
        public static ChessBoard ToBoard(this byte[] bytes)
        {
            var board = ChessBoard.StartFormation;
            var bitboard = new Bitboard(bytes);

            for (byte posHash = 0; posHash < 64; posHash++)
            {
                byte pieceHash = (byte)(bitboard.GetBitsAt(posHash * 5, 5) >> 3);
                var piece = new ChessPiece(pieceHash);
                board.UpdatePieceAt(new ChessPosition(posHash), piece);
            }

            return board;
        }

        #endregion Methods
    }
}
