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
