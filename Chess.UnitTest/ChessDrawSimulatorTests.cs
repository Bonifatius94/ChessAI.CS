using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
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
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(1, 5), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(7, 3), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = true }),
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
                    new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(6, 1), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.Black, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = true }),
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
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(6, 1), new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(3, 5), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = true }),
                new ChessPieceAtPos(new ChessPosition(7, 1), new ChessPiece() { Type = ChessPieceType.Rook,    Color = ChessColor.Black, WasMoved = true }),
            };

            board = new ChessBoard(pieces);
            enemyDraw = new ChessDraw(board, new ChessPosition(3, 5), new ChessPosition(2, 5));
            board.ApplyDraw(enemyDraw);
            Assert.True(new ChessDrawSimulator().GetCheckGameStatus(board, enemyDraw) == CheckGameStatus.Stalemate);
        }

        // moves to be made for testing the check situation
        private static List<Tuple<ChessPosition, ChessPosition>> _moves1 = new List<Tuple<ChessPosition, ChessPosition>>()
        {
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("A6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("C4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G7"), new ChessPosition("G5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("B5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("E6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C2"), new ChessPosition("C3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("E7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E1"), new ChessPosition("F1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H7"), new ChessPosition("H6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("H5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("G6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B5"), new ChessPosition("D7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("D7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H5"), new ChessPosition("H3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A6"), new ChessPosition("A5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("B4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G6"), new ChessPosition("H4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H3"), new ChessPosition("G4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("C6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("A3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F8"), new ChessPosition("B4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("C1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("F8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("B2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H8"), new ChessPosition("G8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("F3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B7"), new ChessPosition("B6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("D4")),
        };

        // moves to be made for testing the check situation
        private static List<Tuple<ChessPosition, ChessPosition>> _moves2 = new List<Tuple<ChessPosition, ChessPosition>>()
        {
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H2"), new ChessPosition("H4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B7"), new ChessPosition("B5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("B4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("H6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G2"), new ChessPosition("G3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("C6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("F3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("A6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("B2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H8"), new ChessPosition("G8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("A3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C6"), new ChessPosition("B8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("C1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G7"), new ChessPosition("G5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("B1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("G7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C2"), new ChessPosition("C3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("D6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("G4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G7"), new ChessPosition("G6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("A3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("E5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("G5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A6"), new ChessPosition("A5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("B5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C8"), new ChessPosition("F5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H1"), new ChessPosition("G1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F5"), new ChessPosition("B1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("A2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("C4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("A7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("F1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("A8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("A2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("A7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("G1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("D7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("D3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("F6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B5"), new ChessPosition("D4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E5"), new ChessPosition("D4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C3"), new ChessPosition("D4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D6"), new ChessPosition("D5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G5"), new ChessPosition("H3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C7"), new ChessPosition("C6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("E2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A5"), new ChessPosition("B4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E1"), new ChessPosition("D2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C6"), new ChessPosition("C5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("E3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("B7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H4"), new ChessPosition("H5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H6"), new ChessPosition("F5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E3"), new ChessPosition("D2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B7"), new ChessPosition("B8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A2"), new ChessPosition("C4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F6"), new ChessPosition("D7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("H1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F5"), new ChessPosition("G3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("E2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E4"), new ChessPosition("D5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G6"), new ChessPosition("G8")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("E3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("B6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D5"), new ChessPosition("D6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("G1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("B1")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("E2")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E3"), new ChessPosition("E4")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F8"), new ChessPosition("E7")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E4"), new ChessPosition("F3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B6"), new ChessPosition("C6")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("B3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("G5")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("E3")),
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F7"), new ChessPosition("F5")),
        };

        [Fact]
        public void DefendCheckTest()
        {
            defendCheckTest(_moves1);
            //defendCheckTest(_moves2);
        }

        private void defendCheckTest(List<Tuple<ChessPosition, ChessPosition>> moves)
        {
            // init a chess board in start formation
            var board = ChessBoard.StartFormation;
            ChessDraw lastDraw = new ChessDraw();

            // apply the moves to the chess board
            foreach (var move in moves)
            {
                lastDraw = new ChessDraw(board, move.Item1, move.Item2);
                board.ApplyDraw(lastDraw);
            }

            // make sure that the algorithm recognizes the check situation
            Assert.True(new ChessDrawSimulator().GetCheckGameStatus(board, lastDraw) == CheckGameStatus.Check);

            // test whether the black player recognizes he is checked and needs to save his king
            var draws = board.BlackPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, lastDraw, true));

            // check whether the king is safe after each of the draws
            foreach (var draw in draws)
            {
                var simBoard = new ChessBoard(board.Pieces);
                simBoard.ApplyDraw(draw);
                Assert.True(new ChessDrawSimulator().GetCheckGameStatus(simBoard, draw) == CheckGameStatus.None);
            }
        }

        #endregion Tests
    }
}
