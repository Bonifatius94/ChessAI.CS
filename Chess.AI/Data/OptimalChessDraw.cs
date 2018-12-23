using Chess.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess.AI.Data
{
    /// <summary>
    /// Representing an entity of an optimal chess draw and the corresponding game situation.
    /// </summary>
    public class OptimalChessDraw
    {
        #region Constructor

        /// <summary>
        /// Create an empty instance (for EF).
        /// </summary>
        public OptimalChessDraw() { }

        /// <summary>
        /// Create a new optimal chess draw instance with the given chess board and chess draw.
        /// </summary>
        /// <param name="board">the chess board with the game situation</param>
        /// <param name="optimalDraw">the optimal chess draw</param>
        public OptimalChessDraw(ChessBoard board, ChessDraw optimalDraw)
        {
            Board = board;
            OptimalDraw = optimalDraw;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The numberic ID column (primary key).
        /// </summary>
        [Key]
        public ulong ID { get; set; }

        /// <summary>
        /// The unique game situation hash containing the drawing side and the chess board with the game situation.
        /// </summary>
        [StringLength(64)]
        public string GameSituationHash { get; set; }

        /// <summary>
        /// The hash of the optimal chess draw for the given game situation.
        /// </summary>
        [StringLength(3)]
        public int OptimalDrawHash { get; set; }

        /// <summary>
        /// The chess board with the game situation.
        /// </summary>
        [NotMapped]
        public ChessBoard Board
        {
            get { return GameSituationHash.ToBoard(); }
            set { GameSituationHash = value.ToHash(); }
        }

        /// <summary>
        /// The optimal chess draw for the given game situation.
        /// </summary>
        [NotMapped]
        public ChessDraw OptimalDraw
        {
            get { return new ChessDraw(OptimalDrawHash); }
            set { OptimalDrawHash = value.GetHashCode(); }
        }

        #endregion Members
    }

    /// <summary>
    /// An extension for conversions between a hash code string and a chess board instance (both directions).
    /// </summary>
    public static class ChessBoardHashEx
    {
        #region Constants

        private const byte CHESS_PIECE_NULL = 0xFF;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Help converting a chess board instance to a hash string.
        /// </summary>
        /// <param name="board">the chess board to be converted</param>
        /// <returns>a hash string containing the data from the chess board</returns>
        public static string ToHash(this ChessBoard board)
        {
            // init a new bitboard with 40 byte
            var bitboard = new Bitboard(320);

            // loop through all chess positions
            for (byte pos = 0; pos < 64; pos++)
            {
                // get the chess piece bits
                var position = new ChessPosition(pos);
                byte pieceCode = (byte)((board.GetPieceAt(position)?.GetHashCode() ?? CHESS_PIECE_NULL) << 3);

                // apply bits to bitboard
                bitboard.SetBitsAt(pos * 5, pieceCode, 5);
            }

            // convert bitboard to a hex stzing
            string hex = bitboard.BinaryData.BytesToHexString();
            return hex;
        }

        /// <summary>
        /// Help converting a hash string to a chess board instance.
        /// </summary>
        /// <param name="hash">the hash string to be converted</param>
        /// <returns>a new chess board instance containing the data from the hash</returns>
        public static ChessBoard ToBoard(this string hash)
        {
            var board = new ChessBoard();
            var bytes = hash.HexStringToBytes();
            var bitboard = new Bitboard(bytes);

            for (byte posHash = 0; posHash < 64; posHash++)
            {
                byte pieceHash = (byte)(bitboard.GetBitsAt(posHash * 5, 5) >> 3);
                var piece = (pieceHash == CHESS_PIECE_NULL) ? (ChessPiece?)null : new ChessPiece(pieceHash);
                board.UpdatePieceAt(new ChessPosition(posHash), piece, (posHash == 63));
            }

            return board;
        }
        
        #endregion Methods
    }

    /// <summary>
    /// Provide conversion functionality between hex strings and binary byte arrays.
    /// </summary>
    public static class BytesToHexStringEx
    {
        #region Methods

        /// <summary>
        /// Convert a hex string to a binary byte array.
        /// </summary>
        /// <param name="hexString">The hex string containing the data.</param>
        /// <returns>a binary byte array</returns>
        public static byte[] HexStringToBytes(this string hexString)
        {
            // init binary data array
            int bytesCount = (hexString.Length / 2) + (hexString.Length % 2 > 0 ? 1 : 0);
            var data = new byte[bytesCount];

            // loop through all hex digits
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // get upper and lower nibble
                byte upper = (byte)(getHexByte(hexString[i]) << 4);
                byte lower = (i + 1 < hexString.Length) ? getHexByte(hexString[i + 1]) : (byte)0;

                // append the two corresponding hex characters
                data[i / 2] = (byte)(upper | lower);
            }

            return data;
        }

        /// <summary>
        /// Convert a binary byte array to a hex string.
        /// </summary>
        /// <param name="data">The binary byte array containing the data.</param>
        /// <returns>a hex string</returns>
        public static string BytesToHexString(this byte[] data)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                // get upper and lower nibble
                byte upper = (byte)((data[i] & 0xF0) >> 4);
                byte lower = (byte)(data[i] & 0x0F);

                // append the two corresponding hex characters
                builder.Append($"{ getHexChar(upper) }{ getHexChar(lower) }");
            }

            return builder.ToString();
        }

        #region Helpers

        private static byte getHexByte(char hex)
        {
            return (byte)(hex - ((hex >= '0' && hex <= '9') ? '0' : ((hex >= 'A' && hex <= 'F') ? 'A' : 'a')));
        }

        private static char getHexChar(byte data)
        {
            return (char)(((data < 10) ? '0' : 'A') + data);
        }

        #endregion Helpers

        #endregion Methods
    }
}
