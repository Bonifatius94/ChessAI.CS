using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess board and all fields / pieces on it.
    /// </summary>
    public struct ChessBoard : ICloneable
    {
        #region Constants

        /// <summary>
        /// The dimension of the chess board (width / length) which is usually 8.
        /// </summary>
        public const int CHESS_BOARD_DIMENSION = 8;

        private static readonly List<ChessPiece> START_FORMATION = new List<ChessPiece>()
        {
            new ChessPiece() { Position = new ChessPosition("A1"), Type = ChessPieceType.Rook   , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("B1"), Type = ChessPieceType.Knight , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("C1"), Type = ChessPieceType.Bishop , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("D1"), Type = ChessPieceType.Queen  , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("E1"), Type = ChessPieceType.King   , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("F1"), Type = ChessPieceType.Bishop , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("G1"), Type = ChessPieceType.Knight , Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("H1"), Type = ChessPieceType.Rook   , Color = ChessColor.White, WasMoved = false },

            new ChessPiece() { Position = new ChessPosition("A2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("B2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("C2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("D2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("E2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("F2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("G2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("H2"), Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false },

            new ChessPiece() { Position = new ChessPosition("A7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("B7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("C7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("D7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("E7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("F7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("G7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("H7"), Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false },

            new ChessPiece() { Position = new ChessPosition("A8"), Type = ChessPieceType.Rook   , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("B8"), Type = ChessPieceType.Knight , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("C8"), Type = ChessPieceType.Bishop , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("D8"), Type = ChessPieceType.Queen  , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("E8"), Type = ChessPieceType.King   , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("F8"), Type = ChessPieceType.Bishop , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("G8"), Type = ChessPieceType.Knight , Color = ChessColor.Black, WasMoved = false },
            new ChessPiece() { Position = new ChessPosition("H8"), Type = ChessPieceType.Rook   , Color = ChessColor.Black, WasMoved = false },
        };

        #endregion Constants

        #region Constructor
        
        /// <summary>
        /// Create a new instance of a chess board with the given chess pieces.
        /// </summary>
        /// <param name="pieces">The chess pieces to be applied to the chess board</param>
        public ChessBoard(IEnumerable<ChessPiece> pieces)
        {
            _pieces = new ChessPiece?[CHESS_BOARD_DIMENSION * CHESS_BOARD_DIMENSION];
            
            foreach (var piece in pieces)
            {
                _pieces[piece.Position.GetHashCode()] = (ChessPiece)piece.Clone();
            }
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// An array of all chess pieces at the index of their position's hash code. (value is null if there is no chess piece at the position)
        /// </summary>
        private ChessPiece?[] _pieces;

        /// <summary>
        /// Retrieve a new chess board instance with start formation.
        /// </summary>
        public static ChessBoard StartFormation { get { return new ChessBoard(START_FORMATION); } }

        /// <summary>
        /// A list of all chess pieces that are currently on the chess board.
        /// </summary>
        public IEnumerable<ChessPiece> Pieces { get { return _pieces.Where(x => x != null).Select(x => x.Value); } }

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPiece> WhitePieces { get { return Pieces.Where(x => x.Color == ChessColor.White); } }

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPiece> BlackPieces { get { return Pieces.Where(x => x.Color == ChessColor.Black); } }

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
        public bool IsCapturedAt(ChessPosition position)
        {
            return _pieces[position.GetHashCode()] != null;
        }

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece? GetPieceAt(ChessPosition position)
        {
            return _pieces[position.GetHashCode()];
        }

        /// <summary>
        /// Update the chess piece at the given position.
        /// </summary>
        /// <param name="position">The position of the chess piece to be updated</param>
        /// <param name="newPiece">The new chess piece data</param>
        public void UpdatePieceAt(ChessPosition position, ChessPiece? newPiece)
        {
            _pieces[position.GetHashCode()] = newPiece;
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        public void ApplyDraw(ChessDraw draw)
        {
            // get the destination chess field instance of the chess board
            var drawingPiece = GetPieceAt(draw.OldPosition).Value;
            var pieceToTake = GetPieceAt(draw.NewPosition);
            
            // update drawing piece data
            drawingPiece.Position = draw.NewPosition;
            drawingPiece.WasMoved = true;

            // handle peasant promotion
            if (draw.Type == ChessDrawType.PeasantPromotion)
            {
                drawingPiece.Type = draw.PeasantPromotionPieceType.Value;
            }

            // handle rochade
            if (draw.Type == ChessDrawType.Rochade)
            {
                // get the rook involved and its old and new position
                var oldRookPosition = new ChessPosition(draw.NewPosition.Row, (draw.NewPosition.Column == 2) ? 0 : 7);
                var newRookPosition = new ChessPosition(draw.NewPosition.Row, (draw.NewPosition.Column == 2) ? 3 : 5);
                var drawingRook = GetPieceAt(oldRookPosition).Value;
                drawingRook.Position = newRookPosition;

                // move the tower
                UpdatePieceAt(oldRookPosition, null);
                UpdatePieceAt(newRookPosition, drawingRook);
            }

            // handle en-passant
            if (draw.Type == ChessDrawType.EnPassant)
            {
                // get position of the taken enemy peasant and remove it
                var takenPeasantPosition = new ChessPosition((draw.DrawingSide == ChessColor.White) ? 4 : 3, draw.NewPosition.Column);
                UpdatePieceAt(takenPeasantPosition, null);
            }

            // apply data to the chess board
            UpdatePieceAt(draw.OldPosition, null);
            UpdatePieceAt(draw.NewPosition, drawingPiece);
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
                    var position = new ChessPosition(row, column);
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

        /// <summary>
        /// Create a new chess board instance with the same game situation.
        /// </summary>
        /// <returns>a new chess board instance</returns>
        public object Clone()
        {
            return new ChessBoard(this.Pieces);
        }

        #endregion Methods
    }
}
