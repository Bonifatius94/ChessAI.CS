using Chess.Lib;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessPieceTests : TestBase
    {
        #region Constructor

        public ChessPieceTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void ConstructorAndGetterSetterTest()
        {
            // test if all valid chess pieces can be created and invalid inputs are rejected
            // also test all getters / setters of the chess piece
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
                        byte hashCode = (byte)((pieceColor << 4) | (wasPieceMoved << 3) | pieceType);

                        // create a new chess piece instance (setters test) and check if the chess piece has the correct features (getters test)
                        var piece = new ChessPiece() { Type = type, Color = color, WasMoved = wasMoved };
                        Assert.True(piece.Type == type && piece.Color == color&& piece.WasMoved == wasMoved && piece.GetHashCode() == hashCode);

                        // create a new chess piece instance (setters test) and check if the chess piece has the correct features (getters test)
                        piece = new ChessPiece(hashCode);
                        Assert.True(piece.Type == type && piece.Color == color && piece.WasMoved == wasMoved && piece.GetHashCode() == hashCode);
                    }
                }
            }

            // test if several invalid chess pieces are rejected
            try
            {
                // create invalid chess piece (should throw an exception)
                new ChessPiece(0xFF);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }

            // test if several invalid chess pieces are rejected
            try
            {
                // create invalid chess piece (should throw an exception)
                new ChessPiece(0x20);
                Assert.True(false);
            }
            catch (Exception) { /* nothing to do here ... */ }
        }

        #endregion Tests
    }
}
