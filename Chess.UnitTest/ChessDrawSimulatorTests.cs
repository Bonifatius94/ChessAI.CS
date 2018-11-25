using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawSimulatorTests : TestBase
    {
        #region Constructor

        public ChessDrawSimulatorTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        [Fact]
        public void IsDrawIntoCheckTest()
        {
            // test this for both black and white chess pieces
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var allyColor = (ChessColor)colorValue;
                var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;

                var newPos = new Dictionary<ChessPieceType, ChessPosition>()
                {
                    { ChessPieceType.Queen,   new ChessPosition(4, 4)                                         },
                    { ChessPieceType.Rook,    new ChessPosition(4, 4)                                         },
                    { ChessPieceType.Bishop,  new ChessPosition(4, 5)                                         },
                    { ChessPieceType.Knight,  new ChessPosition(5, 3)                                         },
                    { ChessPieceType.Peasant, new ChessPosition(((allyColor == ChessColor.White) ? 4 : 2), 4) },
                };

                // go through all drawing chess piece types (except king; king is already tested in king draws unit test)
                for (int pieceTypeValue = 1; pieceTypeValue < 6; pieceTypeValue++)
                {
                    var pieceType = (ChessPieceType)pieceTypeValue;
                    
                    // simulate situations with and without attacker (positive and negative test)
                    for (int putAttackerValue = 0; putAttackerValue < 2; putAttackerValue++)
                    {
                        bool putAttacker = (putAttackerValue == 1);
                        var oldPos = new ChessPosition(3, 4);

                        var pieces = new List<ChessPieceAtPos>()
                        {
                            new ChessPieceAtPos(oldPos,                  new ChessPiece() { Type = pieceType,             Color = allyColor,  WasMoved = true }),
                            new ChessPieceAtPos(new ChessPosition(0, 7), new ChessPiece() { Type = ChessPieceType.King,   Color = allyColor,  WasMoved = true }),
                            new ChessPieceAtPos(new ChessPosition(7, 5), new ChessPiece() { Type = ChessPieceType.King,   Color = enemyColor, WasMoved = true }),
                        };

                        if (putAttacker) { pieces.Add(new ChessPieceAtPos(new ChessPosition(7, 0), new ChessPiece() { Type = ChessPieceType.Bishop, Color = enemyColor, WasMoved = true })); }

                        var board = new ChessBoard(pieces);
                        var draw = new ChessDraw(board, oldPos, newPos[pieceType]);

                        bool shouldBeDrawIntoCheck = putAttacker;
                        bool isDrawIntoCheck = new ChessDrawSimulator().IsDrawIntoCheck(board, draw);
                        Assert.True(shouldBeDrawIntoCheck == isDrawIntoCheck);
                    }
                }
            }
        }

        [Fact]
        public void GetCheckGameStatusTest()
        {
            // test no check
            var board = ChessBoard.StartFormation;
            var enemyDraw = new ChessDraw(board, new ChessPosition(0, 0), new ChessPosition(0, 0));
            board.ApplyDraw(enemyDraw);
            Assert.True(new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.None);

            // test simple check
            var pieces = new List<ChessPieceAtPos>()
            {
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(7, 3), new ChessPiece() { Type = ChessPieceType.Queen, Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.Black, WasMoved = true }),
            };
            
            board = new ChessBoard(pieces);
            enemyDraw = new ChessDraw(board, new ChessPosition(7, 3), new ChessPosition(6, 4));
            board.ApplyDraw(enemyDraw);
            Assert.True(new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.Check);

            // test checkmate
            for (int putSavingPieceValue = 0; putSavingPieceValue < 2; putSavingPieceValue++)
            {
                pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.White, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece() { Type = ChessPieceType.Queen, Color = ChessColor.Black, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.Black, WasMoved = true }),
                };

                // allow the ally to avoid the checkmate by putting an additional chess piece on the board
                // and test if the simulator makes use of this opportunity
                bool putSavingPiece = (putSavingPieceValue == 1);
                if (putSavingPiece) { pieces.Add(new ChessPieceAtPos(new ChessPosition(1, 7), new ChessPiece() { Type = ChessPieceType.Rook, Color = ChessColor.White, WasMoved = true })); }

                board = new ChessBoard(pieces);
                enemyDraw = new ChessDraw(board, new ChessPosition(2, 3), new ChessPosition(1, 4));
                board.ApplyDraw(enemyDraw);

                Assert.True(
                    (!putSavingPiece && new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.Checkmate)
                    || (putSavingPiece && new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.Check)
                );
            }

            // test stalemate
            pieces = new List<ChessPieceAtPos>()
            {
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece() { Type = ChessPieceType.Queen, Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(3, 5), new ChessPiece() { Type = ChessPieceType.Queen, Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece() { Type = ChessPieceType.King,  Color = ChessColor.Black, WasMoved = true }),
            };

            board = new ChessBoard(pieces);
            enemyDraw = new ChessDraw(board, new ChessPosition(3, 5), new ChessPosition(2, 5));
            board.ApplyDraw(enemyDraw);
            Assert.True(new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.Stalemate);
            
        }

        #endregion Tests
    }
}
