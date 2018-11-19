using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Chess.UnitTest
{
    public class ChessPositionTests
    {
        #region Tests

        [Fact]
        public void FieldNameTest()
        {
            // test if all valid chess positions can be created and have the correct features
            for (int row = 0; row < ChessBoard.CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < ChessBoard.CHESS_BOARD_DIMENSION; column++)
                {
                    // get the field name
                    char rowChar = (char)(row + '1');
                    char columnChar = (char)(column + 'A');
                    string fieldName = $"{ columnChar }{ rowChar }";

                    // create a new chess position instance
                    var position = new ChessPosition(fieldName);

                    // check if the row, column and hash code values are set as expected
                    Assert.True(position.Row == row && position.Column == column && position.GetHashCode() >= 0 && position.GetHashCode() < 64);
                }
            }

            // test if several invalid chess positions are rejected
            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition("I9");
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition("?0");
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
        }

        #endregion Tests
    }
}
