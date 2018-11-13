using System;
using System.Text;

namespace ChessAI.Lib
{
    public class ChessBoard
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of a chess board in start position.
        /// </summary>
        public ChessBoard()
        {
            // init fields
            _fields = getChessFieldsInStartPosition();
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The dimension of the chess board (width / length) which is usually 8.
        /// </summary>
        public const int CHESS_BOARD_DIMENSION = 8;

        private ChessField[,] _fields;
        /// <summary>
        /// A 2D array (8 x 8) of chess fields.
        /// </summary>
        public ChessField[,] Fields { get { return _fields; } }

        #region ChessFieldsPreparation

        private ChessField[,] getChessFieldsInStartPosition()
        {
            var fields = new ChessField[CHESS_BOARD_DIMENSION, CHESS_BOARD_DIMENSION];

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

        #endregion Members

        #region Methods

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
            const string SEPARATING_LINE = "  -----------------------------------------";
            StringBuilder builder = new StringBuilder(SEPARATING_LINE).AppendLine();

            for (int row = 0; row < CHESS_BOARD_DIMENSION; row++)
            {
                builder.Append($"{ row + 1 } |");

                for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
                {
                    char chessPieceColor = _fields[row, column].Piece != null ? (char)_fields[row, column].Piece.Color : ' ';
                    char chessPieceType = _fields[row, column].Piece != null ? (char)_fields[row, column].Piece.Type : ' ';
                    builder.Append($" { chessPieceColor }{ chessPieceType } |");
                }
                
                builder.AppendLine(SEPARATING_LINE).AppendLine();
            }

            for (int column = 0; column < CHESS_BOARD_DIMENSION; column++)
            {
                builder.Append($"    { (char)('A' + column) }");
            }

            return builder.ToString();
        }

        #endregion Methods
    }
}
