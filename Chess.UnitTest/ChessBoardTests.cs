using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Chess.UnitTest
{
    public class ChessBoardTests
    {
        [Fact]
        public void ConstructorTest()
        {
            // define the chess pieces to be put on the chess board
            var pieces = new List<ChessPiece>()
            {
                new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.White, Position = new ChessPosition("A1"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Knight,  Color = ChessColor.White, Position = new ChessPosition("B1"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, Position = new ChessPosition("G1"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("F2"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("G2"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("H2"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("B3"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.White, Position = new ChessPosition("E3"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("A4"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition("B7"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Rook,    Color = ChessColor.White, Position = new ChessPosition("D7"), WasMoved = true  },

                new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, Position = new ChessPosition("C6"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, Position = new ChessPosition("E6"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, Position = new ChessPosition("A7"), WasMoved = true  },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, Position = new ChessPosition("F7"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, Position = new ChessPosition("G7"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.Black, Position = new ChessPosition("H7"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.Black, Position = new ChessPosition("F8"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Knight,  Color = ChessColor.Black, Position = new ChessPosition("G8"), WasMoved = false },
                new ChessPiece() { Type = ChessPieceType.Rook,    Color = ChessColor.Black, Position = new ChessPosition("H8"), WasMoved = false },
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
                    Assert.True((pieceAtPos != null && pieces.Contains(pieceAtPos.Value)) || (pieceAtPos == null && !pieces.Any(x => x.Position == position)));
                }
            }
        }
    }
}
