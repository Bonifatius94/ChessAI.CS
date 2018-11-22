using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawGeneratorTests : TestBase
    {
        #region Constructor

        public ChessDrawGeneratorTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void KingDrawsTest()
        {
            // check if only draws are computed where the destination is actually on the chess board
            // and also make sure that only enemy pieces are taken (blocking allied pieces not)
            for (int run = 0; run < 4; run++)
            {
                // get the (row, column) of the white king
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                var pieces = new List<ChessPiece>()
                {
                    new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, Position = new ChessPosition(row, col), WasMoved = true },
                    new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, Position = new ChessPosition(6, 1),     WasMoved = true },
                    new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, Position = new ChessPosition(5, 2),     WasMoved = true },
                    new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.Black, Position = new ChessPosition(2, 5),     WasMoved = true },
                };

                // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                var board = new ChessBoard(pieces);
                var draws = new ChessDrawGenerator().GetDraws(board, board.WhiteKing, new ChessDraw(), true);
                Assert.True(draws.Count() == ((row + col == 7) ? 2 : 3));

                // evaluate black king draws (when white king is onto A8, then the white peasant cannot be taken because of draw into check by the black king)
                board = new ChessBoard(pieces);
                draws = new ChessDrawGenerator().GetDraws(board, board.BlackKing, new ChessDraw(), true);
                Assert.True(draws.Count() == ((row == 7 && col == 0) ? 7 : 8));
            }
            
            // check if rochade draws are handled correctly
            for (int run = 0; run < 4; run++)
            {
                for (int wasMovedValue = 0; wasMovedValue < 4; wasMovedValue++)
                {
                    // determine whether king / rook was already moved before the rochade
                    bool wasKingMoved = wasMovedValue / 2 == 0;
                    bool wasRookMoved = wasMovedValue % 2 == 0;

                    // get the (row, column) of the rook before rochade
                    int rookRow = run / 2 == 0 ? 0 : 7;
                    int rookCol = run % 2 == 0 ? 0 : 7;

                    // determine the old and new positions of king / rook after rochade
                    var oldKingPos = new ChessPosition(rookRow, 4);
                    var newKingPos = new ChessPosition(rookRow, (rookCol == 0 ? 2 : 6));
                    var oldRookPos = new ChessPosition(rookRow, rookCol);
                    var newRookPos = new ChessPosition(rookRow, (rookCol == 0 ? 3 : 5));

                    // determine the side performing a rochade
                    var allyColor = (rookRow == 0) ? ChessColor.White : ChessColor.Black;
                    var enemyColor = (rookRow == 0) ? ChessColor.Black : ChessColor.White;

                    for (int attack = 0; attack < 4; attack++)
                    {
                        var enemyPos = new ChessPosition(4, (rookRow == 0) ? (4 - attack) : (4 + attack));
                        var enemyKingPos = new ChessPosition(4, 0);

                        var pieces = new List<ChessPiece>()
                        {
                            new ChessPiece() { Type = ChessPieceType.King, Color = allyColor,  Position = oldKingPos,   WasMoved = wasKingMoved },
                            new ChessPiece() { Type = ChessPieceType.Rook, Color = allyColor,  Position = oldRookPos,   WasMoved = wasRookMoved },
                            new ChessPiece() { Type = ChessPieceType.Rook, Color = enemyColor, Position = enemyPos,     WasMoved = true         },
                            new ChessPiece() { Type = ChessPieceType.King, Color = enemyColor, Position = enemyKingPos, WasMoved = true         },
                        };

                        // init chess board and rochade draw
                        var board = new ChessBoard(pieces);
                        var draw = new ChessDraw(board, pieces[0].Position, newKingPos);

                        // check whether the draw validation returns the expected value
                        bool shouldRochadeBeValid = !wasKingMoved && !wasRookMoved && attack >= 3;
                        bool isRochadeValid = draw.IsValid(board, new ChessDraw());
                        Assert.True(shouldRochadeBeValid == isRochadeValid);

                        // check whether the rochade is applied correctly to the chess board
                        if (isRochadeValid)
                        {
                            board.ApplyDraw(draw);
                            Assert.True(
                                board.GetPieceAt(oldKingPos) == null && board.GetPieceAt(newKingPos).Value.Type == ChessPieceType.King && board.GetPieceAt(newKingPos).Value.Color == allyColor
                                && board.GetPieceAt(oldRookPos) == null && board.GetPieceAt(newRookPos).Value.Type == ChessPieceType.Rook && board.GetPieceAt(newRookPos).Value.Color == allyColor
                            );
                        }
                    }
                }
            }
        }

        [Fact]
        public void QueenDrawsTest()
        {
            // TODO: implement test
        }

        [Fact]
        public void RookDrawsTest()
        {
            // TODO: implement test
        }

        [Fact]
        public void BishopDrawsTest()
        {
            // TODO: implement test
        }

        [Fact]
        public void KnightDrawsTest()
        {
            // TODO: implement test
        }

        [Fact]
        public void PawnDrawsTest()
        {
            // TODO: implement test
        }
        
        #endregion Tests
    }
}
