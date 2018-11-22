using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawPossibilitiesTest : TestBase
    {
        #region Constructor

        public ChessDrawPossibilitiesTest(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void KingDrawsTest()
        {
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var color = (ChessColor)colorValue;

                for (int i = 0; i < 4; i++)
                {
                    int row = i / 2 == 0 ? 0 : 7;
                    int col = i % 2 == 0 ? 0 : 7;

                    var pieces = new List<ChessPiece>() {
                        new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.White, Position = new ChessPosition(row, col), WasMoved = false },
                        new ChessPiece() { Type = ChessPieceType.Queen, Color = ChessColor.White, Position = new ChessPosition(row + 1, col), WasMoved = false },
                        new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.Black, Position = new ChessPosition(4, 4), WasMoved = false },
                    };

                    // evaluate white king draws
                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawPossibilitiesHelper().GetPossibleDraws(board, pieces[0], new ChessDraw(), true);
                    Assert.True(draws.Count() == 2);

                    // TODO: implement logic
                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawPossibilitiesHelper().GetPossibleDraws(board, pieces[0], new ChessDraw(), true);
                    Assert.True(draws.Count() == 2);
                }
            }

            // TODO: implement draw into chess test

            // TODO: implement rochade test
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
