using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    public struct ChessBitboard
    {
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
        public IEnumerable<ChessDraw> GetAllDraws(ChessDraw? lastDraw = null, bool analyzeDrawIntoCheck = false)
        {
            // initialize draws with empty list
            var draws = new List<ChessDraw>();

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
                        case ChessPieceType.King:    draws.AddRange(getKingDraws(drawingSide));              break;
                        case ChessPieceType.Queen:   draws.AddRange(getQueenDraws(drawingSide));             break;
                        case ChessPieceType.Rook:    draws.AddRange(getRookDraws(drawingSide));              break;
                        case ChessPieceType.Bishop:  draws.AddRange(getBishopDraws(drawingSide));            break;
                        case ChessPieceType.Knight:  draws.AddRange(getKnightDraws(drawingSide));            break;
                        case ChessPieceType.Peasant: draws.AddRange(getPeasantDraws(drawingSide, lastDraw)); break;
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

            // get king draws
            ulong forewardRight  = (side == ChessColor.White) ? bitboard << 9 : bitboard >> 7;
            ulong forewardMiddle = (side == ChessColor.White) ? bitboard << 8 : bitboard >> 8;
            ulong forewardLeft   = (side == ChessColor.White) ? bitboard << 7 : bitboard >> 9;
            ulong sideRight      = (side == ChessColor.White) ? bitboard << 1 : bitboard >> 1;
            ulong sideLeft       = (side == ChessColor.White) ? bitboard >> 1 : bitboard << 1;
            ulong backwardRight  = (side == ChessColor.White) ? bitboard >> 9 : bitboard << 7;
            ulong backwardMiddle = (side == ChessColor.White) ? bitboard >> 8 : bitboard << 8;
            ulong backwardLeft   = (side == ChessColor.White) ? bitboard >> 7 : bitboard << 9;
            // TODO: implement validation of board edge overflows
            // TODO: think of removing the checks for drawing side as draw bitboards are mirrored anyways

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
            ulong unevenMask = ~evenMask;

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private IEnumerable<ChessDraw> getKnightDraws(ChessColor side)
        {
            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private IEnumerable<ChessDraw> getPeasantDraws(ChessColor side, ChessDraw? lastDraw = null)
        {
            // get peasants bitboard
            byte offset = (byte)((byte)side * 6);
            ulong bitboard = _bitboards[5 + offset];

            // get all fields captured by enemy pieces as bitboard
            ulong enemyPieces = getAllCapturedFields(side.Opponent());

            // get one-foreward draws
            ulong bitboardOneFordward = (side == ChessColor.White) ? bitboard << 8 : bitboard >> 8;

            // get two-foreward draws
            ulong bitboardTwoFordward = (side == ChessColor.White) ? (bitboard & 0x00FF000000000000L) << 16 : (bitboard & 0x000000000000FF00L) >> 16;
            // TODO: implement error handling: peasant already at board's edge => apply a mask with bits only set for the given line, too far or too short draws get eliminated

            // get right catch draws
            ulong bitboardCatchRight = (side == ChessColor.White) ? bitboard << 9 : bitboard >> 7;

            // get left catch draws
            ulong bitboardCatchLeft = (side == ChessColor.White) ? bitboard << 7 : bitboard >> 9;

            // TODO: implement logic
            return new List<ChessDraw>();
        }

        private ulong getAllCapturedFields(ChessColor side)
        {
            // init result with bitboard of false values
            ulong ret = 0;
            byte offset = (byte)((byte)side * 6);

            // loop through all bitboards of the given side
            for (byte i = 0; i < 6; i++)
            {
                // apply occupied fields to the output by bitwise OR
                ret |= _bitboards[i + offset];
            }

            return ret;
        }

        #endregion DrawGen

        #endregion Methods
    }
}
