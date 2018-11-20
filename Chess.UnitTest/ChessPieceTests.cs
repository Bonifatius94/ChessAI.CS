using Chess.Lib;
using System;
using Xunit;

namespace Chess.UnitTest
{
    public class ChessPieceTests
    {
        #region Tests

        [Fact]
        public void ConstructorTest()
        {
            // test if all valid chess pieces can be created and invalid inputs are rejected
            // also test all getters / setters of the chess piece
            for (int row = 0; row < ChessBoard.CHESS_BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < ChessBoard.CHESS_BOARD_DIMENSION; column++)
                {
                    // get position out of (row, column) tuple
                    var position = new ChessPosition(row, column);

                    for (int pieceType = 0; pieceType < 6; pieceType++)
                    {
                        // get chess piece type
                        var type = (ChessPieceType)pieceType;

                        for (int pieceColor = 0; pieceColor < 2; pieceColor++)
                        {
                            // get chess color
                            var color = (ChessColor)pieceColor;

                            for (int wasPieceMoved = 0; wasPieceMoved < 2; wasPieceMoved++)
                            {
                                // get was moved
                                bool wasMoved = wasPieceMoved == 1;

                                // calculate expected hash code
                                short hashCode = (short)((pieceColor << 10) | (wasPieceMoved << 9) | (pieceType << 6) | (position.GetHashCode()));

                                // create a new chess piece instance (setters test) and check if the chess piece has the correct features (getters test)
                                var piece = new ChessPiece() { Type = type, Color = color, Position = position, WasMoved = wasMoved };
                                Assert.True(piece.Type == type && piece.Color == color && piece.Position == position && piece.WasMoved == wasMoved && piece.GetHashCode() == hashCode);

                                // create a new chess piece instance (setters test) and check if the chess piece has the correct features (getters test)
                                piece = new ChessPiece(hashCode);
                                Assert.True(piece.Type == type && piece.Color == color && piece.Position == position && piece.WasMoved == wasMoved && piece.GetHashCode() == hashCode);
                            }
                        }
                    }
                }
            }

            // test if several invalid chess pieces are rejected
            try
            {
                // create invalid chess piece (should throw an exception)
                new ChessPiece(-1);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            // test if several invalid chess pieces are rejected
            try
            {
                // create invalid chess piece (should throw an exception)
                new ChessPiece(2048);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
        }

        #endregion Tests
    }
}
