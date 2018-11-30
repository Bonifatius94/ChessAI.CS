using Chess.AI;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessAITest : TestBase
    {
        #region Constructor

        public ChessAITest(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void DrawAITest()
        {
            // TODO: test higher difficulties if 

            for (int difficultyValue = 0; difficultyValue < 3; difficultyValue++)
            {
                var difficulty = (ChessDifficultyLevel)difficultyValue;

                var board = ChessBoard.StartFormation;
                var draw = new ChessDrawAI().GetNextDraw(board, null, difficulty);

                output.WriteLine(draw.ToString());
            }
        }

        #endregion Tests
    }
}
