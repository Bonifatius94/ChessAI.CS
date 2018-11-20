using Chess.Lib;
using System;
using Xunit;

namespace Chess.UnitTest
{
    public class ChessPositionTests
    {
        #region Tests

        [Fact]
        public void ConstructorTest()
        {
            // test if all valid chess positions can be created and have the correct features
            for (int row = 0; row < ChessBoard.CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < ChessBoard.CHESS_BOARD_DIMENSION; column++)
                {
                    ChessPosition position;
                    string fieldName = $"{ (char)(column + 'A') }{ (char)(row + '1') }";
                    byte hashCode = (byte)(row * ChessBoard.CHESS_BOARD_DIMENSION + column);
                    
                    // create a new chess position instance and check if the row, column and hash code values are set as expected
                    position = new ChessPosition(fieldName);
                    Assert.True(position.Row == row && position.Column == column && position.GetHashCode() == hashCode);

                    // create a new chess position instance and check if the row, column and hash code values are set as expected
                    position = new ChessPosition(new Tuple<int, int>(row, column));
                    Assert.True(position.Row == row && position.Column == column && position.GetHashCode() == hashCode);

                    // create a new chess position instance and check if the row, column and hash code values are set as expected
                    position = new ChessPosition(row, column);
                    Assert.True(position.Row == row && position.Column == column && position.GetHashCode() == hashCode);

                    // create a new chess position instance and check if the row, column and hash code values are set as expected
                    position = new ChessPosition((byte)(row * 8 + column));
                    Assert.True(position.Row == row && position.Column == column && position.GetHashCode() == hashCode);
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
            
            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition(8, 8);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition(-1, -1);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
            
            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition(72);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            try
            {
                // create invalid chess position (should throw an exception)
                new ChessPosition(255);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
        }
        
        #endregion Tests
    }
}
