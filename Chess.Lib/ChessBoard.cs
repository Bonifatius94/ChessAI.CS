using System;
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
        #region Constructor

        /// <summary>
        /// Create a new instance of a chess board in start position.
        /// </summary>
        public ChessBoard()
        {
            // init fields and pieces
            Pieces = getChessPiecesInStartPosition();
            updatePiecesByPositon();
        }

        /// <summary>
        /// Create a deep copy of the given chess board. (clone constructor)
        /// </summary>
        /// <param name="original">The chess board to be cloned</param>
        public ChessBoard(ChessBoard original)
        {
            // create a deep copy of the given chess pieces
            Pieces = original.PiecesByPosition.ToList().Select(x => (ChessPiece)x.Value.Clone()).ToList();
            PiecesByPosition = Pieces.ToDictionary(x => x.Position);
        }

        #endregion Constructor

        #region Members
        
        /// <summary>
        /// The dimension of the chess board (width / length) which is usually 8.
        /// </summary>
        public const int CHESS_BOARD_DIMENSION = 8;
        
        /// <summary>
        /// A list of all chess pieces that are currently on the chess board.
        /// </summary>
        public List<ChessPiece> Pieces { get; }

        /// <summary>
        /// A dictionary of all chess fields that can be accessed by a chess field position object.
        /// </summary>
        public Dictionary<ChessFieldPosition, ChessPiece> PiecesByPosition { get; private set; }
        
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

        private void updatePiecesByPositon()
        {
            PiecesByPosition = Pieces.ToDictionary(x => x.Position);
        }

        #region ChessFieldsPreparation

        private List<ChessPiece> getChessPiecesInStartPosition()
        {
            var pieces = new List<ChessPiece>();

            // init the peasants (for both sides)
            for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
            {
                // get the positions of white and black peasants with the given column
                var posWhitePeasant = new ChessFieldPosition(1, column);
                var posBlackPeasant = new ChessFieldPosition(6, column);

                // create the peasants
                pieces.Add(new ChessPiece() { Color = ChessPieceColor.White, Type = ChessPieceType.Peasant, Position = posWhitePeasant, WasAlreadyDrawn = false });
                pieces.Add(new ChessPiece() { Color = ChessPieceColor.Black, Type = ChessPieceType.Peasant, Position = posBlackPeasant, WasAlreadyDrawn = false });
            }

            // init the high-value chess pieces (for both sides)
            for (int row = 0; row < CHESS_BOARD_DIMENSION; row += 7)
            {
                // determine the color of the chess pieces
                var color = (row == 0) ? ChessPieceColor.White : ChessPieceColor.Black;

                // create all high-value chess pieces for the given side
                pieces.AddRange(new List<ChessPiece>() {
                    new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Position = new ChessFieldPosition(row, 0), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Position = new ChessFieldPosition(row, 1), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Position = new ChessFieldPosition(row, 2), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Queen,  Position = new ChessFieldPosition(row, 3), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.King,   Position = new ChessFieldPosition(row, 4), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Position = new ChessFieldPosition(row, 5), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Position = new ChessFieldPosition(row, 6), WasAlreadyDrawn = false },
                    new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Position = new ChessFieldPosition(row, 7), WasAlreadyDrawn = false }
                });
            }

            return pieces;
        }
        
        #endregion ChessFieldsPreparation

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsFieldCaptured(ChessFieldPosition position)
        {
            return PiecesByPosition.ContainsKey(position);
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        public void ApplyDraw(ChessDraw draw)
        {
            // get the destination chess field instance of the chess board
            var drawingPiece = PiecesByPosition[draw.OldPosition];
            var pieceToTake = PiecesByPosition.GetValueOrDefault(draw.NewPosition);

            // take enemy piece (if there is one)
            if (pieceToTake != null && pieceToTake.Color != drawingPiece.Color)
            {
                Pieces.Remove(pieceToTake);
            }

            // move piece from original field to the destination
            drawingPiece.Position = draw.NewPosition;
            drawingPiece.WasAlreadyDrawn = true;

            // rebuild the pieces dictionary
            updatePiecesByPositon();
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
                    var piece = PiecesByPosition.GetValueOrDefault(position);

                    // TODO: try to use unicode chess symbols
                    char chessPieceColor = piece != null ? (char)piece.Color : ' ';
                    char chessPieceType = piece != null ? (char)piece.Type : ' ';
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
