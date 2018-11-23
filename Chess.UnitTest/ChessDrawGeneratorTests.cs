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
        private int attColDiff;
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
                    new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(6, 1),     new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(5, 2),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.Black,  WasMoved = true }),
                };

                // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                var board = new ChessBoard(pieces);
                var draws = new ChessDrawGenerator().GetDraws(board, board.WhiteKing.Position, new ChessDraw(), true);
                Assert.True(draws.Count() == ((row + col == 7) ? 2 : 3));

                // evaluate black king draws (when white king is onto A8, then the white peasant cannot be taken because of draw into check by the black king)
                board = new ChessBoard(pieces);
                draws = new ChessDrawGenerator().GetDraws(board, board.BlackKing.Position, new ChessDraw(), true);
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
                var enemyColor = (rookRow == 0) ? ChessColor.Black : ChessColor.White;

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

                        var pieces = new List<ChessPieceAtPos>()
                        {
                            new ChessPieceAtPos(oldKingPos,   new ChessPiece() { Type = ChessPieceType.King, Color = allyColor,  WasMoved = wasKingMoved }),
                            new ChessPieceAtPos(oldRookPos,   new ChessPiece() { Type = ChessPieceType.Rook, Color = allyColor,  WasMoved = wasRookMoved }),
                            new ChessPieceAtPos(enemyPos,     new ChessPiece() { Type = ChessPieceType.Rook, Color = enemyColor, WasMoved = true         }),
                            new ChessPieceAtPos(enemyKingPos, new ChessPiece() { Type = ChessPieceType.King, Color = enemyColor, WasMoved = true         }),
                        };

                        // init chess board and rochade draw
                        var board = new ChessBoard(pieces);
                        var draw = new ChessDraw(board, pieces[0].Position, newKingPos);

                        // check whether the draw validation returns the expected value
                        bool shouldRochadeBeValid = !wasKingMoved && !wasRookMoved && attack >= 3;
                        bool isRochadeValid = draw.IsValid(board, new ChessDraw());
                        Assert.True(shouldRochadeBeValid == isRochadeValid);

                        // check whether the rochade is applied correctly to the chess board
                        if (isRochadeValid)
                        {
                            board.ApplyDraw(draw);
                            Assert.True(
                                   board.GetPieceAt(oldKingPos) == null && board.GetPieceAt(newKingPos).Value.Type == ChessPieceType.King && board.GetPieceAt(newKingPos).Value.Color == allyColor
                                && board.GetPieceAt(oldRookPos) == null && board.GetPieceAt(newRookPos).Value.Type == ChessPieceType.Rook && board.GetPieceAt(newRookPos).Value.Color == allyColor
                            );
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
                // get the (row, column) of the white king
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                var pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(4, 3),     new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black,  WasMoved = true }),
                };

                // evaluate queen draws
                var board = new ChessBoard(pieces);
                var draws = new ChessDrawGenerator().GetDraws(board, pieces[0].Position, new ChessDraw(), true);
                Assert.True(draws.Count() == ((row + col == 7) ? 12 : 16));
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
                // get the (row, column) of the white king
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                // TODO: implement test

                //var pieces = new List<ChessPieceAtPos>()
                //{
                //    new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.Queen,   Color = ChessColor.White,  WasMoved = true }),
                //    new ChessPieceAtPos(new ChessPosition(4, 3),     new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White,  WasMoved = true }),
                //    new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White,  WasMoved = true }),
                //    new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.White,  WasMoved = true }),
                //    new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black,  WasMoved = true }),
                //};

                //// evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                //var board = new ChessBoard(pieces);
                //var draws = new ChessDrawGenerator().GetDraws(board, pieces[0].Position, new ChessDraw(), true);
                //Assert.True(draws.Count() == ((row + col == 7) ? 12 : 16));
            }
        }

        #endregion RookDraws

        #region BishopDraws

        [Fact]
        public void BishopDrawsTest()
        {
            // TODO: implement test
        }

        #endregion BishopDraws

        #region KnightDraws

        [Fact]
        public void KnightDrawsTest()
        {
            // TODO: implement test
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
            //for (int rowOfSide = 1; rowOfSide < 8; rowOfSide++)
            //{
            //    int row = ;
            //}

            // TODO: implement logic
        }

        private void testCatchDraws()
        {
            // TODO: implement logic
        }

        private void testEnPassant()
        {
            // simulate for white and black side
            for (int dfwColorValue = 0; dfwColorValue < 2; dfwColorValue++)
            {
                var dfwColor = (ChessColor)dfwColorValue;
                var epColor = (dfwColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;

                // get the column where the peasant is moving foreward
                for (int fwCol = 0; fwCol < ChessBoard.CHESS_BOARD_DIMENSION; fwCol++)
                {
                    int dfwRow = (dfwColor == ChessColor.White) ? 1 : 6;
                    int attRow = (dfwColor == ChessColor.White) ? 3 : 4;
                    var dfwOldPos = new ChessPosition(dfwRow, fwCol);
                    var dfwNewPos = new ChessPosition(attRow, fwCol);

                    // get the column where the en-passant peasant is placed
                    for (int attCol = 0; attCol < ChessBoard.CHESS_BOARD_DIMENSION; attCol++)
                    {
                        if (fwCol != attCol)
                        {
                            var attOldPos = new ChessPosition(attRow, attCol);
                            var attNewPos = new ChessPosition(attRow + ((dfwColor == ChessColor.White) ? -1 : 1), fwCol);

                            var pieces = new List<ChessPieceAtPos>()
                            {
                                new ChessPieceAtPos(dfwOldPos,               new ChessPiece() { Type = ChessPieceType.Peasant, Color = dfwColor,         WasMoved = false }),
                                new ChessPieceAtPos(attOldPos,               new ChessPiece() { Type = ChessPieceType.Peasant, Color = epColor,          WasMoved = true  }),
                                new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = false }),
                                new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = false }),
                            };

                            // apply the double foreward draw as preparation for the en-passant
                            var board = new ChessBoard(pieces);
                            var drawDfw = new ChessDraw(board, dfwOldPos, dfwNewPos);
                            board.ApplyDraw(drawDfw);

                            // create the en-passant draw and validate it
                            var drawEp = new ChessDraw(board, attOldPos, attNewPos);
                            bool shouldDrawBeValid = Math.Abs(attCol - fwCol) == 1;
                            bool isDrawValid = drawEp.IsValid(board, drawDfw);
                            Assert.True(shouldDrawBeValid == isDrawValid);

                            if (shouldDrawBeValid)
                            {
                                // check if the en-passant draw gets correctly applied to the chess board
                                board.ApplyDraw(drawEp);
                                Assert.True(board.GetPieceAt(dfwNewPos) == null && board.GetPieceAt(attNewPos).Value == pieces[1].Piece);
                            }
                        }
                    }
                }
            }
        }

        private void testPeasantPromotion()
        {
            // get the column where the peasant is moving foreward
            for (int fwCol = 0; fwCol < ChessBoard.CHESS_BOARD_DIMENSION; fwCol++)
            {

            }

            // TODO: implement logic
        }

        #endregion PeasantDraws

        #endregion Tests
    }
}
