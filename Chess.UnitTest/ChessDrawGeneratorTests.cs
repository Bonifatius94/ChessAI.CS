﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class ChessDrawGeneratorTests : TestBase
    {
        #region Constructor

        public ChessDrawGeneratorTests(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        #region KingDraws

        [Fact]
        public void KingDrawsTest()
        {
            try
            {
                testStandardKingDraws();
                testRochadeKingDraws();
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.ToString());
                Assert.True(false);
            }
        }

        private void testStandardKingDraws()
        {
            // check if only draws are computed where the destination is actually on the chess board
            // and also make sure that only enemy pieces are taken (blocking allied pieces not)
            for (int run = 0; run < 4; run++)
            {
                // get the (row, column) of the white king
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                var pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
                    new ChessPieceAtPos(new ChessPosition(6, 1),     new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
                    new ChessPieceAtPos(new ChessPosition(5, 2),     new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
                    new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, true)),
                };

                // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                var board = new ChessBoard(pieces);
                var draws = ChessDrawGenerator.Instance.GetDraws(board, board.WhiteKing.Position, null, true);
                Assert.True(draws.Count() == ((row + col == 7) ? 2 : 3));

                // evaluate black king draws (when white king is onto A8, then the white peasant cannot be taken because of draw into check by the black king)
                board = new ChessBoard(pieces);
                draws = ChessDrawGenerator.Instance.GetDraws(board, board.BlackKing.Position, null, true);
                Assert.True(draws.Count() == ((row == 7 && col == 0) ? 7 : 8));
            }
        }

        private void testRochadeKingDraws()
        {
            // go through each king x rook combo
            for (int run = 0; run < 4; run++)
            {
                // get the (row, column) of the rook before rochade
                int rookRow = run / 2 == 0 ? 0 : 7;
                int rookCol = run % 2 == 0 ? 0 : 7;

                // determine the old and new positions of king / rook after rochade
                var oldKingPos = new ChessPosition(rookRow, 4);
                var newKingPos = new ChessPosition(rookRow, (rookCol == 0 ? 2 : 6));
                var oldRookPos = new ChessPosition(rookRow, rookCol);
                var newRookPos = new ChessPosition(rookRow, (rookCol == 0 ? 3 : 5));

                // determine the side performing a rochade
                var allyColor = (rookRow == 0) ? ChessColor.White : ChessColor.Black;
                var enemyColor = allyColor.Opponent();

                // go through all 4 tuples bool x bool; only (false, false) should enable a rochade
                for (int wasMovedValue = 0; wasMovedValue < 4; wasMovedValue++)
                {
                    // determine whether king / rook was already moved before the rochade
                    bool wasKingMoved = wasMovedValue / 2 == 0;
                    bool wasRookMoved = wasMovedValue % 2 == 0;

                    // attach with a rook and go through every scenario threatening the king's rochade passage
                    for (int attack = 0; attack < 4; attack++)
                    {
                        var enemyPos = new ChessPosition(4, (rookCol == 0) ? (4 - attack) : (4 + attack));
                        var enemyKingPos = new ChessPosition(4, 0);

                        for (int block = 0; block < 2; block++)
                        {
                            bool isBlocked = block == 1;
                            var blockPos = new ChessPosition(rookRow, 1);

                            var pieces = new List<ChessPieceAtPos>()
                            {
                                new ChessPieceAtPos(oldKingPos,   new ChessPiece(ChessPieceType.King, allyColor,  wasKingMoved)),
                                new ChessPieceAtPos(oldRookPos,   new ChessPiece(ChessPieceType.Rook, allyColor,  wasRookMoved)),
                                new ChessPieceAtPos(enemyPos,     new ChessPiece(ChessPieceType.Rook, enemyColor, true        )),
                                new ChessPieceAtPos(enemyKingPos, new ChessPiece(ChessPieceType.King, enemyColor, true        )),
                            };

                            if (isBlocked) { pieces.Add(new ChessPieceAtPos(blockPos, new ChessPiece(ChessPieceType.Knight, allyColor, false))); }

                            // init chess board and rochade draw
                            IChessBoard board = new ChessBoard(pieces);
                            var draw = new ChessDraw(board, pieces[0].Position, newKingPos);

                            // check whether the draw validation returns the expected value
                            bool shouldRochadeBeValid = !wasKingMoved && !wasRookMoved && attack >= 3 && (!isBlocked || rookCol != 0);
                            bool isRochadeValid = draw.IsValid(board);

                            Assert.True(shouldRochadeBeValid == isRochadeValid);

                            // check whether the rochade is applied correctly to the chess board
                            if (isRochadeValid)
                            {
                                board = board.ApplyDraw(draw);
                                Assert.True(
                                       !board.IsCapturedAt(oldKingPos) && board.GetPieceAt(newKingPos).Type == ChessPieceType.King && board.GetPieceAt(newKingPos).Color == allyColor
                                    && !board.IsCapturedAt(oldRookPos) && board.GetPieceAt(newRookPos).Type == ChessPieceType.Rook && board.GetPieceAt(newRookPos).Color == allyColor
                                );
                            }
                        }
                    }
                }
            }
        }

        #endregion KingDraws

        #region QueenDraws

        [Fact]
        public void QueenDrawsTest()
        {
            // check if only draws are computed where the destination is actually on the chess board
            // and also make sure that only enemy pieces are taken (blocking allied pieces not)
            for (int run = 0; run < 4; run++)
            {
                // get the (row, column) of the white rook
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                // test draws of white / black queens
                for (int colorValue = 0; colorValue < 2; colorValue++)
                {
                    var allyColor = (ChessColor)colorValue;
                    var enemyColor = allyColor.Opponent();

                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece(ChessPieceType.Queen,   allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(4, 3),     new ChessPiece(ChessPieceType.Peasant, allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece(ChessPieceType.King,    allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece(ChessPieceType.Bishop,  allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece(ChessPieceType.King,    enemyColor, true)),
                    };

                    // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                    IChessBoard board = new ChessBoard(pieces);
                    var draws = ChessDrawGenerator.Instance.GetDraws(board, pieces[0].Position, null, true);
                    Assert.True(draws.Count() == ((row + col == 7) ? 12 : 16));

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board = board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece(ChessPieceType.Queen, allyColor, true);
                        Assert.True(!board.IsCapturedAt(draw.OldPosition)  && board.GetPieceAt(draw.NewPosition) == pieceCmp);
                    }
                }
            }
        }

        #endregion QueenDraws

        #region RookDraws

        [Fact]
        public void RookDrawsTest()
        {
            // check if only draws are computed where the destination is actually on the chess board
            // and also make sure that only enemy pieces are taken (blocking allied pieces not)
            for (int run = 0; run < 4; run++)
            {
                // get the (row, column) of the white queen
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                // test draws of white / black rooks
                for (int colorValue = 0; colorValue < 2; colorValue++)
                {
                    var allyColor = (ChessColor)colorValue;
                    var enemyColor = allyColor.Opponent();

                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece(ChessPieceType.Rook,   allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(0, 3),     new ChessPiece(ChessPieceType.Bishop, allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(7, 4),     new ChessPiece(ChessPieceType.Queen,  allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(3, 4),     new ChessPiece(ChessPieceType.King,   allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece(ChessPieceType.Bishop, enemyColor, true)),
                        new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece(ChessPieceType.Queen,  enemyColor, true)),
                        new ChessPieceAtPos(new ChessPosition(4, 2),     new ChessPiece(ChessPieceType.King,   enemyColor, true)),
                    };

                    // evaluate queen draws
                    IChessBoard board = new ChessBoard(pieces);
                    var draws = ChessDrawGenerator.Instance.GetDraws(board, pieces[0].Position, null, true);
                    Assert.True(draws.Count() == ((row + col == 7) ? 7 : 5));

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board = board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece(ChessPieceType.Rook, allyColor, true);
                        Assert.True(!board.IsCapturedAt(draw.OldPosition) && board.GetPieceAt(draw.NewPosition) == pieceCmp);
                    }
                }
            }
        }

        #endregion RookDraws

        #region BishopDraws

        [Fact]
        public void BishopDrawsTest()
        {
            // test draws of white / black bishops
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var allyColor = (ChessColor)colorValue;
                var enemyColor = allyColor.Opponent();
                var bishopPos = new ChessPosition(4, 4);

                var pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(bishopPos,               new ChessPiece(ChessPieceType.Bishop, allyColor,  true)),
                    new ChessPieceAtPos(new ChessPosition(0, 0), new ChessPiece(ChessPieceType.Queen,  allyColor,  true)),
                    new ChessPieceAtPos(new ChessPosition(7, 7), new ChessPiece(ChessPieceType.Knight, allyColor,  true)),
                    new ChessPieceAtPos(new ChessPosition(0, 6), new ChessPiece(ChessPieceType.King,   allyColor,  true)),
                    new ChessPieceAtPos(new ChessPosition(6, 2), new ChessPiece(ChessPieceType.Bishop, enemyColor, true)),
                    new ChessPieceAtPos(new ChessPosition(1, 7), new ChessPiece(ChessPieceType.Knight, enemyColor, true)),
                    new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece(ChessPieceType.King,   enemyColor, true)),
                };

                IChessBoard board = new ChessBoard(pieces);
                var draws = ChessDrawGenerator.Instance.GetDraws(board, bishopPos, null, true);
                Assert.True(draws.Count() == 10);

                // check if the draws are correctly applied to the chess board
                foreach (var draw in draws)
                {
                    board = new ChessBoard(pieces);
                    board = board.ApplyDraw(draw);
                    var pieceCmp = new ChessPiece(ChessPieceType.Bishop, allyColor, true);
                    Assert.True(!board.IsCapturedAt(draw.OldPosition) && board.GetPieceAt(draw.NewPosition) == pieceCmp);
                }
            }
        }

        #endregion BishopDraws

        #region KnightDraws

        [Fact]
        public void KnightDrawsTest()
        {
            // test draws of white / black knights
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var allyColor = (ChessColor)colorValue;
                var enemyColor = allyColor.Opponent();

                var knightPositions = new List<ChessPosition>()
                {
                    new ChessPosition(1, 2),
                    new ChessPosition(2, 6),
                    new ChessPosition(6, 4),
                    new ChessPosition(5, 1),
                };

                // go through all knight positions (including positions at the margin of the chess board, etc.)
                foreach (var knightPos in knightPositions)
                {
                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(knightPos,               new ChessPiece(ChessPieceType.Knight,  allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(4, 3), new ChessPiece(ChessPieceType.Peasant, allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(0, 7), new ChessPiece(ChessPieceType.Rook,    allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    allyColor,  true)),
                        new ChessPieceAtPos(new ChessPosition(4, 7), new ChessPiece(ChessPieceType.Peasant, enemyColor, true)),
                        new ChessPieceAtPos(new ChessPosition(7, 2), new ChessPiece(ChessPieceType.Queen,   enemyColor, true)),
                        new ChessPieceAtPos(new ChessPosition(6, 5), new ChessPiece(ChessPieceType.King,    enemyColor, true)),
                    };

                    IChessBoard board = new ChessBoard(pieces);
                    var draws = ChessDrawGenerator.Instance.GetDraws(board, knightPos, null, true);
                    Assert.True(draws.Count() == 5);

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board = board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece(ChessPieceType.Knight, allyColor, true);
                        Assert.True(!board.IsCapturedAt(draw.OldPosition) && board.GetPieceAt(draw.NewPosition) == pieceCmp);
                    }
                }
            }
        }

        #endregion KnightDraws

        #region PeasantDraws

        [Fact]
        public void PeasantDrawsTest()
        {
            try
            {
                testOneAndDoubleForeward();
                testCatchDraws();
                testEnPassant();
                testPeasantPromotion();
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.ToString());
                Assert.True(false);
            }
        }

        private void testOneAndDoubleForeward()
        {
            // simulate for white and black side
            for (int dfwColorValue = 0; dfwColorValue < 2; dfwColorValue++)
            {
                var allyColor = (ChessColor)dfwColorValue;

                // get the column where the peasant is moving foreward
                for (int col = 0; col < 8; col++)
                {
                    int oldRow    = (allyColor == ChessColor.White) ? 1 : 6;
                    int sfwNewRow = (allyColor == ChessColor.White) ? 2 : 5;
                    int dfwNewRow = (allyColor == ChessColor.White) ? 3 : 4;
                    var oldPos = new ChessPosition(oldRow, col);
                    var sfwNewPos = new ChessPosition(sfwNewRow, col);
                    var dfwNewPos = new ChessPosition(dfwNewRow, col);

                    // check if the draw is only valid if the peasant was not already moved
                    for (int wasMovedValue = 0; wasMovedValue < 2; wasMovedValue++)
                    {
                        var wasMoved = (wasMovedValue == 1);

                        // check if blocking pieces of both colors are taken in consideration
                        for (int blockingPieceRowDiff = 2; blockingPieceRowDiff < 4; blockingPieceRowDiff++)
                        {
                            int blockRow = (allyColor == ChessColor.White) ? (oldRow + blockingPieceRowDiff) : (oldRow - blockingPieceRowDiff);
                            var blockPos = new ChessPosition(blockRow, col);

                            for (int bpColorValue = 0; bpColorValue < 2; bpColorValue++)
                            {
                                var bpColor = (ChessColor)bpColorValue;
                                //output.WriteLine($"testing constellation: allyColor={ allyColor.ToString() }, sfwNewPos={ sfwNewPos }, dfwNewPos={ dfwNewPos }, blockPos={ blockPos }, wasMoved={ wasMoved }");

                                var pieces = new List<ChessPieceAtPos>()
                                {
                                    new ChessPieceAtPos(oldPos,                  new ChessPiece(ChessPieceType.Peasant, allyColor,        wasMoved)),
                                    new ChessPieceAtPos(blockPos,                new ChessPiece(ChessPieceType.Peasant, bpColor,          wasMoved)),
                                    new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    ChessColor.White, false   )),
                                    new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece(ChessPieceType.King,    ChessColor.Black, false   )),
                                };

                                IChessBoard board = new ChessBoard(pieces);

                                var sfwDraw = new ChessDraw(board, oldPos, sfwNewPos);
                                bool shouldSfwBeValid = (blockingPieceRowDiff > 1);
                                bool isSfwValid = sfwDraw.IsValid(board);
                                Assert.True(shouldSfwBeValid == isSfwValid);

                                var dfwDraw = new ChessDraw(board, oldPos, dfwNewPos);
                                bool shouldDfwBeValid = (wasMoved == false) && (blockingPieceRowDiff > 2);
                                bool isDfwValid = dfwDraw.IsValid(board);
                                Assert.True(shouldDfwBeValid == isDfwValid);

                                if (isSfwValid)
                                {
                                    var pieceCmp = new ChessPiece(ChessPieceType.Peasant, allyColor, true);

                                    // check if the chess piece is moved correctly
                                    board = new ChessBoard(pieces);
                                    board = board.ApplyDraw(dfwDraw);
                                    Assert.True(!board.IsCapturedAt(oldPos) && board.GetPieceAt(dfwNewPos) == pieceCmp);
                                }

                                if (isDfwValid)
                                {
                                    var pieceCmp = new ChessPiece(ChessPieceType.Peasant, allyColor, !wasMoved);

                                    // check if the chess piece is moved correctly
                                    board = new ChessBoard(pieces);
                                    board = board.ApplyDraw(dfwDraw);
                                    Assert.True(!board.IsCapturedAt(oldPos) && board.GetPieceAt(dfwNewPos) == pieceCmp);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void testCatchDraws()
        {
            // simulate for white and black side for each ally and target chess pieces 
            for (int colorValues = 0; colorValues < 4; colorValues++)
            {
                var allyColor = (ChessColor)(colorValues / 2);
                var targetColor = (ChessColor)(colorValues % 2);

                // check for all rows on the chess board
                for (int rowDiff = 0; rowDiff < 6; rowDiff++)
                {
                    int allyRow = (allyColor == ChessColor.White) ? (1 + rowDiff) : (6 - rowDiff);
                    int nextRow = (allyColor == ChessColor.White) ? (allyRow + 1) : (allyRow - 1);

                    // get the column where the peasant is moving foreward
                    for (int allyCol = 0; allyCol < 8; allyCol++)
                    {
                        var oldPos = new ChessPosition(allyRow, allyCol);

                        for (int targetColMiddle = 1; targetColMiddle < 7; targetColMiddle++)
                        {
                            var targetPosLeft  = new ChessPosition(nextRow, targetColMiddle - 1);
                            var targetPosRight = new ChessPosition(nextRow, targetColMiddle + 1);

                            var allyPeasant  = new ChessPiece(ChessPieceType.Peasant, allyColor,   true);
                            var enemyPeasant = new ChessPiece(ChessPieceType.Peasant, targetColor, true);
                            int kingsRow = (allyColor == ChessColor.White) ? 0 : 7;

                            var pieces = new List<ChessPieceAtPos>()
                            {
                                new ChessPieceAtPos(oldPos, allyPeasant),
                                new ChessPieceAtPos(targetPosLeft, enemyPeasant),
                                new ChessPieceAtPos(targetPosRight, enemyPeasant),
                                new ChessPieceAtPos(new ChessPosition(kingsRow, 0), new ChessPiece(ChessPieceType.King, ChessColor.White, false)),
                                new ChessPieceAtPos(new ChessPosition(kingsRow, 7), new ChessPiece(ChessPieceType.King, ChessColor.Black, false)),
                            };

                            //output.WriteLine($"current constellation: allyColor={allyColor}, targetColor={targetColor}, allyRow={allyRow}, allyCol={allyCol}, targetColMiddle={targetColMiddle}");

                            IChessBoard board = new ChessBoard(pieces);
                            var catchDraws = ChessDrawGenerator.Instance.GetDraws(board, oldPos, null, true).Where(x => x.OldPosition.Column != x.NewPosition.Column);

                            int expectedCatchDrawsCount = (targetColor == allyColor) ? 0 : (targetColMiddle == allyCol) ? 2 : ((Math.Abs(targetColMiddle - allyCol) == 2) ? 1 : 0);
                            expectedCatchDrawsCount = ((rowDiff == 5) ? 4 : 1) * expectedCatchDrawsCount;

                            Assert.True(catchDraws.Count() == expectedCatchDrawsCount);

                            // check if the draws are correctly applied to the chess board
                            foreach (var draw in catchDraws)
                            {
                                board = new ChessBoard(pieces);
                                board = board.ApplyDraw(draw);
                                var pieceCmp = new ChessPiece(ChessPieceType.Peasant, allyColor, true);
                                Assert.True(!board.IsCapturedAt(draw.OldPosition) && (board.GetPieceAt(draw.NewPosition) == pieceCmp || (rowDiff == 5)));
                            }
                        }
                    }
                }
            }
        }

        private void testEnPassant()
        {
            // simulate for white and black side
            for (int dfwColorValue = 0; dfwColorValue < 2; dfwColorValue++)
            {
                var dfwColor = (ChessColor)dfwColorValue;
                var epColor = dfwColor.Opponent();

                // get the column where the peasant is moving foreward
                for (int fwCol = 0; fwCol < 8; fwCol++)
                {
                    int dfwRow = (dfwColor == ChessColor.White) ? 1 : 6;
                    int attRow = (dfwColor == ChessColor.White) ? 3 : 4;
                    var dfwOldPos = new ChessPosition(dfwRow, fwCol);
                    var dfwNewPos = new ChessPosition(attRow, fwCol);

                    // get the column where the en-passant peasant is placed
                    for (int attCol = 0; attCol < 8; attCol++)
                    {
                        if (fwCol != attCol)
                        {
                            var attOldPos = new ChessPosition(attRow, attCol);
                            var attNewPos = new ChessPosition(attRow + ((dfwColor == ChessColor.White) ? -1 : 1), fwCol);

                            var pieces = new List<ChessPieceAtPos>()
                            {
                                new ChessPieceAtPos(dfwOldPos,               new ChessPiece(ChessPieceType.Peasant, dfwColor,         false)),
                                new ChessPieceAtPos(attOldPos,               new ChessPiece(ChessPieceType.Peasant, epColor,          true )),
                                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece(ChessPieceType.King,    ChessColor.White, false)),
                                new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece(ChessPieceType.King,    ChessColor.Black, false)),
                            };

                            // apply the double foreward draw as preparation for the en-passant
                            IChessBoard board = new ChessBoard(pieces);
                            var drawDfw = new ChessDraw(board, dfwOldPos, dfwNewPos);
                            board = board.ApplyDraw(drawDfw);

                            // create the en-passant draw and validate it
                            var drawEp = new ChessDraw(board, attOldPos, attNewPos);
                            bool shouldDrawBeValid = Math.Abs(attCol - fwCol) == 1;
                            bool isDrawValid = drawEp.IsValid(board, drawDfw);
                            Assert.True(shouldDrawBeValid == isDrawValid);

                            if (shouldDrawBeValid)
                            {
                                // check if the en-passant draw gets correctly applied to the chess board
                                board = board.ApplyDraw(drawEp);
                                Assert.True(!board.IsCapturedAt(dfwNewPos) && board.GetPieceAt(attNewPos) == pieces[1].Piece);
                            }
                        }
                    }
                }
            }
        }

        private void testPeasantPromotion()
        {
            // simulate for white and black side
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var allyColor = (ChessColor)colorValue;
                var enemyColor = allyColor.Opponent();
                
                // go through all columns where the promoting peasant is moving foreward
                for (int fwCol = 0; fwCol < 8; fwCol++)
                {
                    int fwRow = (allyColor == ChessColor.White) ? 6 : 1;
                    var fwPos = new ChessPosition(fwRow, fwCol);
                    
                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(4, 0), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
                        new ChessPieceAtPos(new ChessPosition(4, 7), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
                        new ChessPieceAtPos(fwPos,                   new ChessPiece(ChessPieceType.Peasant, allyColor,        true)),
                    };

                    int catchRow = (allyColor == ChessColor.White) ? 7 : 0;
                    int leftCatchCol  = fwCol - 1;
                    int rightCatchCol = fwCol + 1;

                    var enemyPeasant = new ChessPiece(ChessPieceType.Peasant, enemyColor, true);
                    ChessPosition posCatchLeft;
                    ChessPosition posCatchRight;

                    if (ChessPosition.AreCoordsValid(catchRow, leftCatchCol))
                    {
                        posCatchLeft = new ChessPosition(catchRow, leftCatchCol);
                        pieces.Add(new ChessPieceAtPos(posCatchLeft, enemyPeasant));
                    }

                    if (ChessPosition.AreCoordsValid(catchRow, rightCatchCol))
                    {
                        posCatchRight = new ChessPosition(catchRow, rightCatchCol);
                        pieces.Add(new ChessPieceAtPos(posCatchRight, enemyPeasant));
                    }

                    IChessBoard board = new ChessBoard(pieces);
                    var draws = ChessDrawGenerator.Instance.GetDraws(board, fwPos, null, true);
                    Assert.True(draws.Count() ==  (4 * ((fwCol % 7 == 0) ? 2 : 3)));
                        
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board = board.ApplyDraw(draw);
                        Assert.True(board.AllPieces.All(x => x.Piece.Color == enemyColor || x.Piece.Type != ChessPieceType.Peasant));
                    }
                }
            }
        }

        #endregion PeasantDraws

        #endregion Tests
    }
}
