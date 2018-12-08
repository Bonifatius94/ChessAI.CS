using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessBoardTests : TestBase
    {
        #region Constructor

        public ChessBoardTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        [Fact]
        public void ConstructorTest()
        {
            // define the chess pieces to be put on the chess board
            var pieces = new List<ChessPieceAtPos>()
            {
                new ChessPieceAtPos(new ChessPosition("A1"), new ChessPiece(ChessPieceType.Queen,   ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("B1"), new ChessPiece(ChessPieceType.Knight,  ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("G1"), new ChessPiece(ChessPieceType.King,    ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("F2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
                new ChessPieceAtPos(new ChessPosition("G2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
                new ChessPieceAtPos(new ChessPosition("H2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
                new ChessPieceAtPos(new ChessPosition("B3"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("E3"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("A4"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("B7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true )),
                new ChessPieceAtPos(new ChessPosition("D7"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, true )),

                new ChessPieceAtPos(new ChessPosition("C6"), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true )),
                new ChessPieceAtPos(new ChessPosition("E6"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("A7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true )),
                new ChessPieceAtPos(new ChessPosition("F7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("G7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("H7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("F8"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("G8"), new ChessPiece(ChessPieceType.Knight,  ChessColor.Black, false)),
                new ChessPieceAtPos(new ChessPosition("H8"), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, false)),
            };

            // create a new chess board with the given chess pieces
            var board = new ChessBoard(pieces);

            // go through every chess position on the chess board and check if the chess piece is set correctly
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    var position = new ChessPosition(row, column);
                    var pieceAtPos = board.GetPieceAt(position);
                    Assert.True((pieceAtPos != null && pieces.Any(x => x.Piece == pieceAtPos.Value)) || (pieceAtPos == null && !pieces.Any(x => x.Position == position)));
                }
            }
        }
    }
}
