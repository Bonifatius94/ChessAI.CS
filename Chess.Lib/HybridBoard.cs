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

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Chess.Lib
//{
//    public struct HybridBoard : IChessBoard
//    {
//        #region Constructor

//        // TODO: add useful constructors

//        #endregion Constructor

//        #region Members

//        // the two board representations managing the data independently
//        private ChessBoard _piecesBoard;
//        private ChessBitboard _bitboard;

//        // use very efficient pieces board operation
//        public IEnumerable<ChessPieceAtPos> WhitePieces => throw new NotImplementedException();

//        // use very efficient pieces board operation
//        public IEnumerable<ChessPieceAtPos> BlackPieces => throw new NotImplementedException();

//        // use very efficient bitboard operation
//        public ChessPieceAtPos WhiteKing => throw new NotImplementedException();

//        // use very efficient bitboard operation
//        public ChessPieceAtPos BlackKing => throw new NotImplementedException();

//        #endregion Members

//        #region Methods

//        public ChessBoard ApplyDraw(ChessDraw draw)
//        {
//            throw new NotImplementedException();
//        }

//        public ChessBoard ApplyDraws(IList<ChessDraw> draws)
//        {
//            throw new NotImplementedException();
//        }

//        public ChessPiece GetPieceAt(ChessPosition position)
//        {
//            throw new NotImplementedException();
//        }

//        public ChessPiece GetPieceAt(byte position)
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerable<ChessPieceAtPos> GetPiecesOfColor(ChessColor side)
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsCapturedAt(ChessPosition position)
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsCapturedAt(byte position)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion Methods
//    }
//}
