using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess board and all fields / pieces on it.
    /// </summary>
    public class ChessBoard
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of a chess board in start position.
        /// </summary>
        public ChessBoard()
        {
            // init fields and pieces
            Fields = getChessFieldsInStartPosition();
            Pieces = Fields.GetFields1D().Where(field => field.IsCapturedByPiece).Select(field => field.Piece).ToList();
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
        public ChessField[,] Fields { get; }

        /// <summary>
        /// A list of all chess pieces that are currently on the chess board.
        /// </summary>
        public List<ChessPiece> Pieces { get; }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPiece WhiteKing { get { return Pieces.Where(x => x.Color == ChessPieceColor.White && x.Type == ChessPieceType.King).First(); } }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPiece BlackKing { get { return Pieces.Where(x => x.Color == ChessPieceColor.Black && x.Type == ChessPieceType.King).First(); } }
        
        #endregion Members

        #region Methods

        #region ChessFieldsPreparation

        private ChessField[,] getChessFieldsInStartPosition()
        {
            var fields = new ChessField[CHESS_BOARD_DIMENSION, CHESS_BOARD_DIMENSION];

            for (int row = 0; row < CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
                {
                    fields[row, column] = new ChessField() { Position = new ChessFieldPosition() { Row = row, Column = column }, Piece = null };
                }
            }

            // init pieces (black + white)
            initPeasants(ref fields);
            initHighValuePieces(ref fields);
            
            return fields;
        }

        private void initPeasants(ref ChessField[,] fields)
        {
            for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
            {
                // init white peasant of column
                fields[1, column] = new ChessField() {
                    Piece = new ChessPiece() { Color = ChessPieceColor.White, Type = ChessPieceType.Peasant, Board = this },
                    Position = new ChessFieldPosition() { Row = 1, Column = column }
                };

                // init black peasant of column
                fields[6, column] = new ChessField() {
                    Piece = new ChessPiece() { Color = ChessPieceColor.Black, Type = ChessPieceType.Peasant, Board = this },
                    Position = new ChessFieldPosition() { Row = 6, Column = column }
                };
            }
        }

        private void initHighValuePieces(ref ChessField[,] fields)
        {
            // execute this for each side (black + white)
            for (int row = 0; row < CHESS_BOARD_DIMENSION; row += 7)
            {
                // determine color of the chess pieces
                var color = (row == 0) ? ChessPieceColor.White : ChessPieceColor.Black;

                fields[row, 0] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 0 } };
                fields[row, 1] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 1 } };
                fields[row, 2] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 2 } };
                fields[row, 3] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Queen,  Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 3 } };
                fields[row, 4] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.King,   Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 4 } };
                fields[row, 5] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Bishop, Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 5 } };
                fields[row, 6] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Knight, Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 6 } };
                fields[row, 7] = new ChessField() { Piece = new ChessPiece() { Color = color, Type = ChessPieceType.Rock,   Board = this }, Position = new ChessFieldPosition() { Row = row, Column = 7 } };
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
                    char chessPieceColor = Fields[row, column].Piece != null ? (char)Fields[row, column].Piece.Color : ' ';
                    char chessPieceType = Fields[row, column].Piece != null ? (char)Fields[row, column].Piece.Type : ' ';
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
