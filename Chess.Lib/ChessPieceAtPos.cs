using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib
{
    public readonly struct ChessPieceAtPos
    {
        #region Constructor

        public ChessPieceAtPos(ChessPosition position, ChessPiece piece)
        {
            Position = position;
            Piece = piece;
        }

        #endregion Constructor

        #region Members

        public readonly ChessPosition Position;
        public readonly ChessPiece Piece;

        #endregion Members

        #region Methods

        public override string ToString()
        {
            return $"{ Piece.ToString() } ({ Position.ToString() })";
        }

        // TODO: implement other overrides Equals(), GetHashCode(), ==(ChessPieceAtPos, ChessPieceAtPos), ...

        #endregion Methods
    }
}
