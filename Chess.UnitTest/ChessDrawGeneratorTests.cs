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
                    new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(6, 1),     new ChessPiece() { Type = ChessPieceType.Peasant, Color = ChessColor.White,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(5, 2),     new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = ChessColor.Black,  WasMoved = true }),
                };

                // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                var board = new ChessBoard(pieces);
                var draws = new ChessDrawGenerator().GetDraws(board, board.WhiteKing.Position, null, true);
                Assert.True(draws.Count() == ((row + col == 7) ? 2 : 3));

                // evaluate black king draws (when white king is onto A8, then the white peasant cannot be taken because of draw into check by the black king)
                board = new ChessBoard(pieces);
                draws = new ChessDrawGenerator().GetDraws(board, board.BlackKing.Position, null, true);
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
                        bool isRochadeValid = draw.IsValid(board);
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
                // get the (row, column) of the white rook
                int row = run / 2 == 0 ? 0 : 7;
                int col = run % 2 == 0 ? 0 : 7;

                // test draws of white / black queens
                for (int colorValue = 0; colorValue < 2; colorValue++)
                {
                    var allyColor = (ChessColor)colorValue;
                    var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;

                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.Queen,   Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 3),     new ChessPiece() { Type = ChessPieceType.Peasant, Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece() { Type = ChessPieceType.King,    Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(2, 5),     new ChessPiece() { Type = ChessPieceType.King,    Color = enemyColor, WasMoved = true }),
                    };

                    // evaluate white king draws (when white king is onto A8, then the white peasant is blocking)
                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawGenerator().GetDraws(board, pieces[0].Position, null, true);
                    Assert.True(draws.Count() == ((row + col == 7) ? 12 : 16));

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece() { Type = ChessPieceType.Queen, Color = allyColor, WasMoved = true };
                        Assert.True(board.GetPieceAt(draw.OldPosition) == null && board.GetPieceAt(draw.NewPosition).Value == pieceCmp);
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
                    var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;

                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(row, col), new ChessPiece() { Type = ChessPieceType.Rook,    Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(0, 3),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(7, 4),     new ChessPiece() { Type = ChessPieceType.Queen,   Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(3, 4),     new ChessPiece() { Type = ChessPieceType.King,    Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(3, 0),     new ChessPiece() { Type = ChessPieceType.Bishop,  Color = enemyColor, WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 7),     new ChessPiece() { Type = ChessPieceType.Queen,   Color = enemyColor, WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 2),     new ChessPiece() { Type = ChessPieceType.King,    Color = enemyColor, WasMoved = true }),
                    };

                    // evaluate queen draws
                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawGenerator().GetDraws(board, pieces[0].Position, null, true);
                    Assert.True(draws.Count() == ((row + col == 7) ? 7 : 5));

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece() { Type = ChessPieceType.Rook, Color = allyColor, WasMoved = true };
                        Assert.True(board.GetPieceAt(draw.OldPosition) == null && board.GetPieceAt(draw.NewPosition).Value == pieceCmp);
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
                var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;
                var bishopPos = new ChessPosition(4, 4);

                var pieces = new List<ChessPieceAtPos>()
                {
                    new ChessPieceAtPos(bishopPos,               new ChessPiece() { Type = ChessPieceType.Bishop, Color = allyColor,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(0, 0), new ChessPiece() { Type = ChessPieceType.Queen,  Color = allyColor,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(7, 7), new ChessPiece() { Type = ChessPieceType.Knight, Color = allyColor,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(0, 6), new ChessPiece() { Type = ChessPieceType.King,   Color = allyColor,  WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(6, 2), new ChessPiece() { Type = ChessPieceType.Bishop, Color = enemyColor, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(1, 7), new ChessPiece() { Type = ChessPieceType.Knight, Color = enemyColor, WasMoved = true }),
                    new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece() { Type = ChessPieceType.King,   Color = enemyColor, WasMoved = true }),
                };

                var board = new ChessBoard(pieces);
                var draws = new ChessDrawGenerator().GetDraws(board, bishopPos, null, true);
                Assert.True(draws.Count() == 10);

                // check if the draws are correctly applied to the chess board
                foreach (var draw in draws)
                {
                    board = new ChessBoard(pieces);
                    board.ApplyDraw(draw);
                    var pieceCmp = new ChessPiece() { Type = ChessPieceType.Bishop, Color = allyColor, WasMoved = true };
                    Assert.True(board.GetPieceAt(draw.OldPosition) == null && board.GetPieceAt(draw.NewPosition).Value == pieceCmp);
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
                var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;

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
                        new ChessPieceAtPos(knightPos,               new ChessPiece() { Type = ChessPieceType.Knight,  Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 3), new ChessPiece() { Type = ChessPieceType.Peasant, Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(0, 7), new ChessPiece() { Type = ChessPieceType.Rook,    Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = allyColor,  WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 7), new ChessPiece() { Type = ChessPieceType.Peasant, Color = enemyColor, WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(7, 2), new ChessPiece() { Type = ChessPieceType.Queen,   Color = enemyColor, WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(6, 5), new ChessPiece() { Type = ChessPieceType.King,    Color = enemyColor, WasMoved = true }),
                    };

                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawGenerator().GetDraws(board, knightPos, null, true);
                    Assert.True(draws.Count() == 5);

                    // check if the draws are correctly applied to the chess board
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board.ApplyDraw(draw);
                        var pieceCmp = new ChessPiece() { Type = ChessPieceType.Knight, Color = allyColor, WasMoved = true };
                        Assert.True(board.GetPieceAt(draw.OldPosition) == null && board.GetPieceAt(draw.NewPosition).Value == pieceCmp);
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
                for (int col = 0; col < ChessBoard.CHESS_BOARD_DIMENSION; col++)
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
                                    new ChessPieceAtPos(oldPos,                  new ChessPiece() { Type = ChessPieceType.Peasant, Color = allyColor,        WasMoved = wasMoved }),
                                    new ChessPieceAtPos(blockPos,                new ChessPiece() { Type = ChessPieceType.Peasant, Color = bpColor,          WasMoved = wasMoved }),
                                    new ChessPieceAtPos(new ChessPosition(0, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.White, WasMoved = false    }),
                                    new ChessPieceAtPos(new ChessPosition(7, 4), new ChessPiece() { Type = ChessPieceType.King,    Color = ChessColor.Black, WasMoved = false    }),
                                };

                                var board = new ChessBoard(pieces);

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
                                    // check if the chess piece is moved correctly
                                    board = new ChessBoard(pieces);
                                    board.ApplyDraw(dfwDraw);
                                    Assert.True(
                                        board.GetPieceAt(oldPos) == null
                                        && board.GetPieceAt(dfwNewPos).Value == new ChessPiece() { Type = ChessPieceType.Peasant, Color = allyColor, WasMoved = true }
                                    );
                                }

                                if (isDfwValid)
                                {
                                    // check if the chess piece is moved correctly
                                    board = new ChessBoard(pieces);
                                    board.ApplyDraw(dfwDraw);
                                    Assert.True(
                                        board.GetPieceAt(oldPos) == null
                                        && board.GetPieceAt(dfwNewPos).Value == new ChessPiece() { Type = ChessPieceType.Peasant, Color = allyColor, WasMoved = !wasMoved }
                                    );
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
                    for (int allyCol = 0; allyCol < ChessBoard.CHESS_BOARD_DIMENSION; allyCol++)
                    {
                        var oldPos = new ChessPosition(allyRow, allyCol);
                        int leftCatchCol  = allyCol - 1;
                        int rightCatchCol = allyCol + 1;

                        for (int targetColMiddle = 1; targetColMiddle < 7; targetColMiddle++)
                        {
                            var targetPosLeft  = new ChessPosition(nextRow, targetColMiddle - 1);
                            var targetPosRight = new ChessPosition(nextRow, targetColMiddle + 1);
                            
                            // investigate left catch if valid
                            if (ChessPosition.AreCoordsValid(leftCatchCol, nextRow))
                            {
                                var lcNewPos = new ChessPosition(nextRow, leftCatchCol);

                                // TODO: implement test
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
            // simulate for white and black side
            for (int colorValue = 0; colorValue < 2; colorValue++)
            {
                var allyColor = (ChessColor)colorValue;
                var enemyColor = (allyColor == ChessColor.White) ? ChessColor.Black : ChessColor.White;
                
                // go through all columns where the promoting peasant is moving foreward
                for (int fwCol = 0; fwCol < ChessBoard.CHESS_BOARD_DIMENSION; fwCol++)
                {
                    int fwRow = (allyColor == ChessColor.White) ? 6 : 1;
                    var fwPos = new ChessPosition(fwRow, fwCol);
                    
                    var pieces = new List<ChessPieceAtPos>()
                    {
                        new ChessPieceAtPos(new ChessPosition(4, 0), new ChessPiece() { Color = ChessColor.White, Type = ChessPieceType.King,    WasMoved = true }),
                        new ChessPieceAtPos(new ChessPosition(4, 7), new ChessPiece() { Color = ChessColor.Black, Type = ChessPieceType.King,    WasMoved = true }),
                        new ChessPieceAtPos(fwPos,                   new ChessPiece() { Color = allyColor,        Type = ChessPieceType.Peasant, WasMoved = true }),
                    };

                    int catchRow = (allyColor == ChessColor.White) ? 7 : 0;
                    int leftCatchCol  = fwCol - 1;
                    int rightCatchCol = fwCol + 1;

                    var enemyPeasant = new ChessPiece() { Color = enemyColor, Type = ChessPieceType.Peasant, WasMoved = true };
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

                    var board = new ChessBoard(pieces);
                    var draws = new ChessDrawGenerator().GetDraws(board, fwPos, null, true);
                    Assert.True(draws.Count() ==  (4 * ((fwCol % 7 == 0) ? 2 : 3)));
                        
                    foreach (var draw in draws)
                    {
                        board = new ChessBoard(pieces);
                        board.ApplyDraw(draw);
                        Assert.True(board.Pieces.All(x => x.Piece.Color == enemyColor || x.Piece.Type != ChessPieceType.Peasant));
                    }
                }
            }
        }

        #endregion PeasantDraws

        #endregion Tests
    }
}
