using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This struct represents a chess board and all fields / pieces on it. It is designed for high performance draw computations which may be non-intuitive but efficient.
    /// </summary>
    public struct ChessBitboard : IChessBoard
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

        /// <summary>
        /// Create a new chess board instance from the given human-readable chess board.
        /// </summary>
        /// <param name="board">The human-readable chess board containing the board data.</param>
        public ChessBitboard(ChessBoard board)
        {
            // initialize bitboards
            _bitboards = new ulong[13];

            // apply board data to the bitboards
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
        // TODO: think about adding another 2 entries for a cached summary of all white and all black pieces (this saves 6 bitwise OR operations)

        /// <summary>
        /// Retrieve a new chess board instance with start formation.
        /// </summary>
        public static ChessBitboard StartFormation => new ChessBitboard(ChessBoard.StartFormation);

        #region IChessBoard

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> WhitePieces => GetPiecesOfColor(ChessColor.White);

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> BlackPieces => GetPiecesOfColor(ChessColor.Black);

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos WhiteKing => getKing(ChessColor.White);

        /// <summary>
        /// Selects the black king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos BlackKing => getKing(ChessColor.Black);

        #endregion IChessBoard

        #endregion Members

        #region Methods

        private byte getPosition(ulong bitboard)
        {
            // this returns the numeric value of the highest bit set on the given bitboard
            // if the given bitboard has multiple bits set, only the position of the highest bit is returned
            return (byte)BitOperations.Log2(bitboard);
        }

        private ChessPieceAtPos getKing(ChessColor side)
        {
            // get the position of the king and whether he was already moved
            byte pos = getPosition(_bitboards[6]);
            bool wasMoved = isSetAt(_bitboards[12], pos);

            // put everything together (this already uses bitwise operations, so no further optimizations required)
            return new ChessPieceAtPos(new ChessPosition(pos), new ChessPiece(ChessPieceType.King, side, wasMoved));
        }

        private bool isSetAt(ulong bitboard, byte pos)
        {
            ulong mask = 0x1uL << pos;
            return (bitboard & mask) > 0;
        }

        //private ulong setBitAt(ulong bitboard, byte pos)
        //{
        //    ulong mask = 0x1uL << pos;
        //    return bitboard | mask;
        //}

        private ChessPieceAtPos[] getPiecesAtPos(ChessPieceType type, ChessColor color)
        {
            // get bitboard
            byte index = (byte)(((int)type - 1) + ((int)color * 6));
            ulong bitboard = _bitboards[index];

            // get all positions containing pieces from the bitboard (max. 8 pieces)
            var posCache = new CachedChessPositions(bitboard);
            var positions = posCache.Positions;

            // TODO: implement logic
            throw new NotImplementedException();
        }

        #region IChessBoard

        public ChessPiece GetPieceAt(ChessPosition pos) => GetPieceAt((byte)pos.GetHashCode());

        public ChessPiece GetPieceAt(byte pos)
        {
            ChessPiece piece = ChessPiece.NULL;

            // only create a chess piece the board is captured at the given position
            if (IsCapturedAt(pos))
            {
                ChessPieceType type = ChessPieceType.Invalid;
                ChessColor color = ChessColor.White;

                // determine the piece type and color
                for (int i = 0; i < 12; i++)
                {
                    if (isSetAt(_bitboards[i], pos))
                    {
                        type = (ChessPieceType)((i % 6) + 1);
                        color = (ChessColor)(i / 6);
                    }
                }

                piece = new ChessPiece(type, color, isSetAt(_bitboards[12], pos));
            }

            return piece;
        }

        public bool IsCapturedAt(ChessPosition pos) => IsCapturedAt((byte)pos.GetHashCode());

        public bool IsCapturedAt(byte pos)
        {
            ulong mask = 0x1uL << pos;

            // combine all bitboards to one bitboard by bitwise OR
            ulong allPieces = _bitboards[0] | _bitboards[1] | _bitboards[2] | _bitboards[3] | _bitboards[4] | _bitboards[5]
                | _bitboards[6] | _bitboards[7] | _bitboards[8] | _bitboards[9] | _bitboards[10] | _bitboards[11];

            return (allPieces & mask) > 0;
        }

        public IEnumerable<ChessPieceAtPos> GetPiecesOfColor(ChessColor side)
        {
            // TODO: implement logic
            throw new NotImplementedException();
        }

        public ChessBoard ApplyDraw(ChessDraw draw)
        {
            // TODO: implement logic
            throw new NotImplementedException();
        }

        public ChessBoard ApplyDraws(IList<ChessDraw> draws)
        {
            // TODO: implement logic
            throw new NotImplementedException();
        }

        #endregion IChessBoard

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
                ulong bitboard = _bitboards[i];

                // loop through all positions
                for (byte pos = 0; pos < 64; pos++)
                {
                    // write piece to array if there is one
                    if ((bitboard & 0x0000000000000001uL) > 0) { pieces[pos] = new ChessPiece(pieceType, color, isSetAt(_bitboards[12], pos)); }

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
            // loop through all bitboards
            for (byte i = 0; i < 12; i++)
            {
                // determine the chess piece type and color of the iteration
                var pieceType = (ChessPieceType)((i % 6) + 1);
                var color = (ChessColor)(i / 6);

                // init empty bitboard
                ulong bitboard = 0;

                // loop through all positions
                for (byte pos = 0; pos < 64; pos++)
                {
                    // set piece bit if the position is captured
                    ulong mask = 0x1uL << pos;
                    bool setBit = board.IsCapturedAt(pos) && board.GetPieceAt(pos).Type == pieceType && board.GetPieceAt(pos).Color == color;
                    bitboard |= setBit ? mask : 0x0uL;
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

                wasMoved |= board.IsCapturedAt(whitePos) && board.GetPieceAt(whitePos).WasMoved ? 0x1uL << whitePos : 0x0uL;
                wasMoved |= board.IsCapturedAt(blackPos) && board.GetPieceAt(blackPos).WasMoved ? 0x1uL << blackPos : 0x0uL;
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
            var draws = new ulong[0];

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
                    switch (pieceType)
                    {
                        case ChessPieceType.King:    draws = draws.ArrayConcat(getKingDraws(drawingSide));              break;
                        case ChessPieceType.Queen:   draws = draws.ArrayConcat(getQueenDraws(drawingSide));             break;
                        case ChessPieceType.Rook:    draws = draws.ArrayConcat(getRookDraws(drawingSide));              break;
                        case ChessPieceType.Bishop:  draws = draws.ArrayConcat(getBishopDraws(drawingSide));            break;
                        case ChessPieceType.Knight:  draws = draws.ArrayConcat(getKnightDraws(drawingSide));            break;
                        case ChessPieceType.Peasant: draws = draws.ArrayConcat(getPeasantDraws(drawingSide, lastDraw)); break;
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

        #region King

        private ulong[] getKingDraws(ChessColor side)
        {
            // get the kings bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[offset];

            // determine the position of the piece onto the bitboard
            byte oldPos = (byte)BitOperations.Log2(bitboard);
            // TODO: check if this is too costly

            // get king draws
            ulong standardDraws = 0x0uL;

            if (side == ChessColor.White)
            {
                // TODO: add validation
                standardDraws |= bitboard << 9;
                standardDraws |= bitboard << 8;
                standardDraws |= bitboard << 7;
                standardDraws |= bitboard << 1;
                standardDraws |= bitboard >> 1;
                standardDraws |= bitboard >> 9;
                standardDraws |= bitboard >> 8;
                standardDraws |= bitboard >> 7;
            }
            else
            {
                // TODO: add validation
                standardDraws |= bitboard << 9;
                standardDraws |= bitboard << 8;
                standardDraws |= bitboard << 7;
                standardDraws |= bitboard << 1;
                standardDraws |= bitboard >> 1;
                standardDraws |= bitboard >> 9;
                standardDraws |= bitboard >> 8;
                standardDraws |= bitboard >> 7;
            }

            // TODO: implement validation of board edge overflows (e.g. use something like greater than / smaller than comparison
            // 1) do line check with the position

            // TODO: implement logic
            return new ulong[0];
        }

        #endregion King

        #region Queen

        private ulong[] getQueenDraws(ChessColor side)
        {
            return getRookDraws(side, 1).ArrayConcat(getBishopDraws(side, 1));
        }

        #endregion Queen

        private ulong[] getRookDraws(ChessColor side, byte bitboardIndex = 2)
        {
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // TODO: implement logic
            return new ulong[0];
        }

        private ulong[] getBishopDraws(ChessColor side, byte bitboardIndex = 3)
        {
            // get bishops bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // use even / uneven bitmasks
            ulong evenMask = 0x55AA55AA55AA55AAuL;
            ulong unevenMask = 0xAA55AA55AA55AA55uL;

            // TODO: implement logic
            return new ulong[0];
        }

        private ulong[] getKnightDraws(ChessColor side)
        {
            // get bishops bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[4 + offset];

            // determine the positions of the knights
            var positions = getPositions(bitboard);

            // TODO: compute all knight draws

            return new ulong[0];
        }

        private ulong[] getPeasantDraws(ChessColor side, ChessDraw? lastDraw = null)
        {
            var draws = new ulong[4];

            // get peasants bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[5 + offset];

            // get all fields captured by enemy pieces as bitboard
            ulong alliedPieces = getCapturedFields(side);
            ulong enemyPieces = getCapturedFields(side.Opponent());
            ulong blockingPieces = alliedPieces & enemyPieces;
            ulong enemyPeasants = _bitboards[5 + (byte)side.Opponent() * 6];

            bool checkForEnPassant = (lastDraw != null && lastDraw.Value.DrawingPieceType == ChessPieceType.Peasant 
                && Math.Abs(lastDraw.Value.OldPosition.Row - lastDraw.Value.NewPosition.Row) == 2);

            if (side == ChessColor.White)
            {
                // 1) get en-passant mask (mask has bits set at level=5, only if last draw is a two-foreward peasant draw, otherwise mask is zero)
                // 2) add an additional enemy peasant one field backwards, so the en-passant gets handled by the standard catch logic
                ulong enPassantMask = checkForEnPassant ? ((ROW_5 & (COL_A & COL_C)) << (lastDraw.Value.NewPosition.Column - 1)) & ROW_5 : 0uL;
                bool isEnPassant = (bitboard & enPassantMask) > 0;
                ulong additionalPeasantMask = isEnPassant ? (enemyPeasants >> 8) & (COL_A << lastDraw.Value.NewPosition.Column) & ROW_3 : 0L;
                enemyPieces |= additionalPeasantMask;

                // get one-foreward / two-foreward draws
                draws[0] = (bitboard << 8) & ~blockingPieces;
                draws[1] = ((bitboard & ROW_2) << 16) & (~blockingPieces | (~blockingPieces << 8));

                // get right / left catch draws (including en-passant)
                draws[2] = (bitboard << 9) & ~COL_H & enemyPieces;
                draws[3] = (bitboard << 7) & ~COL_A & enemyPieces;
            }
            else
            {
                // 1) get en-passant mask (mask has bits set at level=5, only if last draw is a two-foreward peasant draw, otherwise mask is zero)
                // 2) add an additional enemy peasant one field backwards, so the en-passant gets handled by the standard catch logic
                ulong enPassantMask = checkForEnPassant ? ((ROW_4 & (COL_A & COL_C)) << (lastDraw.Value.NewPosition.Column - 1)) & ROW_4 : 0uL;
                bool isEnPassant = (bitboard & enPassantMask) > 0;
                ulong additionalPeasantMask = isEnPassant ? (enemyPeasants << 8) & (COL_A << lastDraw.Value.NewPosition.Column) & ROW_6 : 0L;
                enemyPieces |= additionalPeasantMask;

                // get one-foreward / two-foreward draws
                draws[0] = (bitboard >> 8) & ~blockingPieces;
                draws[1] = ((bitboard & ROW_7) >> 16) & (~blockingPieces | (~blockingPieces >> 8));

                // get right / left catch draws (including en-passant)
                draws[2] = (bitboard >> 7) & ~COL_A & enemyPieces;
                draws[3] = (bitboard >> 9) & ~COL_H & enemyPieces;
            }

            return draws;
        }

        private ulong getCapturedFields(ChessColor side)
        {
            byte offset = (byte)((byte)side * 6);
            return _bitboards[offset] | _bitboards[offset + 1] | _bitboards[offset + 2] | _bitboards[offset + 3] | _bitboards[offset + 4] | _bitboards[offset + 5];
        }

        private ChessPosition[] getPositions(ulong bitboard, bool unlimited = false)
        {
            // generic approach for boards with more than 10 pieces
            if (unlimited)
            {
                // init position cache for worst-case
                var posCache = new ChessPosition[32];
                byte count = 0;

                // loop through all bits of the board
                for (byte pos = 0; pos < 64; pos++)
                {
                    bool isSet = (bitboard & 0x1uL) > 0;
                    if (isSet) { posCache[count++] = new ChessPosition(pos); }
                }

                // return the resulting array (without empty entries)
                return posCache.SubArray(0, count);
            }
            // optimized approach for boards with max. 10 pieces
            else
            {
                // get positions using the compact positions format
                var posCache = new CachedChessPositions(bitboard);
                return posCache.Positions;
            }
        }

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
    //    /// Compute the log2(x) function for the given ulong value. The result is rounded to floor if more than one bit is set.
    //    /// </summary>
    //    /// <param name="value">The value to be evaluated.</param>
    //    /// <returns>log2(value), rounded to the floor.</returns>
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
