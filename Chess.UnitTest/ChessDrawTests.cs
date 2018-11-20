using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Chess.UnitTest
{
    public class ChessDrawTests
    {
        #region Tests

        public void ConstructorAndGetterSetterTest()
        {
            // check if all possible chess draws can be created
            for (int drawingSideValue = 0; drawingSideValue < 2; drawingSideValue++)
            {
                var drawingSide = (ChessColor)drawingSideValue;

                for (int drawTypeValue = 0; drawTypeValue < 4; drawTypeValue++)
                {
                    var drawType = (ChessDrawType)drawTypeValue;

                    for (int drawingPieceTypeValue = 0; drawingPieceTypeValue < 6; drawingPieceTypeValue++)
                    {
                        var drawingPieceType = (ChessPieceType)drawingPieceTypeValue;

                        for (int promotionPieceTypeValue = 0; promotionPieceTypeValue < 7; promotionPieceTypeValue++)
                        {
                            var promotionPieceType = (promotionPieceTypeValue != 6) ? (ChessPieceType?)((ChessPieceType)promotionPieceTypeValue) : null;

                            for (int oldPosHash = 0; oldPosHash < ChessBoard.CHESS_BOARD_DIMENSION * ChessBoard.CHESS_BOARD_DIMENSION; oldPosHash++)
                            {
                                var oldPos = new ChessPosition((byte)oldPosHash);

                                for (int newPosHash = 0; newPosHash < ChessBoard.CHESS_BOARD_DIMENSION * ChessBoard.CHESS_BOARD_DIMENSION; newPosHash++)
                                {
                                    var newPos = new ChessPosition((byte)newPosHash);

                                    // get expected hash code
                                    int hashCode = ((drawingSideValue << 20) | (drawTypeValue << 18) | (drawingPieceTypeValue << 15) | (promotionPieceTypeValue << 12) | (oldPosHash << 6) | (newPosHash));

                                    // create a new chess draw instance and check if the getters work correctly
                                    var draw = new ChessDraw(hashCode);

                                    // check if the created chess draw has the correct features
                                    Assert.True(
                                        draw.DrawingSide == drawingSide && draw.Type == drawType && draw.DrawingPieceType == drawingPieceType 
                                        && draw.PeasantPromotionPieceType == promotionPieceType && draw.OldPosition == oldPos && draw.NewPosition == newPos && draw.GetHashCode() == hashCode
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }

        public void IsValidTest()
        {
            // TODO: implement test
        }

        #endregion Tests
    }
}
