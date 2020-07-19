using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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

        // start formation positions mask
        private const ulong START_POSITIONS = 0xFFFF00000000FFFFul;

        // position masks for kings and rooks on the start formation
        private const ulong FIELD_A1 = 0x0000000000000001uL;
        private const ulong FIELD_E1 = FIELD_A1 << 4;
        private const ulong FIELD_H1 = FIELD_A1 << 7;
        private const ulong FIELD_A8 = FIELD_A1 << 56;
        private const ulong FIELD_E8 = FIELD_A1 << 60;
        private const ulong FIELD_H8 = FIELD_A1 << 63;

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
        /// <param name="ownsBitboard">Indicates whether the given bitboards copy is exclusively used by this instance. 
        /// If so, the copy operation can be saved by directly using the bitboards handed in.</param>
        // TODO: think about flagging this constructor private, so it cannot be used in the wrong way
        public ChessBitboard(ulong[] bitboards, bool ownsBitboard = false)
        {
            if (ownsBitboard)
            {
                // directly assign the bitboards, as this instance owns the copy that was handed in
                _bitboards = bitboards;
            }
            else
            {
                // initialize empty bitboards
                _bitboards = new ulong[13];

                // copy bitboards
                bitboards.CopyTo(_bitboards, 0);
            }
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
        public static ChessBitboard StartFormation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ChessBitboard(ChessBoard.StartFormation);
        }

        #region IChessBoard

        /// <summary>
        /// A list of all chess pieces (and their position) that are currently on the chess board. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> AllPieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((ChessPieceAtPos[])GetPiecesOfColor(ChessColor.White)).ArrayConcat((ChessPieceAtPos[])GetPiecesOfColor(ChessColor.Black));
        }

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> WhitePieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetPiecesOfColor(ChessColor.White);
        }

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> BlackPieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetPiecesOfColor(ChessColor.Black);
        }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos WhiteKing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => getKing(ChessColor.White);
        }

        /// <summary>
        /// Selects the black king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos BlackKing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => getKing(ChessColor.Black);
        }

        #endregion IChessBoard

        #endregion Members

        #region Methods

        #region IChessBoard

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece GetPieceAt(ChessPosition position) => GetPieceAt((byte)position.GetHashCode());

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece GetPieceAt(byte position)
        {
            ChessPiece piece = ChessPiece.NULL;

            // only create a chess piece the board is captured at the given position
            if (IsCapturedAt(position))
            {
                ChessPieceType type = ChessPieceType.Invalid;
                ChessColor color = ChessColor.White;

                // determine the piece type and color
                for (int i = 0; i < 12; i++)
                {
                    if (isSetAt(_bitboards[i], position))
                    {
                        type = (ChessPieceType)((i % 6) + 1);
                        color = (ChessColor)(i / 6);
                    }
                }

                piece = new ChessPiece(type, color, isSetAt(_bitboards[12], position));
            }

            return piece;
        }

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsCapturedAt(ChessPosition position) => IsCapturedAt((byte)position.GetHashCode());

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsCapturedAt(byte position)
        {
            ulong mask = 0x1uL << position;

            // combine all bitboards to one bitboard by bitwise OR
            ulong allPieces = _bitboards[0] | _bitboards[1] | _bitboards[2] | _bitboards[3] | _bitboards[4] | _bitboards[5]
                | _bitboards[6] | _bitboards[7] | _bitboards[8] | _bitboards[9] | _bitboards[10] | _bitboards[11];

            return (allPieces & mask) > 0;
        }

        /// <summary>
        /// Retrieve all chess pieces of the given player's side.
        /// </summary>
        /// <param name="side">The player's side</param>
        /// <returns>a list of all chess pieces of the given player's side</returns>
        public IEnumerable<ChessPieceAtPos> GetPiecesOfColor(ChessColor side)
        {
            // initialize the result cache with empty entries (max. 16 pieces per side)
            var piecesAtPos = new ChessPieceAtPos[16];

            // initialize the number of real content on the cache with 0
            byte count = 0;

            // determine the side offset
            byte offset = (byte)((byte)side * 6);
            
            // loop through all piece types for the given side
            for (byte i = 0; i < 6; i++)
            {
                // determine the board index
                byte boardIndex = (byte)(i + offset);

                // get pieces by board index and append them to the result
                var pieces = getPieces(boardIndex);
                pieces.CopyTo(piecesAtPos, count);

                // update the content count
                count += (byte)pieces.Length;
            }

            // trim the result cache, remove trailing empty entries
            return piecesAtPos.SubArray(0, count);
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        public IChessBoard ApplyDraw(ChessDraw draw)
        {
            // clone bitboards
            ulong[] bitboards = new ulong[13];
            _bitboards.CopyTo(bitboards, 0);

            // apply the draw to the cloned bitboard
            applyDraw(bitboards, draw);

            // return a new bitboard instance using the cloned bitboards
            return new ChessBitboard(bitboards, true);
        }

        /// <summary>
        /// Draw the chess pieces to the given positions on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draws">The chess draws to be executed</param>
        public IChessBoard ApplyDraws(IList<ChessDraw> draws)
        {
            // clone bitboards
            ulong[] bitboards = new ulong[13];
            _bitboards.CopyTo(bitboards, 0);

            // apply all draws to the bitboards copy
            for (int i = 0; i < draws.Count; i++)
            {
                applyDraw(bitboards, draws[i]);
            }

            // return a new bitboard instance using the cloned bitboards
            return new ChessBitboard(bitboards, true);
        }

        /// <summary>
        /// Flip all bits on the given bitboards to apply the given draw (calling this again with the same draw reverts everything).
        /// </summary>
        /// <param name="bitboards">The bitboards to apply the draw to.</param>
        /// <param name="draw">The draw to be applied (or reverted).</param>
        private void applyDraw(ulong[] bitboards, ChessDraw draw)
        {
            // TODO: check if this works

            // determine bitboard masks of the drawing piece's old and new position
            ulong oldPos = 0x1uL << draw.OldPosition.GetHashCode();
            ulong newPos = 0x1uL << draw.NewPosition.GetHashCode();

            // set was moved
            if (draw.IsFirstMove) { bitboards[12] ^= oldPos; }

            // determine the bitboard index of the drawing piece
            byte sideOffset = draw.DrawingSide.SideOffset();
            byte drawingBoardIndex = (byte)((byte)draw.DrawingPieceType - 1 + sideOffset);

            // move the drawing piece by flipping its' bits at the old and new position on the bitboard
            bitboards[drawingBoardIndex] ^= oldPos | newPos;

            // handle rochade: move casteling rook accordingly, king will be moved by standard logic
            if (draw.Type == ChessDrawType.Rochade)
            {
                // determine the rooks bitboard
                byte rooksBoardIndex = (byte)(2 + sideOffset);

                // move the casteling rook by filpping bits at its' old and new position on the bitboard
                bitboards[rooksBoardIndex] ^= 
                      ((newPos & COL_C) << 1) | ((newPos & COL_C) >> 2)  // big rochade
                    | ((newPos & COL_G) << 1) | ((newPos & COL_G) >> 1); // small rochade
            }

            // handle catching draw: remove caught enemy piece accordingly
            if (draw.TakenPieceType != null)
            {
                // determine the taken piece's bitboard
                byte takenPieceBitboardIndex = (byte)(draw.DrawingSide.Opponent().SideOffset() + draw.TakenPieceType.Value.PieceTypeOffset());

                // handle en-passant: remove enemy peasant accordingly, drawing peasant will be moved by standard logic
                if (draw.Type == ChessDrawType.EnPassant)
                {
                    // determine the white and black mask
                    ulong whiteMask = draw.DrawingSide.WhiteMask();
                    ulong blackMask = ~whiteMask;

                    // catch the enemy peasant by flipping the bit at his position
                    ulong targetColumn = COL_A << draw.NewPosition.Column;
                    bitboards[takenPieceBitboardIndex] ^=
                          (whiteMask & targetColumn & ROW_5)  // caught enemy white peasant
                        | (blackMask & targetColumn & ROW_4); // caught enemy black peasant
                }
                // handle normal catch: catch the enemy piece by flipping the bit at its' position on the bitboard
                else { bitboards[takenPieceBitboardIndex] ^= newPos; }
            }
            
            // handle peasant promotion: wipe peasant and put the promoted piece
            if (draw.PeasantPromotionPieceType != null)
            {
                // remove the peasant at the new position
                bitboards[drawingBoardIndex] ^= newPos;

                // put the promoted piece at the new position instead
                byte promotionBoardIndex = (byte)(sideOffset + draw.PeasantPromotionPieceType.Value.PieceTypeOffset());
                bitboards[promotionBoardIndex] ^= newPos;
            }
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
            byte sideOffset = drawingSide.SideOffset();
            var opponent = drawingSide.Opponent();

            // compute the draws for the pieces of each type
            var kingDraws    = getDraws(_bitboards, drawingSide, ChessPieceType.King, lastDraw);
            var queenDraws   = (_bitboards[sideOffset + 1] != 0x0uL) ? getDraws(_bitboards, drawingSide, ChessPieceType.Queen, lastDraw)   : new ChessDraw[0];
            var rookDraws    = (_bitboards[sideOffset + 2] != 0x0uL) ? getDraws(_bitboards, drawingSide, ChessPieceType.Rook, lastDraw)    : new ChessDraw[0];
            var bishopDraws  = (_bitboards[sideOffset + 3] != 0x0uL) ? getDraws(_bitboards, drawingSide, ChessPieceType.Bishop, lastDraw)  : new ChessDraw[0];
            var knightDraws  = (_bitboards[sideOffset + 4] != 0x0uL) ? getDraws(_bitboards, drawingSide, ChessPieceType.Knight, lastDraw)  : new ChessDraw[0];
            var peasantDraws = (_bitboards[sideOffset + 5] != 0x0uL) ? getDraws(_bitboards, drawingSide, ChessPieceType.Peasant, lastDraw) : new ChessDraw[0];

            // concat draws to one array
            var draws = kingDraws.ArrayConcat(queenDraws).ArrayConcat(rookDraws)
                .ArrayConcat(bishopDraws).ArrayConcat(knightDraws).ArrayConcat(peasantDraws);

            // if flag is active, filter only draws that do not cause draws into check
            if (analyzeDrawIntoCheck)
            {
                // make a working copy of all local bitboards
                var simBitboards = new ulong[13];
                _bitboards.CopyTo(simBitboards, 0);

                // init legal draws count with the amount of all draws (unvalidated)
                byte legalDrawsCount = (byte)draws.Length;

                // loop through draws and simulate each draw
                for (byte i = 0; i < legalDrawsCount; i++)
                {
                    // simulate the draw
                    applyDraw(simBitboards, draws[i]);

                    // calculate enemy answer draws (only fields that could be captured as one bitboard)
                    ulong enemyCapturableFields =
                          unionBitboards(getKingDrawBitboards(simBitboards, opponent, false))
                        | unionBitboards(getQueenDrawBitboards(simBitboards, opponent))
                        | unionBitboards(getRookDrawBitboards(simBitboards, opponent))
                        | unionBitboards(getBishopDrawBitboards(simBitboards, opponent))
                        | unionBitboards(getKnightDrawBitboards(simBitboards, opponent))
                        | unionBitboards(getPeasantDrawBitboards(simBitboards, opponent));

                    // revert the simulated draw (flip the bits back, this actually works LOL!!!)
                    applyDraw(simBitboards, draws[i]);
                    // TODO: test if cloning the board is actually faster than reverting the draw (use the better option)

                    // check if one of those draws would catch the allied king (bitwise AND) -> draw-into-check
                    if ((simBitboards[sideOffset] & enemyCapturableFields) > 0)
                    {
                        // overwrite the illegal draw with the last unevaluated draw in the array
                        draws[i--] = draws[--legalDrawsCount];
                    }
                }

                // remove illegal draws
                draws = draws.SubArray(0, legalDrawsCount);
            }

            return draws;
        }

        private ChessDraw[] getDraws(ulong[] bitboards, ChessColor side, ChessPieceType type, ChessDraw? lastDraw)
        {
            // get drawinh pieces
            byte index = (byte)(side.SideOffset() + type.PieceTypeOffset());
            var drawingPieces = getPositions(bitboards[index]);

            // init draws result set (max. draws)
            var draws = new ChessDraw[drawingPieces.Length * 28];
            byte count = 0;

            // loop through drawing pieces
            for (byte i = 0; i < drawingPieces.Length; i++)
            {
                var pos = (byte)drawingPieces[i].GetHashCode();

                // only set the drawing piece to the bitboard, wipe all others
                ulong filter = ~(0x1uL << pos);
                ulong[] pieceDrawsBoards;

                // compute the chess piece's capturable positions as bitboard
                switch (type)
                {
                    case ChessPieceType.King:    pieceDrawsBoards = getKingDrawBitboards(bitboards, side, true, filter);        break;
                    case ChessPieceType.Queen:   pieceDrawsBoards = getQueenDrawBitboards(bitboards, side, filter);             break;
                    case ChessPieceType.Rook:    pieceDrawsBoards = getRookDrawBitboards(bitboards, side, filter);              break;
                    case ChessPieceType.Bishop:  pieceDrawsBoards = getBishopDrawBitboards(bitboards, side, filter);            break;
                    case ChessPieceType.Knight:  pieceDrawsBoards = getKnightDrawBitboards(bitboards, side, filter);            break;
                    case ChessPieceType.Peasant: pieceDrawsBoards = getPeasantDrawBitboards(bitboards, side, filter, lastDraw); break;
                    default: throw new ArgumentException("Invalid chess piece type detected!");
                }

                // extract the capturable positions from bitboard
                ulong drawBitboard = unionBitboards(pieceDrawsBoards);
                var capturablePositions = getPositions(drawBitboard);

                // check for peasant promotion
                bool containsPeasantPromotion = type == ChessPieceType.Peasant 
                    && ((side == ChessColor.White && (drawBitboard & ROW_8) > 0) || (side == ChessColor.Black && (drawBitboard & ROW_1) > 0));

                // convert the positions into chess draws
                if (containsPeasantPromotion)
                {
                    for (byte j = 0; j < capturablePositions.Length; j++)
                    {
                        if ((side == ChessColor.White && (drawBitboard & ROW_8) > 0) || (side == ChessColor.Black && (drawBitboard & ROW_1) > 0))
                        {
                            // add types that the piece can promote to (queen, rook, bishop, knight)
                            for (byte pieceType = 2; pieceType < 6; pieceType++)
                            {
                                draws[count++] = new ChessDraw(this, drawingPieces[i], capturablePositions[j], (ChessPieceType)pieceType);
                            }
                        }
                        else
                        {
                            draws[count++] = new ChessDraw(this, drawingPieces[i], capturablePositions[j]);
                        }
                    }
                }
                else
                {
                    for (byte j = 0; j < capturablePositions.Length; j++)
                    {
                        draws[count++] = new ChessDraw(this, drawingPieces[i], capturablePositions[j]);
                    }
                }
            }

            return draws.SubArray(0, count);
        }

        #region King

        private ulong[] getKingDrawBitboards(ulong[] bitboards, ChessColor side, bool rochade, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL)
        {
            // determine standard and rochade draws
            ulong standardDraws = getStandardKingDrawBitboard(bitboards, side, drawingPiecesFilter);
            ulong rochadeDraws = rochade ? getRochadeKingDrawBitboard(bitboards, side, drawingPiecesFilter) : 0x0uL;

            return new ulong[] { standardDraws, rochadeDraws };
        }

        private ulong getStandardKingDrawBitboard(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL)
        {
            // get the king bitboard
            ulong bitboard = bitboards[side.SideOffset()];

            // determine allied pieces to eliminate blocked draws
            ulong alliedPieces = getCapturedFields(side);

            // compute all possible draws using bit-shift, moreover eliminate illegal overflow draws
            // info: the top/bottom comments are related to white-side perspective
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

        private ulong getRochadeKingDrawBitboard(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL)
        {
            // get the king and rook bitboard
            byte offset = side.SideOffset();
            ulong king = bitboards[offset];
            ulong rooks = bitboards[offset + 2];
            ulong wasMoved = bitboards[12];

            // filter by side
            ulong whiteMask = (ulong)((byte)side - 1);
            ulong blackMask = ~whiteMask;

            ulong draws =
                  (((king & FIELD_E1 & ~wasMoved) >> 2) & ((rooks & FIELD_A1 & ~wasMoved) << 3) & whiteMask)  // white big rochade
                | (((king & FIELD_E1 & ~wasMoved) << 2) & ((rooks & FIELD_H1 & ~wasMoved) >> 2) & whiteMask)  // white small rochade
                | (((king & FIELD_E8 & ~wasMoved) >> 2) & ((rooks & FIELD_A8 & ~wasMoved) << 3) & blackMask)  // black big rochade
                | (((king & FIELD_E8 & ~wasMoved) >> 2) & ((rooks & FIELD_H8 & ~wasMoved) >> 2) & blackMask); // black small rochade

            return draws;
        }

        #endregion King

        #region Queen

        private ulong[] getQueenDrawBitboards(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL)
        {
            return getRookDrawBitboards(bitboards, side, drawingPiecesFilter, 1).ArrayConcat(getBishopDrawBitboards(bitboards, side, drawingPiecesFilter, 1));
        }

        #endregion Queen

        #region Rook

        private ulong[] getRookDrawBitboards(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL, byte bitboardIndex = 2)
        {
            // TODO: test this logic!!!

            var draws = new ulong[4];

            // get the bitboard
            ulong bitboard = bitboards[bitboardIndex + side.SideOffset()];

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

                // handle catches the same way as overflow / collision detection (this has to be done afterwards 
                // as the catches are legal draws that need to occur onto the result bitboard)
                bRooks ^= ((bRooks >> (i * 8)) & enemyPieces) << (i * 8); // bottom
                lRooks ^= ((lRooks >> (i * 1)) & enemyPieces) << (i * 1); // left
                rRooks ^= ((rRooks << (i * 1)) & enemyPieces) >> (i * 1); // right
                tRooks ^= ((tRooks << (i * 8)) & enemyPieces) >> (i * 8); // top
            }

            return draws;
        }

        #endregion Rook

        #region Bishop

        private ulong[] getBishopDrawBitboards(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL, byte bitboardIndex = 3)
        {
            // TODO: test this logic!!!

            var draws = new ulong[4];

            // get the bitboard
            ulong bitboard = bitboards[bitboardIndex + side.SideOffset()];

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
                draws[0] |= brBishops >> (i * 7);
                draws[1] |= blBishops >> (i * 9);
                draws[2] |= trBishops << (i * 9);
                draws[3] |= tlBishops << (i * 7);

                // handle catches the same way as overflow / collision detection (this has to be done afterwards 
                // as the catches are legal draws that need to occur onto the result bitboard)
                brBishops ^= ((brBishops >> (i * 7)) & enemyPieces) << (i * 7); // bottom right
                blBishops ^= ((blBishops >> (i * 9)) & enemyPieces) << (i * 9); // bottom left
                trBishops ^= ((trBishops << (i * 9)) & enemyPieces) >> (i * 9); // top right
                tlBishops ^= ((tlBishops << (i * 7)) & enemyPieces) >> (i * 7); // top left
            }

            return draws;
        }

        #endregion Bishop

        #region Knight

        private ulong[] getKnightDrawBitboards(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL)
        {
            // TODO: test this logic!!!

            // get bishops bitboard
            ulong bitboard = bitboards[4 + side.SideOffset()];

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

            return new ulong[] { draws };
        }

        #endregion Knight

        #region Peasant

        private ulong[] getPeasantDrawBitboards(ulong[] bitboards, ChessColor side, ulong drawingPiecesFilter = 0xFFFFFFFFFFFFFFFFuL, ChessDraw? lastDraw = null)
        {
            // TODO: test this logic!!!

            var draws = new ulong[4];

            // get peasants bitboard
            ulong bitboard = bitboards[side.SideOffset() + 5];

            // get all fields captured by enemy pieces as bitboard
            ulong alliedPieces = getCapturedFields(side);
            ulong enemyPieces = getCapturedFields(side.Opponent());
            ulong blockingPieces = alliedPieces | enemyPieces;
            ulong enemyPeasants = bitboards[side.Opponent().SideOffset() + 5];

            // TODO: express this using bitwise operations, e.g. shifting COL_A to the column of the column of the last drawing peasant that is standing onto level 4 and drew 2 levels
            bool checkForEnPassant = (lastDraw != null && lastDraw.Value.DrawingPieceType == ChessPieceType.Peasant 
                && Math.Abs(lastDraw.Value.OldPosition.Row - lastDraw.Value.NewPosition.Row) == 2);

            // TODO: eliminate the if/else for side-dependent logic: use side mask, shift bits depending on side value (0 / 1), ...
            // TODO: implement en-passant with only bitwise commands, no if/else branching

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

            return draws;
        }

        #endregion Peasant

        #endregion DrawGen

        #region Helpers

        private ulong unionBitboards(ulong[] bitboards)
        {
            ulong result = 0x0uL;

            for (byte i = 0; i < bitboards.Length; i++)
            {
                result |= bitboards[i];
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong getCapturedFields(ChessColor side)
        {
            byte offset = (byte)((byte)side * 6);
            return _bitboards[offset] | _bitboards[offset + 1] | _bitboards[offset + 2] | _bitboards[offset + 3] | _bitboards[offset + 4] | _bitboards[offset + 5];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte getPosition(ulong bitboard)
        {
            // this returns the numeric value of the highest bit set on the given bitboard
            // if the given bitboard has multiple bits set, only the position of the highest bit is returned
            return (byte)BitOperations.Log2(bitboard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChessPieceAtPos getKing(ChessColor side)
        {
            // get the position of the king and whether he was already moved
            byte pos = getPosition(_bitboards[(byte)side * 6]);
            bool wasMoved = isSetAt(_bitboards[12], pos);

            // put everything together (this already uses bitwise operations, so no further optimizations required)
            return new ChessPieceAtPos(new ChessPosition(pos), new ChessPiece(ChessPieceType.King, side, wasMoved));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isSetAt(ulong bitboard, byte pos)
        {
            ulong mask = 0x1uL << pos;
            return (bitboard & mask) > 0;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private ulong setBitAt(ulong bitboard, byte pos)
        //{
        //    ulong mask = 0x1uL << pos;
        //    return bitboard | mask;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private ChessPieceAtPos[] getPieces(ChessPieceType type, ChessColor color)
        //{
        //    // determine the bitboard by piece type and color
        //    byte index = (byte)(((int)type - 1) + ((int)color * 6));
        //    ulong bitboard = _bitboards[index];

        //    // get all positions containing pieces from the bitboard (max. 8 pieces)
        //    var posCache = new CachedChessPositions(bitboard);
        //    var positions = posCache.Positions;

        //    var piecesAtPos = new ChessPieceAtPos[8];

        //    for (byte i = 0; i < positions.Length; i++)
        //    {
        //        var pos = positions[i];
        //        var piece = new ChessPiece(type, color, isSetAt(_bitboards[12], (byte)pos.GetHashCode()));
        //        piecesAtPos[i] = new ChessPieceAtPos(pos, piece);
        //    }

        //    return piecesAtPos.SubArray(0, positions.Length);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChessPieceAtPos[] getPieces(byte boardIndex)
        {
            // load the bitboard and determine the piece type and color
            ulong bitboard = _bitboards[boardIndex];
            ChessPieceType type = (ChessPieceType)((boardIndex % 6) + 1);
            ChessColor color = (ChessColor)(boardIndex / 6);

            // get all positions containing pieces from the bitboard (max. 8 pieces)
            var posCache = new CachedChessPositions(bitboard);
            var positions = posCache.Positions;

            var piecesAtPos = new ChessPieceAtPos[8];

            for (byte i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                var piece = new ChessPiece(type, color, isSetAt(_bitboards[12], (byte)pos.GetHashCode()));
                piecesAtPos[i] = new ChessPieceAtPos(pos, piece);
            }

            return piecesAtPos.SubArray(0, positions.Length);
        }

        #endregion Helpers

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

    /// <summary>
    /// An extension providing bitboard helper functions regarding the chess piece type.
    /// </summary>
    public static class ChessPieceTypeBitwiseEx
    {
        #region Methods

        /// <summary>
        /// Return the piece type offset for the bitboard index considering the given piece type.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>a side-dependent index offset for accessing the bitboard.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte PieceTypeOffset(this ChessPieceType type)
        {
            return (byte)((byte)type - 1);
        }

        #endregion Methods
    }

    /// <summary>
    /// An extension providing bitwise helper functions regarding the chess color.
    /// </summary>
    public static class ChessColorBitwiseEx
    {
        #region Methods

        /// <summary>
        /// Return the side offset for the bitboard index considering the given side.
        /// </summary>
        /// <param name="side">The side to evaluate.</param>
        /// <returns>a side-dependent index offset for accessing the bitboard.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SideOffset(this ChessColor side)
        {
            return (byte)((byte)side * 6);
        }

        /// <summary>
        /// Retrieve a bitboard mask by color. IF the given color is white, 
        /// return a bitboard with all bits set to 1, otherwise all bits set to 0.
        /// </summary>
        /// <param name="color">The color to be evaluated.</param>
        /// <returns>either a bitboard with either all bits set to 0 or all bits set to 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong WhiteMask(this ChessColor color)
        {
            // use a signed long subtraction's overflow:
            //   - if color=0: result is 2-complementary representation of -1, which has all bits set to 1
            //   - if color=1: result is 2-complementary representation of 0, which has all bits set to 0
            return (ulong)((byte)color - 1L);
        }

        /// <summary>
        /// Retrieve a bitboard mask by color. IF the given color is black, 
        /// return a bitboard with all bits set to 1, otherwise all bits set to 0.
        /// </summary>
        /// <param name="color">The color to be evaluated.</param>
        /// <returns>either a bitboard with either all bits set to 0 or all bits set to 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BlackMask(this ChessColor color)
        {
            // use logic from white mask and flip all bits, as checking for black is the exact opposite
            return ~WhiteMask(color);
        }

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
