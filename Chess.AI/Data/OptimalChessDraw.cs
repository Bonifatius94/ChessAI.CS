using Chess.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
            string hash = string.Empty;

            for (byte pos = 0; pos < 64; pos++)
            {
                var piece = board.GetPieceAt(new ChessPosition(pos));
                hash += (char)(piece.HasValue ? piece.Value.GetHashCode() : CHESS_PIECE_NULL);
            }

            return hash;
        }

        /// <summary>
        /// Help converting a hash string to a chess board instance.
        /// </summary>
        /// <param name="hash">the hash string to be converted</param>
        /// <returns>a new chess board instance containing the data from the hash</returns>
        public static ChessBoard ToBoard(this string hash)
        {
            var board = new ChessBoard();

            for (byte pos = 0; pos < 64; pos++)
            {
                byte data = (byte)hash[pos];
                var piece = (data == CHESS_PIECE_NULL) ? (ChessPiece?)null : new ChessPiece(data);
                board.UpdatePieceAt(new ChessPosition(pos), piece, (pos == 63));
            }

            return board;
        }

        #endregion Methods
    }

    ///// <summary>
    ///// An extension for appending and cutting bits.
    ///// </summary>
    //public static class BitOperationsEx
    //{
    //    #region Methods

    //    /// <summary>
    //    /// Help cutting bits from a binary array.
    //    /// </summary>
    //    /// <param name="source">the binary array containing the data</param>
    //    /// <param name="start">the starting bit index</param>
    //    /// <param name="count">the amount of bits to be cut</param>
    //    /// <param name="shiftRight">indicates whether the trailing bits should be at the start or at the end of the output array</param>
    //    /// <returns></returns>
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public static byte[] CutBits(this byte[] source, int start, int count, bool shiftRight = true)
    //    {
    //        // determine indices, counts, bit offsets, ...
    //        int bitsToCopy = count;
    //        int offset = start % 8;
    //        int sourceStartIndex = start / 8;
    //        int outputBytesCount = (count / 8) + ((count % 8) > 0 ? 1 : 0);

    //        // make sure the source array is long enough to cut all bits
    //        if ((sourceStartIndex + outputBytesCount) >= source.Length) { throw new ArgumentException($"source array is too short to cut { count } bit starting from bit { start }"); }

    //        // allocate output bytes
    //        var output = new byte[outputBytesCount];

    //        // copy all output bytes (although not shifted right)
    //        for (int i = 0; i < outputBytesCount; i++)
    //        {
    //            // determine how many bits should be copied
    //            int bitsCount = (bitsToCopy >= 8 ? 8 : bitsToCopy);

    //            // cut the data byte and write it to output
    //            byte upper = source[sourceStartIndex + i];
    //            byte lower = (bitsToCopy > 8) ? source[sourceStartIndex + i + 1] : (byte)0x00;
    //            output[i] = CutByte(upper, lower, offset, bitsCount);

    //            // update index and bits to copy
    //            bitsToCopy -= bitsCount;
    //        }

    //        if (shiftRight)
    //        {
    //            // reallocate output bytes
    //            var temp = output;
    //            output = new byte[outputBytesCount];

    //            // determine how much the bit shift has to be
    //            int trailingBits = 8 - (count % 8);

    //            // handle first byte
    //            output[0] = (byte)(temp[0] >> offset);
                
    //            // copy the rest of the bytes
    //            for (int i = 1; i < outputBytesCount - 1; i++)
    //            {
    //                // TODO: fix logic
    //                output[i] = CutByte(temp[i], temp[i + 1], trailingBits);
    //            }
    //        }

    //        return output;
    //    }

    //    /// <summary>
    //    /// Cut a byte that is spread over two bytes.
    //    /// </summary>
    //    /// <param name="upper">the upper source byte</param>
    //    /// <param name="lower">the lower source byte</param>
    //    /// <param name="offset">the offset bits</param>
    //    /// <param name="bitsCount">the bits to </param>
    //    /// <returns>the cut bits as byte (if bits count is less that 8, the data bits are shifted to the left and the trailing bits are zero)</returns>
    //    /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public static byte CutByte(byte upper, byte lower, int offset, int bitsCount = 8)
    //    {
    //        // make sure offset and bits count are valid
    //        if (offset >= 8) { throw new ArgumentException($"wrong offset { offset } (expected index 7 or less)"); }
    //        if (bitsCount > 8) { throw new ArgumentException($"bits count { bitsCount } is more than one byte (expected 8 or less bits)"); }

    //        // handle easy case without offset more efficiently
    //        if (offset == 0) { return (bitsCount == 8) ? upper : (byte)((upper >> (8 - bitsCount)) << (8 - bitsCount)); }
            
    //        // load the current and the following byte concatenated as ushort (16 bits)
    //        ushort cache = (ushort)((upper << 8) | lower);

    //        // determine which bits should be cut (with bitwise AND operation)
    //        ushort dataBitsToCut = (ushort)((1 << (16 - offset)) - 1);

    //        // get the data byte
    //        byte data = (byte)((cache & dataBitsToCut) >> (bitsCount - offset));

    //        return data;
    //    }

    //    /// <summary>
    //    /// Help appending two bytes at the given offset.
    //    /// </summary>
    //    /// <param name="source">the source byte</param>
    //    /// <param name="append">the byte to append</param>
    //    /// <param name="index">the last data bit index of the source byte (exclusive)</param>
    //    /// <returns></returns>
    //    public static byte AppendByteAtIndex(byte source, byte append, int index)
    //    {
    //        byte and = (byte)~((1 << (8 - index)) - 1);
    //        byte data = (byte)((source & and) | (append >> index));
    //        return data;
    //    }

    //    #endregion Methods
    //}
}
