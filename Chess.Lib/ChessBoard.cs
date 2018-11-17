﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess board and all fields / pieces on it.
    /// </summary>
    public class ChessBoard : ICloneable
    {
        #region Constants

        /// <summary>
        /// The dimension of the chess board (width / length) which is usually 8.
        /// </summary>
        public const int CHESS_BOARD_DIMENSION = 8;

        public static readonly List<short> START_FORMATION = new List<ChessPiece>()
        {
            new ChessPiece() { Position = new ChessFieldPosition("A1"), Type = ChessPieceType.Rock   , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("B1"), Type = ChessPieceType.Knight , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("C1"), Type = ChessPieceType.Bishop , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("D1"), Type = ChessPieceType.Queen  , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("E1"), Type = ChessPieceType.King   , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("F1"), Type = ChessPieceType.Bishop , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("G1"), Type = ChessPieceType.Knight , Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("H1"), Type = ChessPieceType.Rock   , Color = ChessPieceColor.White, WasAlreadyDrawn = false },

            new ChessPiece() { Position = new ChessFieldPosition("A2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("B2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("C2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("D2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("E2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("F2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("G2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("H2"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.White, WasAlreadyDrawn = false },

            new ChessPiece() { Position = new ChessFieldPosition("A7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("B7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("C7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("D7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("E7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("F7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("G7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("H7"), Type = ChessPieceType.Peasant, Color = ChessPieceColor.Black, WasAlreadyDrawn = false },

            new ChessPiece() { Position = new ChessFieldPosition("A8"), Type = ChessPieceType.Rock   , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("B8"), Type = ChessPieceType.Knight , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("C8"), Type = ChessPieceType.Bishop , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("D8"), Type = ChessPieceType.Queen  , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("E8"), Type = ChessPieceType.King   , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("F8"), Type = ChessPieceType.Bishop , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("G8"), Type = ChessPieceType.Knight , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },
            new ChessPiece() { Position = new ChessFieldPosition("H8"), Type = ChessPieceType.Rock   , Color = ChessPieceColor.Black, WasAlreadyDrawn = false },

        }.Select(x => (short)x.GetHashCode()).ToList();

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Create a new instance of a chess board in start position.
        /// </summary>
        public ChessBoard() : this(START_FORMATION) { }
        
        /// <summary>
        /// Create a deep copy of the given chess board. (clone constructor)
        /// </summary>
        /// <param name="original">The chess board to be cloned</param>
        public ChessBoard(ChessBoard original) : this(original.Pieces) { }

        /// <summary>
        /// Create a new instance of a chess board with the given chess pieces.
        /// </summary>
        /// <param name="pieces">The chess pieces to be applied to the chess board</param>
        public ChessBoard(IEnumerable<ChessPiece> pieces) : this(pieces.Select(x => (short)x.GetHashCode())) { }

        /// <summary>
        /// Create a new instance of a chess board with the given chess pieces (as short values).
        /// </summary>
        /// <param name="piecesAsHashCodes">The chess pieces to be applied to the chess board</param>
        public ChessBoard(IEnumerable<short> piecesAsHashCodes)
        {
            _pieces = new short?[CHESS_BOARD_DIMENSION * CHESS_BOARD_DIMENSION];

            foreach (var hashCode in piecesAsHashCodes)
            {
                var piece = new ChessPiece(hashCode);
                _pieces[piece.Position.GetHashCode()] = hashCode;
            }
        }

        #endregion Constructor

        #region Members
        
        /// <summary>
        /// An array of all chess pieces at the index of their position's hash code. (value is null if there is no chess piece at the position)
        /// </summary>
        private short?[] _pieces { get; }

        /// <summary>
        /// A list of all chess pieces that are currently on the chess board.
        /// </summary>
        public List<ChessPiece> Pieces { get { return _pieces.Where(x => x != null).Select(x => new ChessPiece(x.Value)).ToList(); } }

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public List<ChessPiece> WhitePieces { get { return Pieces.Where(x => x.Color == ChessPieceColor.White).ToList(); } }

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public List<ChessPiece> BlackPieces { get { return Pieces.Where(x => x.Color == ChessPieceColor.Black).ToList(); } }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPiece WhiteKing { get { return WhitePieces.Where(x => x.Type == ChessPieceType.King).First(); } }

        /// <summary>
        /// Selects the black king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPiece BlackKing { get { return BlackPieces.Where(x => x.Type == ChessPieceType.King).First(); } }

        #endregion Members

        #region Methods
        
        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsCapturedAt(ChessFieldPosition position)
        {
            return _pieces[position.GetHashCode()] != null;
        }

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece? GetPieceAt(ChessFieldPosition position)
        {
            return IsCapturedAt(position) ? (ChessPiece?)new ChessPiece(_pieces[position.GetHashCode()].Value) : null;
        }

        public void UpdatePieceAt(ChessFieldPosition position, ChessPiece? piece)
        {
            _pieces[position.GetHashCode()] = (short?)piece?.GetHashCode();
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        public void ApplyDraw(ChessDraw draw)
        {
            // get the destination chess field instance of the chess board
            var drawingPiece = GetPieceAt(draw.OldPosition).Value;
            var pieceToTake = GetPieceAt(draw.NewPosition);
            
            // update drawing piece data
            drawingPiece.Position = draw.NewPosition;
            drawingPiece.WasAlreadyDrawn = true;

            // apply data to the chess board
            UpdatePieceAt(draw.OldPosition, null);
            UpdatePieceAt(draw.NewPosition, drawingPiece);

            // TODO: implement rochade, en-passant, peasant promotion
        }
        
        /// <summary>
        /// Transform the current game situation of the chess board into a text format.
        /// 
        /// e.g. start position:
        /// 
        ///   -----------------------------------------
        /// 8 | BR | BH | BB | BQ | BK | BB | BH | BR |
        ///   -----------------------------------------
        /// 7 | BP | BP | BP | BP | BP | BP | BP | BP |
        ///   -----------------------------------------
        /// 6 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 5 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 4 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 3 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 2 | WP | WP | WP | WP | WP | WP | WP | WP |
        ///   -----------------------------------------
        /// 1 | WR | WH | WB | WQ | WK | WB | WH | WR |
        ///   -----------------------------------------
        ///     A    B    C    D    E    F    G    H
        /// </summary>
        /// <returns>a string representing the current game situation that can be printed e.g. to a CLI</returns>
        public override string ToString()
        {
            const string SEPARATING_LINE = "   -----------------------------------------";
            var builder = new StringBuilder(SEPARATING_LINE).AppendLine();

            for (int row = CHESS_BOARD_DIMENSION - 1; row >= 0; row--)
            {
                builder.Append($" { row + 1 } |");

                for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
                {
                    var position = new ChessFieldPosition(row, column);
                    var piece = GetPieceAt(position);

                    // TODO: try to use unicode chess symbols
                    char chessPieceColor = piece != null ? (char)piece.Value.Color.ToChar() : ' ';
                    char chessPieceType = piece != null ? piece.Value.Type.ToChar() : ' ';
                    builder.Append($" { chessPieceColor }{ chessPieceType } |");
                }

                builder.AppendLine();
                builder.AppendLine(SEPARATING_LINE);
            }

            builder.Append(" ");

            for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
            {
                builder.Append($"    { (char)('A' + column) }");
            }

            return builder.ToString();
        }

        public object Clone()
        {
            return new ChessBoard(this);
        }

        #endregion Methods
    }
}
