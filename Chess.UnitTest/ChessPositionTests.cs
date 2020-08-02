/*
 * MIT License
 *
 * Copyright(c) 2020 Marco Tröster
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Chess.Lib;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessPositionTests : TestBase
    {
        #region Constructor

        public ChessPositionTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void ConstructorTest()
        {
            // test if all valid chess positions can be created and have the correct features
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    ChessPosition position;
                    string fieldName = $"{ (char)(column + 'A') }{ (char)(row + '1') }";
                    byte hashCode = (byte)(row * 8 + column);
                    
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
