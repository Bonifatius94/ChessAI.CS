using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawTests : TestBase
    {
        #region Constructor

        public ChessDrawTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void ConstructorAndGetterSetterTest()
        {
            // check if all possible chess draws can be created
            for (int drawingSideValue = 0; drawingSideValue < 2; drawingSideValue++)
            {
                var drawingSide = (ChessColor)drawingSideValue;

                for (int drawTypeValue = 0; drawTypeValue < 4; drawTypeValue++)
                {
                    var drawType = (ChessDrawType)drawTypeValue;

                    for (int drawingPieceTypeValue = 1; drawingPieceTypeValue < 7; drawingPieceTypeValue++)
                    {
                        var drawingPieceType = (ChessPieceType)drawingPieceTypeValue;

                        for (int takenPieceTypeValue = 0; takenPieceTypeValue < 7; takenPieceTypeValue++)
                        {
                            var takenPieceType = (takenPieceTypeValue > 0) ? (ChessPieceType?)((ChessPieceType)takenPieceTypeValue) : null;

                            for (int promotionPieceTypeValue = 0; promotionPieceTypeValue < 7; promotionPieceTypeValue++)
                            {
                                var promotionPieceType = (promotionPieceTypeValue > 0) ? (ChessPieceType?)((ChessPieceType)promotionPieceTypeValue) : null;

                                for (int oldPosHash = 0; oldPosHash < 64; oldPosHash++)
                                {
                                    var oldPos = new ChessPosition((byte)oldPosHash);

                                    for (int newPosHash = 0; newPosHash < 64; newPosHash++)
                                    {
                                        var newPos = new ChessPosition((byte)newPosHash);

                                        // get expected hash code
                                        int hashCode = ((drawingSideValue << 23) | (drawTypeValue << 21) | (drawingPieceTypeValue << 18) | (takenPieceTypeValue << 15) | (promotionPieceTypeValue << 12) | (oldPosHash << 6) | (newPosHash));

                                        // create a new chess draw instance and check if the getters work correctly
                                        var draw = new ChessDraw(hashCode);

                                        // check if the created chess draw has the correct features
                                        Assert.True(
                                            draw.DrawingSide == drawingSide && draw.Type == drawType && draw.DrawingPieceType == drawingPieceType && draw.TakenPieceType == takenPieceType
                                            && draw.PeasantPromotionPieceType == promotionPieceType && draw.OldPosition == oldPos && draw.NewPosition == newPos && draw.GetHashCode() == hashCode
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // test if several invalid chess draws are rejected
            try
            {
                // create invalid chess draw (should throw an exception)
                new ChessDraw(-1);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            try
            {
                // create invalid chess draw (should throw an exception)
                new ChessDraw(2097152);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
        }

        [Fact]
        public void IsValidTest()
        {
            // TODO: implement test

            // This test should be obsolete if the draws generator works fine. 
            // The validation only makes sure that the draws generator creates the same draw for the given chess piece and chess board.
            // Moreover the validation is hardly ever used due to performance savings when deactivating it.
        }

        #endregion Tests
    }
}
