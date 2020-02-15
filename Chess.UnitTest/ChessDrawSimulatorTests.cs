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
                var enemyColor = allyColor.Opponent();

                var pieceXNewPos = new Dictionary<ChessPieceType, ChessPosition>()
                {
                    { ChessPieceType.Queen,   new ChessPosition(4, 4)                                         },
                    { ChessPieceType.Rook,    new ChessPosition(4, 4)                                         },
                    { ChessPieceType.Bishop,  new ChessPosition(4, 5)                                         },
                    { ChessPieceType.Knight,  new ChessPosition(5, 3)                                         },
                    { ChessPieceType.Peasant, new ChessPosition(((allyColor == ChessColor.White) ? 4 : 2), 4) },
                };

                // go through all drawing chess piece types (except king; king is already tested in king draws unit test)
                for (int pieceTypeValue = 2; pieceTypeValue < 7; pieceTypeValue++)
                {
                    var pieceType = (ChessPieceType)pieceTypeValue;
                    
                    // simulate situations with and without attacker (positive and negative test)
                    for (int putAttackerValue = 0; putAttackerValue < 2; putAttackerValue++)
                    {
                        bool putAttacker = (putAttackerValue == 1);
                        var oldPos = new ChessPosition(3, 4);

                        var pieces = new List<ChessPieceAtPos>()
                        {
                            new ChessPieceAtPos(oldPos,                  new ChessPiece(pieceType,           allyColor,  true)),
                            new ChessPieceAtPos(new ChessPosition(0, 7), new ChessPiece(ChessPieceType.King, allyColor,  true)),
                            new ChessPieceAtPos(new ChessPosition(7, 5), new ChessPiece(ChessPieceType.King, enemyColor, true)),
                        };

                        if (putAttacker) { pieces.Add(new ChessPieceAtPos(new ChessPosition(7, 0), new ChessPiece(ChessPieceType.Bishop, enemyColor, true))); }

                        var board = new ChessBoard(pieces);
                        var draw = new ChessDraw(board, oldPos, pieceXNewPos[pieceType]);

                        bool shouldBeDrawIntoCheck = putAttacker;
                        bool isDrawIntoCheck = ChessDrawSimulator.Instance.IsDrawIntoCheck(board, draw);
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
            Assert.True(ChessDrawSimulator.Instance.GetCheckGameStatus(board, enemyDraw) == ChessGameStatus.None);

            // test simple check
            var pieces = new List<ChessPieceAtPos>()
            {
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
                new ChessPieceAtPos(new ChessPosition(1, 5), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
                new ChessPieceAtPos(new ChessPosition(7, 3), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, true)),
                new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
            };
            
            board = new ChessBoard(pieces);
            enemyDraw = new ChessDraw(board, new ChessPosition(7, 3), new ChessPosition(6, 4));
            board.ApplyDraw(enemyDraw);
            Assert.True(ChessDrawSimulator.Instance.GetCheckGameStatus(board, enemyDraw) == ChessGameStatus.Check);

            // test checkmate
            for (int putSavingPieceValue = 0; putSavingPieceValue < 2; putSavingPieceValue++)
            {
                pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
                    new ChessPieceAtPos(new ChessPosition(6, 1), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
                    new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, true)),
                    new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
                };

                // allow the ally to avoid the checkmate by putting an additional chess piece on the board
                // and test if the simulator makes use of this opportunity
                bool putSavingPiece = (putSavingPieceValue == 1);
                if (putSavingPiece) { pieces.Add(new ChessPieceAtPos(new ChessPosition(1, 7), new ChessPiece(ChessPieceType.Rook, ChessColor.White, true))); }

                board = new ChessBoard(pieces);
                enemyDraw = new ChessDraw(board, new ChessPosition(2, 3), new ChessPosition(1, 4));
                board.ApplyDraw(enemyDraw);

                Assert.True(
                    (!putSavingPiece && ChessDrawSimulator.Instance.GetCheckGameStatus(board, enemyDraw) == ChessGameStatus.Checkmate)
                    || (putSavingPiece && ChessDrawSimulator.Instance.GetCheckGameStatus(board, enemyDraw) == ChessGameStatus.Check)
                );
            }

            // test stalemate
            pieces = new List<ChessPieceAtPos>()
            {
                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
                new ChessPieceAtPos(new ChessPosition(6, 1), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
                new ChessPieceAtPos(new ChessPosition(2, 3), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, true)),
                new ChessPieceAtPos(new ChessPosition(3, 5), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, true)),
                new ChessPieceAtPos(new ChessPosition(2, 4), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
                new ChessPieceAtPos(new ChessPosition(7, 1), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, true)),
            };

            board = new ChessBoard(pieces);
            enemyDraw = new ChessDraw(board, new ChessPosition(3, 5), new ChessPosition(2, 5));
            board.ApplyDraw(enemyDraw);
            Assert.True(ChessDrawSimulator.Instance.GetCheckGameStatus(board, enemyDraw) == ChessGameStatus.Stalemate);
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

        private static List<Tuple<ChessPosition, ChessPosition>> _moves3 = new List<Tuple<ChessPosition, ChessPosition>>()
        {
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G2"), new ChessPosition("G3")), // white peasant G2-G3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F7"), new ChessPosition("F5")), // black peasant F7-F5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("B4")), // white peasant B2-B4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("F7")), // black king    E8-F7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("G4")), // white peasant G3-G4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("D5")), // black peasant D7-D5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("D4")), // white peasant D2-D4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F5"), new ChessPosition("F4")), // black peasant F5-F4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("H3")), // white knight  G1-H3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("E8")), // black queen   D8-E8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C2"), new ChessPosition("C4")), // white peasant C2-C4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("D7")), // black knight  B8-D7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B4"), new ChessPosition("B5")), // white peasant B4-B5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("H6")), // black knight  G8-H6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("D5")), // white peasant C4-D5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F7"), new ChessPosition("F6")), // black king    F7-F6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H3"), new ChessPosition("G1")), // white knight  H3-G1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("B6")), // black knight  D7-B6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("A3")), // white knight  B1-A3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("A5")), // black peasant A7-A5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("C4")), // white knight  A3-C4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H6"), new ChessPosition("F7")), // black knight  H6-F7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A2"), new ChessPosition("A4")), // white peasant A2-A4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C7"), new ChessPosition("C5")), // black peasant C7-C5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("B6")), // white knight  C4-B6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C8"), new ChessPosition("G4")), // black bishop  C8-G4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H2"), new ChessPosition("H3")), // white peasant H2-H3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("E6")), // black peasant E7-E6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B6"), new ChessPosition("C4")), // white knight  B6-C4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("D8")), // black rook    A8-D8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D4"), new ChessPosition("C5")), // white peasant D4-C5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G4"), new ChessPosition("H5")), // black bishop  G4-H5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C4"), new ChessPosition("B2")), // white knight  C4-B2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F6"), new ChessPosition("E5")), // black king    F6-E5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E1"), new ChessPosition("D2")), // white king    E1-D2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H5"), new ChessPosition("E2")), // black bishop  H5-E2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H1"), new ChessPosition("H2")), // white rook    H1-H2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E5"), new ChessPosition("E4")), // black king    E5-E4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C5"), new ChessPosition("C6")), // white peasant C5-C6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("A8")), // black rook    D8-A8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("E2")), // white bishop  F1-E2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F8"), new ChessPosition("A3")), // black bishop  F8-A3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("C4")), // white knight  B2-C4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F7"), new ChessPosition("G5")), // black knight  F7-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D5"), new ChessPosition("D6")), // white peasant D5-D6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("C6")), // black queen   E8-C6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H3"), new ChessPosition("H4")), // white peasant H3-H4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C6"), new ChessPosition("D7")), // black queen   C6-D7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H4"), new ChessPosition("G5")), // white peasant H4-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("D6")), // black queen   D7-D6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("D3")), // white bishop  E2-D3

            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E4"), new ChessPosition("F5")), // black king    E4-F5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("B2")), // white bishop  C1-B2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F5"), new ChessPosition("G5")), // black king    F5-G5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H2"), new ChessPosition("H3")), // white rook    H2-H3
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D6"), new ChessPosition("E7")), // black queen   D6-E7
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("B1")), // white rook    A1-B1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("B2")), // black bishop  A3-B2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("E1")), // white queen   D1-E1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("E5")), // black bishop  B2-E5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H3"), new ChessPosition("H4")), // white rook    H3-H4
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E5"), new ChessPosition("C3")), // black bishop  E5-C3
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("C1")), // white king    D2-C1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("G8")), // black rook    A8-G8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("B2")), // white rook    B1-B2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("D6")), // black queen   E7-D6
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B5"), new ChessPosition("B6")), // white peasant B5-B6
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D6"), new ChessPosition("F8")), // black queen   D6-F8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D3"), new ChessPosition("B1")), // white bishop  D3-B1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F8"), new ChessPosition("E8")), // black queen   F8-E8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E1"), new ChessPosition("E2")), // white queen   E1-E2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("E7")), // black queen   E8-E7
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("H5")), // white queen   E2-H5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G5"), new ChessPosition("H4")), // black king    G5-H4
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("H7")), // white bishop  B1-H7
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H4"), new ChessPosition("H5")), // black king    H4-H5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("D1")), // white king    C1-D1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C3"), new ChessPosition("E5")), // black bishop  C3-E5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("C2")), // white rook    B2-C2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("A8")), // black rook    G8-A8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("E2")), // white king    D1-E2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H5"), new ChessPosition("H4")), // black king    H5-H4
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E3")), // white king    E2-E3
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("F8")), // black queen   E7-F8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E3"), new ChessPosition("F4")), // white king    E3-F4
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("A7")), // black rook    A8-A7
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F4"), new ChessPosition("E3")), // white king    F4-E3
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E5"), new ChessPosition("A1")), // black bishop  E5-A1
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E3"), new ChessPosition("E4")), // white king    E3-E4
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("B2")), // black bishop  A1-B2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C2"), new ChessPosition("D2")), // white rook    C2-D2
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("A8")), // black rook    A7-A8
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("D5")), // white rook    D2-D5
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F8"), new ChessPosition("D6")), // black queen   F8-D6
            //new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F2"), new ChessPosition("F4")), // white peasant F2-F4
        };

        private static List<Tuple<ChessPosition, ChessPosition>> _moves4 = new List<Tuple<ChessPosition, ChessPosition>>()
        {
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A2"), new ChessPosition("A3")), // white peasant A2-A3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G7"), new ChessPosition("G5")), // black peasant G7-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("F3")), // white knight  G1-F3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G8"), new ChessPosition("H6")), // black knight  G8-H6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G2"), new ChessPosition("G3")), // white peasant G2-G3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H6"), new ChessPosition("G4")), // black knight  H6-G4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E3")), // white peasant E2-E3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G4"), new ChessPosition("E5")), // black knight  G4-E5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("D3")), // white peasant D2-D3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E5"), new ChessPosition("D3")), // black knight  E5-D3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E1"), new ChessPosition("E2")), // white king    E1-E2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H7"), new ChessPosition("H5")), // black peasant H7-H5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("G5")), // white knight  F3-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D3"), new ChessPosition("B4")), // black knight  D3-B4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A3"), new ChessPosition("B4")), // white peasant A3-B4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("C6")), // black knight  B8-C6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("D4")), // white queen   D1-D4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F7"), new ChessPosition("F6")), // black peasant F7-F6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D4"), new ChessPosition("A7")), // white queen   D4-A7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H8"), new ChessPosition("H6")), // black rook    H8-H6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("F3")), // white king    E2-F3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F6"), new ChessPosition("G5")), // black peasant F6-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("G2")), // white bishop  F1-G2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G5"), new ChessPosition("G4")), // black peasant G5-G4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("E4")), // white king    F3-E4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C6"), new ChessPosition("D4")), // black knight  C6-D4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("A2")), // white rook    A1-A2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D4"), new ChessPosition("F3")), // black knight  D4-F3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("D2")), // white knight  B1-D2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C7"), new ChessPosition("C5")), // black peasant C7-C5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("B3")), // white peasant B2-B3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("D6")), // black peasant D7-D6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A7"), new ChessPosition("B8")), // white queen   A7-B8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("D7")), // black queen   D8-D7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("F1")), // white knight  D2-F1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D7"), new ChessPosition("D8")), // black queen   D7-D8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A2"), new ChessPosition("A4")), // white rook    A2-A4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H6"), new ChessPosition("G6")), // black rook    H6-G6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("D6")), // white queen   B8-D6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("B8")), // black rook    A8-B8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D6"), new ChessPosition("D4")), // white queen   D6-D4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("D5")), // black queen   D8-D5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E4"), new ChessPosition("F4")), // white king    E4-F4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("H4")), // black knight  F3-H4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D4"), new ChessPosition("F6")), // white queen   D4-F6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D5"), new ChessPosition("G2")), // black queen   D5-G2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F6"), new ChessPosition("F7")), // white queen   F6-F7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("F7")), // black king    E8-F7
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H1"), new ChessPosition("G1")), // white rook    H1-G1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G2"), new ChessPosition("F3")), // black queen   G2-F3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F4"), new ChessPosition("E5")), // white king    F4-E5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C8"), new ChessPosition("E6")), // black bishop  C8-E6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A4"), new ChessPosition("A1")), // white rook    A4-A1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("D8")), // black rook    B8-D8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("B2")), // white bishop  C1-B2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("D3")), // black rook    D8-D3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("C1")), // white rook    A1-C1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D3"), new ChessPosition("D8")), // black rook    D3-D8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("A1")), // white bishop  B2-A1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E6"), new ChessPosition("B3")), // black bishop  E6-B3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A1"), new ChessPosition("B2")), // white bishop  A1-B2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C5"), new ChessPosition("C4")), // black peasant C5-C4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("B1")), // white rook    C1-B1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("D2")), // black rook    D8-D2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("H2"), new ChessPosition("H3")), // white peasant H2-H3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G6"), new ChessPosition("B6")), // black rook    G6-B6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("H4")), // white peasant G3-H4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D2"), new ChessPosition("D1")), // black rook    D2-D1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B2"), new ChessPosition("D4")), // white bishop  B2-D4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B3"), new ChessPosition("C2")), // black bishop  B3-C2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G1"), new ChessPosition("G4")), // white rook    G1-G4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B6"), new ChessPosition("A6")), // black rook    B6-A6
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G4"), new ChessPosition("G5")), // white rook    G4-G5
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("F1")), // black rook    D1-F1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E3"), new ChessPosition("E4")), // white peasant E3-E4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A6"), new ChessPosition("A8")), // black rook    A6-A8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G5"), new ChessPosition("G3")), // white rook    G5-G3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C2"), new ChessPosition("E4")), // black bishop  C2-E4
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B1"), new ChessPosition("C1")), // white rook    B1-C1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("A8"), new ChessPosition("D8")), // black rook    A8-D8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("G2")), // white rook    G3-G2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E4"), new ChessPosition("D3")), // black bishop  E4-D3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D4"), new ChessPosition("A1")), // white bishop  D4-A1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D8"), new ChessPosition("C8")), // black rook    D8-C8
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G2"), new ChessPosition("G3")), // white rook    G2-G3
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D3"), new ChessPosition("E2")), // black bishop  D3-E2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("G3"), new ChessPosition("G2")), // white rook    G3-G2
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F1"), new ChessPosition("G1")), // black rook    F1-G1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("C1"), new ChessPosition("F1")), // white rook    C1-F1
            new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B7"), new ChessPosition("B5")), // black peasant B7-B5
        };
        
        [Fact]
        public void DefendCheckTest()
        {
            defendCheckTest(_moves1);
            //defendCheckTest(_moves2);
            defendCheckTest(_moves3);
            //defendCheckTest(_moves4);
        }
        
        private void defendCheckTest(List<Tuple<ChessPosition, ChessPosition>> moves)
        {
            var game = prepareGame(moves);
            var lastDraw = game.LastDraw;

            // make sure that the algorithm recognizes the check situation
            Assert.True(ChessDrawSimulator.Instance.GetCheckGameStatus(game.Board, lastDraw) == ChessGameStatus.Check);

            // test whether the black player recognizes he is checked and needs to save his king
            var alliedPieces = game.Board.GetPiecesOfColor(lastDraw.DrawingSide.Opponent());
            var draws = alliedPieces.SelectMany(piece => ChessDrawGenerator.Instance.GetDraws(game.Board, piece.Position, lastDraw, true));

            // check whether the king is safe after each of the draws
            foreach (var draw in draws)
            {
                Assert.True(!ChessDrawSimulator.Instance.IsDrawIntoCheck(game.Board, draw));
            }
        }

        [Fact]
        public void DrawsBugTest()
        {
            var game = prepareGame(_moves4);
            var alliedPieces = game.Board.GetPiecesOfColor(game.SideToDraw);

            try
            {
                var draws = new List<ChessDraw>();

                foreach (var piece in alliedPieces)
                {
                    draws.AddRange(ChessDrawGenerator.Instance.GetDraws(game.Board, piece.Position, game.LastDraw, true));
                }

                draws.ForEach(x => output.WriteLine(x.ToString()));
            }
            catch (Exception /*ex*/)
            {
            }
        }

        #region Helpers

        private ChessGame prepareGame(List<Tuple<ChessPosition, ChessPosition>> moves)
        {
            // init a chess board in start formation
            var game = new ChessGame();
            ChessDraw lastDraw = new ChessDraw();

            // apply the moves to the chess board
            foreach (var move in moves)
            {
                lastDraw = new ChessDraw(game.Board, move.Item1, move.Item2);
                game.ApplyDraw(lastDraw);

                var status = ChessDrawSimulator.Instance.GetCheckGameStatus(game.Board, lastDraw);
                Assert.True(status == ChessGameStatus.Check || status == ChessGameStatus.None);
            }

            return game;
        }

        #endregion Helpers

        #endregion Tests
    }
}
