using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Chess.Lib
{
    public struct ChessBitboard
    {
        #region Constants

        // row masks
        private const ulong ROW_1 = 0x00000000000000FFuL;
        private const ulong ROW_2 = 0x000000000000FF00uL;
        private const ulong ROW_3 = 0x0000000000FF0000uL;
        private const ulong ROW_4 = 0x00000000FF000000uL;
        private const ulong ROW_5 = 0x000000FF00000000uL;
        private const ulong ROW_6 = 0x0000FF0000000000uL;
        private const ulong ROW_7 = 0x00FF000000000000uL;
        private const ulong ROW_8 = 0xFF00000000000000uL;

        // column masks
        private const ulong COL_A = 0x0101010101010101uL;
        private const ulong COL_B = 0x0202020202020202uL;
        private const ulong COL_C = 0x0404040404040404uL;
        private const ulong COL_D = 0x0808080808080808uL;
        private const ulong COL_E = 0x1010101010101010uL;
        private const ulong COL_F = 0x2020202020202020uL;
        private const ulong COL_G = 0x4040404040404040uL;
        private const ulong COL_H = 0x8080808080808080uL;

        #endregion Constants

        #region Constructor

        public ChessBitboard(ChessBoard board)
        {
            _bitboards = new ulong[13];
            FromBoard(board);
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// <para>An array containing the bitboards for each (color, chess piece type) tuple.</para>
        /// <para>Chess Pieces: white pieces (indices 0 - 5), black pieces (indices 6 - 11), standard piece type offsets (king=0, queen=1, rook=2, bishop=3, knight=4, peasant=5).
        /// The bits of each bitboard are addressed by normalized ChessPosition hashes.
        /// </para>
        /// <para>Was Moved: index 12, one bit of each chess piece at the position of the start formation whether the piece was moved. Addressing uses normalized ChessPosition hashes as well.</para>
        /// </summary>
        private ulong[] _bitboards;

        /// <summary>
        /// Retrieve a new chess board instance with start formation.
        /// </summary>
        public static ChessBitboard StartFormation => new ChessBitboard(ChessBoard.StartFormation);

        /// <summary>
        /// Convert the bitboard to the human-readable chess board format (computed property).
        /// </summary>
        public ChessBoard Board => ToBoard();

        #endregion Members

        #region Methods

        public ulong getPiecesBitboard(ChessPieceType type, ChessColor color)
        {
            byte index = (byte)(((byte)type - 1) * (color == ChessColor.White ? 0 : 1));
            return _bitboards[index];
        }

        private ulong getWasMovedBitboard()
        {
            return _bitboards[12];
        }

        private bool isCapturedAt(ulong bitboard, byte pos)
        {
            ulong mask = 0x0000000000000001uL << pos;
            return (bitboard & mask) > 0;
        }

        #region Convert

        // TODO: think maybe of a better method signature
        public ChessBoard ToBoard()
        {
            // TODO: test if this works

            // init pieces array with empty fields
            var pieces = new ChessPiece[64];

            // loop through all bitboards
            for (byte i = 0; i < 12; i++)
            {
                // determine the chess piece type and color of the iteration
                var pieceType = (ChessPieceType)((i % 6) + 1);
                var color = (ChessColor)(i / 6);

                // cache bitboard for shifting bitwise
                ulong bitboard = getPiecesBitboard(pieceType, color);

                // loop through all positions
                for (byte pos = 0; pos < 64; pos++)
                {
                    // write piece to array if there is one
                    if ((bitboard & 0x0000000000000001uL) > 0) { pieces[pos] = new ChessPiece(pieceType, color, isCapturedAt(_bitboards[12], pos)); }

                    // shift bitboard
                    bitboard >>= 1;
                }
            }

            // return a new chess board with the converted chess pieces
            return new ChessBoard(pieces);
        }

        // TODO: think maybe of a better method signature
        public void FromBoard(ChessBoard board)
        {
            // TODO: test if this works

            // loop through all bitboards
            for (byte i = 0; i < 12; i++)
            {
                // determine the chess piece type and color of the iteration
                var pieceType = (ChessPieceType)((i % 6) + 1);
                var color = (ChessColor)(i / 6);

                // cache bitboard for shifting bitwise
                ulong bitboard = 0;

                // loop through all positions
                for (byte pos = 0; pos < 64; pos++)
                {
                    bitboard += board.IsCapturedAt(pos) ? 1uL : 0uL;
                    bitboard >>= 1;
                }

                // apply converted bitboard
                _bitboards[i] = bitboard;
            }

            ulong wasMoved = 0uL;

            // init was moved bitboard
            for (byte i = 0; i < 16; i++)
            {
                byte whitePos = i;
                byte blackPos = (byte)(i + 48);

                wasMoved |= board.IsCapturedAt(whitePos) ? 1uL << whitePos : 0uL;
                wasMoved |= board.IsCapturedAt(blackPos) ? 1uL << blackPos : 0uL;
            }

            // apply converted bitboard
            _bitboards[12] = wasMoved;
        }

        #endregion Convert

        #region DrawGen

        /// <summary>
        /// Generate all draws given the last draw and 
        /// </summary>
        /// <param name="lastDraw"></param>
        /// <param name="analyzeDrawIntoCheck"></param>
        /// <returns></returns>
        public ulong[] GetAllDraws(ChessDraw? lastDraw = null, bool analyzeDrawIntoCheck = false)
        {
            // initialize draws with empty list
            var draws = new List<ulong>();

            // determine the drawing side
            var drawingSide = lastDraw?.DrawingSide ?? ChessColor.White;
            byte offset = (byte)((byte)drawingSide * 6);

            // compute the draws for each chess piece type
            for (byte i = 0; i < 6; i++)
            {
                // get chess piece type and bitboard
                var pieceType = (ChessPieceType)(i + 1);
                ulong bitboard = _bitboards[i + offset];

                // check if the bitboard contains pieces (otherwise skip)
                if (bitboard != 0uL)
                {
                    ulong[] tempDraws;

                    switch (pieceType)
                    {
                        case ChessPieceType.King:    tempDraws = getKingDraws(drawingSide);              break;
                        case ChessPieceType.Queen:   tempDraws = getQueenDraws(drawingSide);             break;
                        case ChessPieceType.Rook:    tempDraws = getRookDraws(drawingSide);              break;
                        case ChessPieceType.Bishop:  tempDraws = getBishopDraws(drawingSide);            break;
                        case ChessPieceType.Knight:  tempDraws = getKnightDraws(drawingSide);            break;
                        case ChessPieceType.Peasant: tempDraws = getPeasantDraws(drawingSide, lastDraw); break;
                        default: throw new ArgumentException($"Invalid chess piece type '{ pieceType }' detected! Cannot compute draws for this unknown piece type!");
                    }
                }
            }

            // if flag is active, filter only draws that do not cause draws into check
            if (analyzeDrawIntoCheck)
            {
                // TODO: implement logic (this needs to be very efficient, e.g. use getAllCapturedFields() to fasten up enemy draw calculation)
            }

            return draws;
        }

        private IEnumerable<ChessDraw> getKingDraws(ChessColor side)
        {
            // get the kings bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[offset];

            // determine the position of the piece onto the bitboard
            byte oldPos = (byte)BitOperations.Log2(bitboard);
            // TODO: check if this is too costly

            // get king draws
            //ulong forewardRight  = (side == ChessColor.White) ? bitboard << 9 : bitboard >> 7;
            //ulong forewardMiddle = (side == ChessColor.White) ? bitboard << 8 : bitboard >> 8;
            //ulong forewardLeft   = (side == ChessColor.White) ? bitboard << 7 : bitboard >> 9;
            //ulong sideRight      = (side == ChessColor.White) ? bitboard << 1 : bitboard >> 1;
            //ulong sideLeft       = (side == ChessColor.White) ? bitboard >> 1 : bitboard << 1;
            //ulong backwardRight  = (side == ChessColor.White) ? bitboard >> 9 : bitboard << 7;
            //ulong backwardMiddle = (side == ChessColor.White) ? bitboard >> 8 : bitboard << 8;
            //ulong backwardLeft   = (side == ChessColor.White) ? bitboard >> 7 : bitboard << 9;
            // TODO: think of removing the checks for drawing side as draw bitboards are mirrored anyways

            // TODO: implement validation of board edge overflows (e.g. use something like greater than / smaller than comparison
            // 1) do line check with the position

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private IEnumerable<ChessDraw> getQueenDraws(ChessColor side)
        {
            return getRookDraws(side, 1).Union(getBishopDraws(side, 1));
        }

        private IEnumerable<ChessDraw> getRookDraws(ChessColor side, byte bitboardIndex = 2)
        {
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private IEnumerable<ChessDraw> getBishopDraws(ChessColor side, byte bitboardIndex = 3)
        {
            // get bishops bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // use even / uneven bitmasks
            ulong evenMask = 0x55AA55AA55AA55AAuL;
            ulong unevenMask = 0xAA55AA55AA55AA55uL;

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private IEnumerable<ChessDraw> getKnightDraws(ChessColor side)
        {
            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private ulong[] getPeasantDraws(ChessColor side, ChessDraw? lastDraw = null)
        {
            // get peasants bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[5 + offset];

            // get all fields captured by enemy pieces as bitboard
            ulong alliedPieces = getCapturedFields(side);
            ulong enemyPieces = getCapturedFields(side.Opponent());
            ulong blockingPieces = alliedPieces & enemyPieces;

            // get en-passant mask (mask has bits set at level=6, only if last draw is a two-foreward peasant draw, otherwise mask is zero)
            bool checkForEnPassant = (lastDraw != null && lastDraw.Value.DrawingPieceType == ChessPieceType.Peasant 
                && Math.Abs(lastDraw.Value.OldPosition.Row - lastDraw.Value.NewPosition.Row) == 2);
            int epOffset = ;
            ulong epRow = (side == ChessColor.White ? ROW_3 : ROW_5);
            ulong enPassantMask = checkForEnPassant ? ((epRow & (COL_A & COL_C)) << epOffset) & epRow : 0uL;

            if (side == ChessColor.White)
            {
                // get one-foreward / two-foreward draws
                ulong drawsOneFordward = (bitboard << 8) & ~blockingPieces;
                ulong drawsTwoFordward = ((bitboard & ROW_2) << 16) & (~blockingPieces | (~blockingPieces << 8));

                // get right / left catch draws
                ulong drawsCatchRight = (bitboard << 9) & ~COL_H & enemyPieces;
                ulong drawsCatchLeft = (bitboard << 7) & ~COL_A & enemyPieces;

                // get en-passant draws


                // TODO: evaluate the computed draws
            }
            else
            {
                // get one-foreward / two-foreward draws
                ulong drawsOneFordward = (bitboard >> 8) & ~blockingPieces;
                ulong drawsTwoFordward = ((bitboard & ROW_7) >> 16) & (~blockingPieces | (~blockingPieces >> 8));

                // get right / left catch draws
                ulong drawsCatchRight = (bitboard >> 7) & ~COL_A & enemyPieces;
                ulong drawsCatchLeft = (bitboard >> 9) & ~COL_H & enemyPieces;

                // get en-passant draws

            }

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private ulong getCapturedFields(ChessColor side)
        {
            byte offset = (byte)((byte)side * 6);
            return _bitboards[offset] | _bitboards[offset + 1] | _bitboards[offset + 2] | _bitboards[offset + 3] | _bitboards[offset + 4] | _bitboards[offset + 5];
        }

        //private ulong getAllCapturedFields(ChessColor side)
        //{
            // init result with bitboard of false values
            //byte offset = (byte)((byte)side * 6);
            //ulong ret = 0;

            //// loop through all bitboards of the given side
            //for (byte i = 0; i < 6; i++)
            //{
            //    // apply occupied fields to the output by bitwise OR
            //    ret |= _bitboards[i + offset];
            //}

            //return ret;
        //}

        #endregion DrawGen

        #endregion Methods
    }

    //public static class MathEx
    //{
    //    #region ulongLog2

    //    // snippet source: https://stackoverflow.com/questions/11376288/fast-computing-of-log2-for-64-bit-integers

    //    // a magic cache table of log2-powers
    //    private static readonly byte[] tab64 = new byte[] {
    //        63,  0, 58,  1, 59, 47, 53,  2,
    //        60, 39, 48, 27, 54, 33, 42,  3,
    //        61, 51, 37, 40, 49, 18, 28, 20,
    //        55, 30, 34, 11, 43, 14, 22,  4,
    //        62, 57, 46, 52, 38, 26, 32, 41,
    //        50, 36, 17, 19, 29, 10, 13, 21,
    //        56, 45, 25, 31, 35, 16,  9, 12,
    //        44, 24, 15,  8, 23,  7,  6,  5
    //    };

    //    /// <summary>
    //    /// Compute the log2(x) function for the given ulong value.
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static byte Log2(ulong value)
    //    {
    //        value |= value >> 1;
    //        value |= value >> 2;
    //        value |= value >> 4;
    //        value |= value >> 8;
    //        value |= value >> 16;
    //        value |= value >> 32;
    //        return tab64[(value - (value >> 1)) * 0x07EDD5E59A4E28C2 >> 58];
    //    }

    //    #endregion ulongLog2
    //}
}
