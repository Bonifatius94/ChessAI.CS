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
                new ChessPieceAtPos(new ChessPosition("A1"), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("B1"), new ChessPiece() { Type = ChessPieceType.Knight,  Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("G1"), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("F2"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("G2"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("H2"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("B3"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("E3"), new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("A4"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("B7"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("D7"), new ChessPiece() { Type = ChessPieceType.Rook,    Color = ChessColor.White, WasMoved = true  }),

                new ChessPieceAtPos(new ChessPosition("C6"), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("E6"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("A7"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = true  }),
                new ChessPieceAtPos(new ChessPosition("F7"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("G7"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("H7"), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("F8"), new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("G8"), new ChessPiece() { Type = ChessPieceType.Knight,  Color = ChessColor.Black, WasMoved = false }),
                new ChessPieceAtPos(new ChessPosition("H8"), new ChessPiece() { Type = ChessPieceType.Rook,    Color = ChessColor.Black, WasMoved = false }),
            };

            // create a new chess board with the given chess pieces
            var board = new ChessBoard(pieces);

            // go through every chess position on the chess board and check if the chess piece is set correctly
            for (int row = 0; row < ChessBoard.CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < ChessBoard.CHESS_BOARD_DIMENSION; column++)
                {
                    var position = new ChessPosition(row, column);
                    var pieceAtPos = board.GetPieceAt(position);
                    Assert.True((pieceAtPos != null && pieces.Any(x => x.Piece == pieceAtPos.Value)) || (pieceAtPos == null && !pieces.Any(x => x.Position == position)));
                }
            }
        }
    }
}
