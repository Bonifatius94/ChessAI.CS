/*
 * MIT License
 * 
 * Copyright(c) 2020 Marco Tröster
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Chess.Lib;
using Chess.UI.Field;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.UI.Board
{
    public class ChessBoardViewModel : PropertyChangedBase
    {
        #region Constructor

        public ChessBoardViewModel(Action<object> onFieldClicked, ChessBoard? board = null)
        {
            Fields = getInitializedFields(onFieldClicked);
            UpdatePieces(board ?? ChessBoard.StartFormation);
        }

        #endregion Constructor

        #region Members

        private IChessBoard _board;

        public bool IsEnabled { get; private set; } = false;

        #region Fields

        public ChessFieldViewModel[] Fields { get; private set; }

        // row 1
        public ChessFieldViewModel Field_A1 { get { return Fields[0];  } }
        public ChessFieldViewModel Field_B1 { get { return Fields[1];  } }
        public ChessFieldViewModel Field_C1 { get { return Fields[2];  } }
        public ChessFieldViewModel Field_D1 { get { return Fields[3];  } }
        public ChessFieldViewModel Field_E1 { get { return Fields[4];  } }
        public ChessFieldViewModel Field_F1 { get { return Fields[5];  } }
        public ChessFieldViewModel Field_G1 { get { return Fields[6];  } }
        public ChessFieldViewModel Field_H1 { get { return Fields[7];  } }

        // row 2
        public ChessFieldViewModel Field_A2 { get { return Fields[8];  } }
        public ChessFieldViewModel Field_B2 { get { return Fields[9];  } }
        public ChessFieldViewModel Field_C2 { get { return Fields[10]; } }
        public ChessFieldViewModel Field_D2 { get { return Fields[11]; } }
        public ChessFieldViewModel Field_E2 { get { return Fields[12]; } }
        public ChessFieldViewModel Field_F2 { get { return Fields[13]; } }
        public ChessFieldViewModel Field_G2 { get { return Fields[14]; } }
        public ChessFieldViewModel Field_H2 { get { return Fields[15]; } }

        // row 3
        public ChessFieldViewModel Field_A3 { get { return Fields[16]; } }
        public ChessFieldViewModel Field_B3 { get { return Fields[17]; } }
        public ChessFieldViewModel Field_C3 { get { return Fields[18]; } }
        public ChessFieldViewModel Field_D3 { get { return Fields[19]; } }
        public ChessFieldViewModel Field_E3 { get { return Fields[20]; } }
        public ChessFieldViewModel Field_F3 { get { return Fields[21]; } }
        public ChessFieldViewModel Field_G3 { get { return Fields[22]; } }
        public ChessFieldViewModel Field_H3 { get { return Fields[23]; } }

        // row 4
        public ChessFieldViewModel Field_A4 { get { return Fields[24]; } }
        public ChessFieldViewModel Field_B4 { get { return Fields[25]; } }
        public ChessFieldViewModel Field_C4 { get { return Fields[26]; } }
        public ChessFieldViewModel Field_D4 { get { return Fields[27]; } }
        public ChessFieldViewModel Field_E4 { get { return Fields[28]; } }
        public ChessFieldViewModel Field_F4 { get { return Fields[29]; } }
        public ChessFieldViewModel Field_G4 { get { return Fields[30]; } }
        public ChessFieldViewModel Field_H4 { get { return Fields[31]; } }

        // row 5
        public ChessFieldViewModel Field_A5 { get { return Fields[32]; } }
        public ChessFieldViewModel Field_B5 { get { return Fields[33]; } }
        public ChessFieldViewModel Field_C5 { get { return Fields[34]; } }
        public ChessFieldViewModel Field_D5 { get { return Fields[35]; } }
        public ChessFieldViewModel Field_E5 { get { return Fields[36]; } }
        public ChessFieldViewModel Field_F5 { get { return Fields[37]; } }
        public ChessFieldViewModel Field_G5 { get { return Fields[38]; } }
        public ChessFieldViewModel Field_H5 { get { return Fields[39]; } }

        // row 6
        public ChessFieldViewModel Field_A6 { get { return Fields[40]; } }
        public ChessFieldViewModel Field_B6 { get { return Fields[41]; } }
        public ChessFieldViewModel Field_C6 { get { return Fields[42]; } }
        public ChessFieldViewModel Field_D6 { get { return Fields[43]; } }
        public ChessFieldViewModel Field_E6 { get { return Fields[44]; } }
        public ChessFieldViewModel Field_F6 { get { return Fields[45]; } }
        public ChessFieldViewModel Field_G6 { get { return Fields[46]; } }
        public ChessFieldViewModel Field_H6 { get { return Fields[47]; } }

        // row 7
        public ChessFieldViewModel Field_A7 { get { return Fields[48]; } }
        public ChessFieldViewModel Field_B7 { get { return Fields[49]; } }
        public ChessFieldViewModel Field_C7 { get { return Fields[50]; } }
        public ChessFieldViewModel Field_D7 { get { return Fields[51]; } }
        public ChessFieldViewModel Field_E7 { get { return Fields[52]; } }
        public ChessFieldViewModel Field_F7 { get { return Fields[53]; } }
        public ChessFieldViewModel Field_G7 { get { return Fields[54]; } }
        public ChessFieldViewModel Field_H7 { get { return Fields[55]; } }

        // row 8
        public ChessFieldViewModel Field_A8 { get { return Fields[56]; } }
        public ChessFieldViewModel Field_B8 { get { return Fields[57]; } }
        public ChessFieldViewModel Field_C8 { get { return Fields[58]; } }
        public ChessFieldViewModel Field_D8 { get { return Fields[59]; } }
        public ChessFieldViewModel Field_E8 { get { return Fields[60]; } }
        public ChessFieldViewModel Field_F8 { get { return Fields[61]; } }
        public ChessFieldViewModel Field_G8 { get { return Fields[62]; } }
        public ChessFieldViewModel Field_H8 { get { return Fields[63]; } }

        #endregion Fields

        #endregion Members

        #region Methods

        private ChessFieldViewModel[] getInitializedFields(Action<object> onFieldClicked)
        {
            var fields = new ChessFieldViewModel[64];

            for (byte pos = 0; pos < 64; pos++)
            {
                fields[pos] = new ChessFieldViewModel(new ChessPosition(pos), onFieldClicked);
            }

            return fields;
        }

        public void UpdatePieces(IChessBoard board)
        {
            for (byte pos = 0; pos < 64; pos++)
            {
                var field = Fields[pos];
                field.UpdatePiece(board.IsCapturedAt(pos) ? (ChessPiece?)board.GetPieceAt(pos) : null);
            }

            _board = (IChessBoard)((ICloneable)board).Clone();
        }

        public void UpdateIsEnabled(bool isActive)
        {
            IsEnabled = isActive;
            NotifyPropertyChanged(nameof(IsEnabled));
        }

        #endregion Methods
    }
}
