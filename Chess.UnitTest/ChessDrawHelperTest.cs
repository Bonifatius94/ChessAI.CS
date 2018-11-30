using Chess.AI;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawHelperTest : TestBase
    {
        #region Constructor

        public ChessDrawHelperTest(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void DrawAITest()
        {
            var board = ChessBoard.StartFormation;
            var draw = new ChessDrawHelper().GetNextDraw(board, new ChessDraw(), ChessDifficultyLevel.Easy);
            output.WriteLine(draw.ToString());
        }

        #endregion Tests
    }
}
