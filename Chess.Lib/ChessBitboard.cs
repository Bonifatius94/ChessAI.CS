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
    public struct ChessBitboard : IChessBoard, ICloneable
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

        // diagonal masks
        private const ulong WHITE_FIELDS = 0x55AA55AA55AA55AAuL;
        private const ulong BLACK_FIELDS = 0xAA55AA55AA55AA55uL;

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

        /// <summary>
        /// Create a new chess board instance from the given bitboards.
        /// </summary>
        /// <param name="bitboards">The bitboards containing the board data.</param>
        public ChessBitboard(ulong[] bitboards)
        {
            // initialize empty bitboards
            _bitboards = new ulong[13];

            // copy bitboards
            bitboards.CopyTo(_bitboards, 0);
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
            byte pos = getPosition(_bitboards[(byte)side * 6]);
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

        // TODO: implement an apply draw function that works onto the local instance, no immutable copy
        // TODO: implement a revert draw function (if this is even possible) -> speeds up draw-into-check validation a lot

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
                    if ((bitboard & 0x1) > 0) { pieces[pos] = new ChessPiece(pieceType, color, isSetAt(_bitboards[12], pos)); }

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
                    bool setBit = board.IsCapturedAt(pos) && board.GetPieceAt(pos).Type == pieceType && board.GetPieceAt(pos).Color == color;
                    bitboard |= setBit ? 0x1uL << pos : 0x0uL;
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
        /// Generate all draws of the given side. For validation purposes, the last draw made by the opponent and a flag
        /// whether legal checks for drawing into a check position should be considered can be passed as additional parameters.
        /// </summary>
        /// <param name="drawingSide">The side to draw.</param>
        /// <param name="lastDraw">The predeceding draw made by the opponent (null on game start).</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether legal checks for drawing into a check 
        /// position should be considered (if not set, illegal draws may be returned).</param>
        /// <returns>all possible chess draws for the given side</returns>
        public ChessDraw[] GetAllDraws(ChessColor drawingSide, ChessDraw? lastDraw = null, bool analyzeDrawIntoCheck = false)
        {
            // determine the drawing side
            byte sideOffset = (byte)((byte)drawingSide * 6);

            // compute the draws for the pieces of each type
            var kingDraws    = getKingDraws(drawingSide, analyzeDrawIntoCheck);
            var queenDraws   = (_bitboards[sideOffset + 1] != 0x0uL) ? getQueenDraws(drawingSide)             : new ChessDraw[0];
            var rookDraws    = (_bitboards[sideOffset + 2] != 0x0uL) ? getRookDraws(drawingSide)              : new ChessDraw[0];
            var bishopDraws  = (_bitboards[sideOffset + 3] != 0x0uL) ? getBishopDraws(drawingSide)            : new ChessDraw[0];
            var knightDraws  = (_bitboards[sideOffset + 4] != 0x0uL) ? getKnightDraws(drawingSide)            : new ChessDraw[0];
            var peasantDraws = (_bitboards[sideOffset + 5] != 0x0uL) ? getPeasantDraws(drawingSide, lastDraw) : new ChessDraw[0];

            // concat draws to one array
            var draws = kingDraws.ArrayConcat(queenDraws).ArrayConcat(rookDraws)
                .ArrayConcat(bishopDraws).ArrayConcat(knightDraws).ArrayConcat(peasantDraws);

            // if flag is active, filter only draws that do not cause draws into check
            if (analyzeDrawIntoCheck)
            {
                // put all draws together to one 
                for (byte i = 0; i < draws.Length; i++)
                {
                    var draw = draws[i];
                }

                // TODO: fasten up as good as possible
            }

            return draws;
        }

        #region King

        private ChessDraw[] getKingDraws(ChessColor side, bool rochade)
        {
            // determine standard and rochade draws
            ulong standardDraws = getStandardKingDraws(side);
            ulong rochadeDraws = rochade ? getRochadeKingDraws(side) : 0x0uL;

            // TODO: convert bitboard draws to ChessDraw format

            return new ChessDraw[0];
        }

        private ulong getStandardKingDraws(ChessColor side)
        {
            // get the king bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[offset];

            // determine allied pieces to eliminate blocked draws
            ulong alliedPieces = getCapturedFields(side);

            // compute all possible draws using bit-shift, moreover eliminate illegal overflow draws
            ulong standardDraws =
                  ((bitboard << 7) & ~(ROW_1 | COL_H | alliedPieces))  // top left
                | ((bitboard << 8) & ~(ROW_1         | alliedPieces))  // top mid
                | ((bitboard << 9) & ~(ROW_1 | COL_A | alliedPieces))  // top right
                | ((bitboard >> 1) & ~(COL_H         | alliedPieces))  // side left
                | ((bitboard << 1) & ~(COL_A         | alliedPieces))  // side right
                | ((bitboard >> 9) & ~(ROW_8 | COL_H | alliedPieces))  // bottom left
                | ((bitboard >> 8) & ~(ROW_8         | alliedPieces))  // bottom mid
                | ((bitboard >> 7) & ~(ROW_8 | COL_A | alliedPieces)); // bottom right

            return standardDraws;
        }

        private ulong getRochadeKingDraws(ChessColor side)
        {
            // init the masks for standard positions of kings and rooks
            const ulong MASK_A1 = 0x0000000000000001uL;
            const ulong MASK_E1 = MASK_A1 << 4;
            const ulong MASK_H1 = MASK_A1 << 7;
            const ulong MASK_A8 = MASK_A1 << 56;
            const ulong MASK_E8 = MASK_A1 << 60;
            const ulong MASK_H8 = MASK_A1 << 63;

            // get the king and rook bitboard
            byte offset = (byte)((byte)side * 6);
            ulong king = _bitboards[offset];
            ulong rooks = _bitboards[offset + 2];
            ulong wasMoved = _bitboards[12];

            // filter by side
            ulong whiteMask = (ulong)((byte)side - 1);
            ulong blackMask = ~whiteMask;

            ulong draws =
                  (((king & MASK_E1 & ~wasMoved) >> 2) & ((rooks & MASK_A1 & ~wasMoved) << 3) & whiteMask)  // white big rochade
                | (((king & MASK_E1 & ~wasMoved) << 2) & ((rooks & MASK_H1 & ~wasMoved) >> 2) & whiteMask)  // white small rochade
                | (((king & MASK_E8 & ~wasMoved) >> 2) & ((rooks & MASK_A8 & ~wasMoved) << 3) & blackMask)  // black big rochade
                | (((king & MASK_E8 & ~wasMoved) >> 2) & ((rooks & MASK_H8 & ~wasMoved) >> 2) & blackMask); // black small rochade

            return draws;
        }

        #endregion King

        #region Queen

        private ChessDraw[] getQueenDraws(ChessColor side)
        {
            return getRookDraws(side, 1).ArrayConcat(getBishopDraws(side, 1));
        }

        #endregion Queen

        private ChessDraw[] getRookDraws(ChessColor side, byte bitboardIndex = 2)
        {
            // TODO: test this logic!!!

            var draws = new ulong[4];

            // get the bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // determine allied and enemy pieces (for collision / catch handling)
            ulong enemyPieces = getCapturedFields(side.Opponent());
            ulong alliedPieces = getCapturedFields(side);

            // init empty draws bitboards, separated by field color
            ulong bRooks = bitboard;
            ulong lRooks = bitboard;
            ulong rRooks = bitboard;
            ulong tRooks = bitboard;

            // compute draws (try to apply 1-7 shifts in each direction)
            for (byte i = 1; i < 8; i++)
            {
                // simulate the computing of all draws:
                // if there would be one or more overflows / collisions with allied pieces, remove certain rooks 
                // from the rooks bitboard, so the overflow won't occur on the real draw computation afterwards
                bRooks ^= ((bRooks >> (i * 8)) & (ROW_8 | alliedPieces)) << (i * 8); // bottom
                lRooks ^= ((lRooks >> (i * 1)) & (COL_H | alliedPieces)) << (i * 1); // left
                rRooks ^= ((rRooks << (i * 1)) & (COL_A | alliedPieces)) >> (i * 1); // right
                tRooks ^= ((tRooks << (i * 8)) & (ROW_1 | alliedPieces)) >> (i * 8); // top

                // compute all legal draws and apply them to the result bitboard
                draws[0] |= bRooks >> (i * 8);
                draws[1] |= lRooks >> (i * 1);
                draws[2] |= rRooks << (i * 1);
                draws[3] |= tRooks << (i * 8);
                // TODO: think about splitting draws by direction, so they can be reversed by draw-into-chess detection

                // handle catches the same way as overflow / collision detection (this has to be done afterwards 
                // as the catches are legal draws that need to occur onto the result bitboard)
                bRooks ^= ((bRooks >> (i * 8)) & enemyPieces) << (i * 8); // bottom
                lRooks ^= ((lRooks >> (i * 1)) & enemyPieces) << (i * 1); // left
                rRooks ^= ((rRooks << (i * 1)) & enemyPieces) >> (i * 1); // right
                tRooks ^= ((tRooks << (i * 8)) & enemyPieces) >> (i * 8); // top
            }

            // TODO: convert bitboard draws to  ChessDraw format

            return new ChessDraw[0];
        }

        private ChessDraw[] getBishopDraws(ChessColor side, byte bitboardIndex = 3)
        {
            // TODO: test this logic!!!

            ulong draws = 0;

            // get the bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[bitboardIndex + offset];

            // determine allied and enemy pieces (for collision / catch handling)
            ulong enemyPieces = getCapturedFields(side.Opponent());
            ulong alliedPieces = getCapturedFields(side);

            // init empty draws bitboards, separated by field color
            ulong brBishops = bitboard;
            ulong blBishops = bitboard;
            ulong trBishops = bitboard;
            ulong tlBishops = bitboard;

            // compute draws (try to apply 1-7 shifts in each direction)
            for (byte i = 1; i < 8; i++)
            {
                // simulate the computing of all draws:
                // if there would be one or more overflows / collisions with allied pieces, remove certain bishops 
                // from the bishops bitboard, so the overflow won't occur on the real draw computation afterwards
                brBishops ^= ((brBishops >> (i * 7)) & (ROW_8 | COL_H | alliedPieces)) << (i * 7); // bottom right
                blBishops ^= ((blBishops >> (i * 9)) & (ROW_8 | COL_A | alliedPieces)) << (i * 9); // bottom left
                trBishops ^= ((trBishops << (i * 9)) & (ROW_1 | COL_A | alliedPieces)) >> (i * 9); // top right
                tlBishops ^= ((tlBishops << (i * 7)) & (ROW_1 | COL_H | alliedPieces)) >> (i * 7); // top left

                // compute all legal draws and apply them to the result bitboard
                draws |= brBishops >> (i * 7) | blBishops >> (i * 9) | trBishops << (i * 9) | tlBishops << (i * 7);
                // TODO: think about splitting draws by direction, so they can be reversed by draw-into-chess detection

                // handle catches the same way as overflow / collision detection (this has to be done afterwards 
                // as the catches are legal draws that need to occur onto the result bitboard)
                brBishops ^= ((brBishops >> (i * 7)) & enemyPieces) << (i * 7); // bottom right
                blBishops ^= ((blBishops >> (i * 9)) & enemyPieces) << (i * 9); // bottom left
                trBishops ^= ((trBishops << (i * 9)) & enemyPieces) >> (i * 9); // top right
                tlBishops ^= ((tlBishops << (i * 7)) & enemyPieces) >> (i * 7); // top left
            }

            // TODO: convert bitboard draws to  ChessDraw format

            return new ChessDraw[0];
        }

        private ChessDraw[] getKnightDraws(ChessColor side)
        {
            // TODO: test this logic!!!

            // get bishops bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[4 + offset];

            // determine allied pieces to eliminate blocked draws
            ulong alliedPieces = getCapturedFields(side);

            // compute all possible draws using bit-shift, moreover eliminate illegal overflow draws
            ulong draws =
                  ((bitboard <<  6) & ~(ROW_1 | COL_H | COL_G | alliedPieces))  // top left  (1-2)
                | ((bitboard << 10) & ~(ROW_1 | COL_A | COL_B | alliedPieces))  // top right (1-2)
                | ((bitboard << 15) & ~(ROW_1 | COL_H | ROW_2 | alliedPieces))  // top left  (2-1)
                | ((bitboard << 17) & ~(ROW_1 | COL_A | ROW_2 | alliedPieces))  // top right (2-1)
                | ((bitboard >> 10) & ~(ROW_8 | COL_H | COL_G | alliedPieces))  // bottom left  (1-2)
                | ((bitboard >>  6) & ~(ROW_8 | COL_A | COL_B | alliedPieces))  // bottom right (1-2)
                | ((bitboard >> 17) & ~(ROW_8 | COL_H | ROW_7 | alliedPieces))  // bottom left  (2-1)
                | ((bitboard >> 15) & ~(ROW_8 | COL_A | ROW_7 | alliedPieces)); // bottom right (2-1)

            // TODO: convert bitboard draws to  ChessDraw format

            return new ChessDraw[0];
        }

        private ChessDraw[] getPeasantDraws(ChessColor side, ChessDraw? lastDraw = null)
        {
            // TODO: test this logic!!!

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

            // TODO: eliminate the if/else for side-dependent logic: use side mask, shift bits depending on side value (0 / 1), ...
            // filter by side
            //ulong whiteMask = (ulong)((byte)side - 1);
            //ulong blackMask = ~whiteMask;

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
                draws[2] = ((bitboard & ~COL_H) << 9) & enemyPieces;
                draws[3] = ((bitboard & ~COL_A) << 7) & enemyPieces;
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
                draws[2] = ((bitboard & ~COL_A) << 7) & enemyPieces;
                draws[3] = ((bitboard & ~COL_H) << 9) & enemyPieces;
            }

            // TODO: convert bitboard draws to  ChessDraw format

            return new ChessDraw[0];
        }

        private ulong getCapturedFields(ChessColor side)
        {
            byte offset = (byte)((byte)side * 6);
            return _bitboards[offset] | _bitboards[offset + 1] | _bitboards[offset + 2] | _bitboards[offset + 3] | _bitboards[offset + 4] | _bitboards[offset + 5];
        }

        private ChessPosition[] getPositions(ulong bitboard)
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

        #endregion DrawGen

        #region Clone

        /// <summary>
        /// Create a deep clone of this bitboard instance.
        /// </summary>
        /// <returns>a deep clone of this instance.</returns>
        public object Clone()
        {
            return new ChessBitboard(_bitboards);
        }

        #endregion Clone

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
