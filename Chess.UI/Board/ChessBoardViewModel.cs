using Chess.Lib;
using Chess.UI.Field;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.UI.Board
{
    public class ChessBoardViewModel
    {
        #region Constructor

        public ChessBoardViewModel(Action<object> onFieldClicked, ChessBoard? board = null)
        {
            initFields(onFieldClicked);
            UpdatePieces(board ?? ChessBoard.StartFormation);
        }

        #endregion Constructor

        #region Members

        public ChessBoard Board { get; private set; }

        #region Fields

        private ChessFieldViewModel[,] _fields;

        // row 1
        public ChessFieldViewModel Field_A1 { get { return _fields[0, 0]; } }
        public ChessFieldViewModel Field_B1 { get { return _fields[0, 1]; } }
        public ChessFieldViewModel Field_C1 { get { return _fields[0, 2]; } }
        public ChessFieldViewModel Field_D1 { get { return _fields[0, 3]; } }
        public ChessFieldViewModel Field_E1 { get { return _fields[0, 4]; } }
        public ChessFieldViewModel Field_F1 { get { return _fields[0, 5]; } }
        public ChessFieldViewModel Field_G1 { get { return _fields[0, 6]; } }
        public ChessFieldViewModel Field_H1 { get { return _fields[0, 7]; } }

        // row 2
        public ChessFieldViewModel Field_A2 { get { return _fields[1, 0]; } }
        public ChessFieldViewModel Field_B2 { get { return _fields[1, 1]; } }
        public ChessFieldViewModel Field_C2 { get { return _fields[1, 2]; } }
        public ChessFieldViewModel Field_D2 { get { return _fields[1, 3]; } }
        public ChessFieldViewModel Field_E2 { get { return _fields[1, 4]; } }
        public ChessFieldViewModel Field_F2 { get { return _fields[1, 5]; } }
        public ChessFieldViewModel Field_G2 { get { return _fields[1, 6]; } }
        public ChessFieldViewModel Field_H2 { get { return _fields[1, 7]; } }

        // row 3
        public ChessFieldViewModel Field_A3 { get { return _fields[2, 0]; } }
        public ChessFieldViewModel Field_B3 { get { return _fields[2, 1]; } }
        public ChessFieldViewModel Field_C3 { get { return _fields[2, 2]; } }
        public ChessFieldViewModel Field_D3 { get { return _fields[2, 3]; } }
        public ChessFieldViewModel Field_E3 { get { return _fields[2, 4]; } }
        public ChessFieldViewModel Field_F3 { get { return _fields[2, 5]; } }
        public ChessFieldViewModel Field_G3 { get { return _fields[2, 6]; } }
        public ChessFieldViewModel Field_H3 { get { return _fields[2, 7]; } }

        // row 4
        public ChessFieldViewModel Field_A4 { get { return _fields[3, 0]; } }
        public ChessFieldViewModel Field_B4 { get { return _fields[3, 1]; } }
        public ChessFieldViewModel Field_C4 { get { return _fields[3, 2]; } }
        public ChessFieldViewModel Field_D4 { get { return _fields[3, 3]; } }
        public ChessFieldViewModel Field_E4 { get { return _fields[3, 4]; } }
        public ChessFieldViewModel Field_F4 { get { return _fields[3, 5]; } }
        public ChessFieldViewModel Field_G4 { get { return _fields[3, 6]; } }
        public ChessFieldViewModel Field_H4 { get { return _fields[3, 7]; } }

        // row 5
        public ChessFieldViewModel Field_A5 { get { return _fields[4, 0]; } }
        public ChessFieldViewModel Field_B5 { get { return _fields[4, 1]; } }
        public ChessFieldViewModel Field_C5 { get { return _fields[4, 2]; } }
        public ChessFieldViewModel Field_D5 { get { return _fields[4, 3]; } }
        public ChessFieldViewModel Field_E5 { get { return _fields[4, 4]; } }
        public ChessFieldViewModel Field_F5 { get { return _fields[4, 5]; } }
        public ChessFieldViewModel Field_G5 { get { return _fields[4, 6]; } }
        public ChessFieldViewModel Field_H5 { get { return _fields[4, 7]; } }

        // row 6
        public ChessFieldViewModel Field_A6 { get { return _fields[5, 0]; } }
        public ChessFieldViewModel Field_B6 { get { return _fields[5, 1]; } }
        public ChessFieldViewModel Field_C6 { get { return _fields[5, 2]; } }
        public ChessFieldViewModel Field_D6 { get { return _fields[5, 3]; } }
        public ChessFieldViewModel Field_E6 { get { return _fields[5, 4]; } }
        public ChessFieldViewModel Field_F6 { get { return _fields[5, 5]; } }
        public ChessFieldViewModel Field_G6 { get { return _fields[5, 6]; } }
        public ChessFieldViewModel Field_H6 { get { return _fields[5, 7]; } }

        // row 7
        public ChessFieldViewModel Field_A7 { get { return _fields[6, 0]; } }
        public ChessFieldViewModel Field_B7 { get { return _fields[6, 1]; } }
        public ChessFieldViewModel Field_C7 { get { return _fields[6, 2]; } }
        public ChessFieldViewModel Field_D7 { get { return _fields[6, 3]; } }
        public ChessFieldViewModel Field_E7 { get { return _fields[6, 4]; } }
        public ChessFieldViewModel Field_F7 { get { return _fields[6, 5]; } }
        public ChessFieldViewModel Field_G7 { get { return _fields[6, 6]; } }
        public ChessFieldViewModel Field_H7 { get { return _fields[6, 7]; } }

        // row 8
        public ChessFieldViewModel Field_A8 { get { return _fields[7, 0]; } }
        public ChessFieldViewModel Field_B8 { get { return _fields[7, 1]; } }
        public ChessFieldViewModel Field_C8 { get { return _fields[7, 2]; } }
        public ChessFieldViewModel Field_D8 { get { return _fields[7, 3]; } }
        public ChessFieldViewModel Field_E8 { get { return _fields[7, 4]; } }
        public ChessFieldViewModel Field_F8 { get { return _fields[7, 5]; } }
        public ChessFieldViewModel Field_G8 { get { return _fields[7, 6]; } }
        public ChessFieldViewModel Field_H8 { get { return _fields[7, 7]; } }

        #endregion Fields

        #endregion Members

        #region Methods

        private ChessFieldViewModel[,] initFields(Action<object> onFieldClicked)
        {
            var fields = new ChessFieldViewModel[8, 8];

            for (byte pos = 0; pos < 64; pos++)
            {
                fields[pos / 8, pos % 8] = new ChessFieldViewModel(new ChessPosition(pos), onFieldClicked);
            }

            return fields;
        }

        public void UpdatePieces(ChessBoard board)
        {
            for (byte pos = 0; pos < 64; pos++)
            {
                var field = _fields[pos / 8, pos % 8];
                field.UpdatePiece(board.IsCapturedAt(pos) ? (ChessPiece?)board.GetPieceAt(pos) : null);
            }

            Board = (ChessBoard)board.Clone();
        }

        #endregion Methods
    }
}
