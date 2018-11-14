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
        /// Creates a new instance of a chess board in start position.
        /// </summary>
        public ChessBoard()
        {
            // init fields and pieces
            Fields2D = getChessFieldsInStartPosition();
            Fields = Fields2D.GetFields1D().ToDictionary(x => x.Position);
            Pieces = Fields2D.GetFields1D().Where(field => field.IsCapturedByPiece).Select(field => field.Piece).ToList();
        }

        /// <summary>
        /// Creates a deep copy of the given chess board. (clone constructor)
        /// </summary>
        /// <param name="original">the fields to be applied to the board</param>
        public ChessBoard(ChessBoard original)
        {
            // create a deep copy of the given fields
            var copy = original.Fields.ToList().Select(x => x.Value.Clone() as ChessField).ToDictionary(x => x.Position);

            // init fields and pieces
            Fields = copy;
            Fields2D = new ChessField[CHESS_BOARD_DIMENSION, CHESS_BOARD_DIMENSION];
            Fields.ToList().ForEach(x => Fields2D[x.Key.Row, x.Key.Column] = x.Value);
            Pieces = Fields2D.GetFields1D().Where(field => field.IsCapturedByPiece).Select(field => field.Piece).ToList();
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The dimension of the chess board (width / length) which is usually 8.
        /// </summary>
        public const int CHESS_BOARD_DIMENSION = 8;

        /// <summary>
        /// A 2D array (8 x 8) of chess fields.
        /// </summary>
        public ChessField[,] Fields2D { get; }

        /// <summary>
        /// A dictionary of all chess fields that can be accessed by a chess field position object.
        /// </summary>
        public Dictionary<ChessFieldPosition, ChessField> Fields { get; }

        /// <summary>
        /// A list of all chess pieces that are currently on the chess board.
        /// </summary>
        public List<ChessPiece> Pieces { get; }

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

        #region ChessFieldsPreparation

        private ChessField[,] getChessFieldsInStartPosition()
        {
            var fields2D = new ChessField[CHESS_BOARD_DIMENSION, CHESS_BOARD_DIMENSION];

            for (int row = 0; row < CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
                {
                    fields2D[row, column] = new ChessField() { Position = new ChessFieldPosition(row, column), Piece = null };
                }
            }

            // init pieces (black + white)
            initPeasants(ref fields2D);
            initHighValuePieces(ref fields2D);
            
            return fields2D;
        }

        private void initPeasants(ref ChessField[,] fields2D)
        {
            for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
            {
                // init white peasant of column
                fields2D[1, column].Piece = new ChessPiece() { Color = ChessPieceColor.White, Type = ChessPieceType.Peasant, Board = this };

                // init black peasant of column
                fields2D[6, column].Piece = new ChessPiece() { Color = ChessPieceColor.Black, Type = ChessPieceType.Peasant, Board = this };
            }
        }

        private void initHighValuePieces(ref ChessField[,] fields2D)
        {
            // execute this for each side (black + white)
            for (int row = 0; row < CHESS_BOARD_DIMENSION; row += 7)
            {
                // determine color of the chess pieces
                var color = (row == 0) ? ChessPieceColor.White : ChessPieceColor.Black;

                fields2D[row, 0].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Board = this };
                fields2D[row, 1].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Board = this };
                fields2D[row, 2].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Board = this };
                fields2D[row, 3].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Queen,  Board = this };
                fields2D[row, 4].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.King,   Board = this };
                fields2D[row, 5].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Board = this };
                fields2D[row, 6].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Board = this };
                fields2D[row, 7].Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Board = this };
            }
        }

        #endregion ChessFieldsPreparation
        
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
            StringBuilder builder = new StringBuilder(SEPARATING_LINE).AppendLine();

            for (int row = CHESS_BOARD_DIMENSION - 1; row >= 0; row--)
            {
                builder.Append($" { row + 1 } |");

                for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
                {
                    // TODO: try to use unicode chess symbols
                    char chessPieceColor = Fields2D[row, column].Piece != null ? (char)Fields2D[row, column].Piece.Color : ' ';
                    char chessPieceType = Fields2D[row, column].Piece != null ? (char)Fields2D[row, column].Piece.Type : ' ';
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

    /// <summary>
    /// This class provides extension methods for handling 2D chess field arrays.
    /// </summary>
    public static class ChessFieldsArrayExtension
    {
        /// <summary>
        /// Retrieve the chess fields array (2D) as a list in 1D.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static List<ChessField> GetFields1D(this ChessField[,] fields)
        {
            var list1D = new List<ChessField>();

            for (int row = 0; row < ChessBoard.CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < ChessBoard.CHESS_BOARD_DIMENSION; column++)
                {
                    list1D.Add(fields[row, column]);
                }
            }

            return list1D;
        }
    }
}
