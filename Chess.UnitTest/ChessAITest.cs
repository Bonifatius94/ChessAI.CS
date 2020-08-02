/*
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

//using Chess.AI;
//using Chess.Lib;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Xunit;
//using Xunit.Abstractions;

//namespace Chess.UnitTest
//{
//    public class ChessAITest : TestBase
//    {
//        #region Constructor

//        public ChessAITest(ITestOutputHelper output) : base(output) { }

//        #endregion Constructor

//        #region Tests

//        [Fact]
//        public void DrawAITest()
//        {
//            // TODO: test higher difficulties if 

//            for (int difficultyValue = 0; difficultyValue < 6; difficultyValue++)
//            {
//                var difficulty = (ChessDifficultyLevel)difficultyValue;

//                var board = ChessBoard.StartFormation;
//                var draw = new MinimaxChessDrawAI().GetNextDraw(board, null, difficulty);

//                output.WriteLine(draw.ToString());
//            }
//        }

//        //[Fact]
//        //public void CheckmateInTwoTest()
//        //{
//        //    // init chess pieces
//        //    var pieces = new List<ChessPieceAtPos>()
//        //    {
//        //        // white pieces
//        //        new ChessPieceAtPos(new ChessPosition("A1"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, true)),
//        //        new ChessPieceAtPos(new ChessPosition("E1"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, true)),
//        //        new ChessPieceAtPos(new ChessPosition("B3"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
//        //        new ChessPieceAtPos(new ChessPosition("D3"), new ChessPiece(ChessPieceType.King,    ChessColor.White, true)),
//        //        new ChessPieceAtPos(new ChessPosition("A5"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, true)),
//        //        new ChessPieceAtPos(new ChessPosition("E5"), new ChessPiece(ChessPieceType.Knight,  ChessColor.White, true)),

//        //        // black pieces
//        //        new ChessPieceAtPos(new ChessPosition("F2"), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("B4"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("F4"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("C5"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("F5"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("C6"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("G6"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("C7"), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true)),
//        //        new ChessPieceAtPos(new ChessPosition("F7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true)),
//        //    };

//        //    // prepare board
//        //    var board = new ChessBoard(pieces);
//        //    const int drawsUntilCheckmate = 2;

//        //    // determine the draws until checkmate
//        //    int drawsCount = drawsToCheckmate(board, ChessColor.Black, ChessDifficultyLevel.Medium, drawsUntilCheckmate);
//        //    Assert.True(drawsCount == drawsUntilCheckmate);
//        //}

//        [Fact]
//        public void CheckmateInFourTest()
//        {
//            // init chess pieces
//            var pieces = new List<ChessPieceAtPos>()
//            {
//                // white pieces
//                new ChessPieceAtPos(new ChessPosition("C1"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.White, true )),
//                new ChessPieceAtPos(new ChessPosition("G1"), new ChessPiece(ChessPieceType.King,    ChessColor.White, true )),
//                new ChessPieceAtPos(new ChessPosition("B2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
//                new ChessPieceAtPos(new ChessPosition("C2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
//                new ChessPieceAtPos(new ChessPosition("F2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
//                new ChessPieceAtPos(new ChessPosition("G2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
//                new ChessPieceAtPos(new ChessPosition("H2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
//                new ChessPieceAtPos(new ChessPosition("D4"), new ChessPiece(ChessPieceType.Knight,  ChessColor.White, true )),
//                new ChessPieceAtPos(new ChessPosition("F5"), new ChessPiece(ChessPieceType.Queen,   ChessColor.White, true )),
//                new ChessPieceAtPos(new ChessPosition("H8"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, true )),

//                // black pieces
//                new ChessPieceAtPos(new ChessPosition("B5"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, true )),
//                new ChessPieceAtPos(new ChessPosition("D5"), new ChessPiece(ChessPieceType.Knight,  ChessColor.Black, true )),
//                new ChessPieceAtPos(new ChessPosition("D6"), new ChessPiece(ChessPieceType.King,    ChessColor.Black, true )),
//                new ChessPieceAtPos(new ChessPosition("A7"), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, true )),
//                new ChessPieceAtPos(new ChessPosition("C7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
//                new ChessPieceAtPos(new ChessPosition("E7"), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, true )),
//                new ChessPieceAtPos(new ChessPosition("G7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
//                new ChessPieceAtPos(new ChessPosition("H7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
//            };

//            // prepare board
//            var board = new ChessBoard(pieces);
//            const int drawsUntilCheckmate = 4;

//            // determine the draws until checkmate
//            int drawsCount = drawsToCheckmate(board, ChessColor.White, ChessDifficultyLevel.Hard, drawsUntilCheckmate);
//            Assert.True(drawsCount == drawsUntilCheckmate);
//        }

//        [Fact]
//        public void CheckmateInSixTest()
//        {

//        }

//        [Fact]
//        public void CheckmateInEightTest()
//        {

//        }

//        private int drawsToCheckmate(ChessBoard board, ChessColor sideToDraw, ChessDifficultyLevel level, int maxDraws)
//        {
//            // init new game
//            int count = 0;
//            var game = new ChessGame() { Board = board };
//            var gameStatus = CheckGameStatus.None;

//            do
//            {
//                // select the best draw considering the next couple of draws
//                var draw = new MinimaxChessDrawAI().GetNextDraw(game.Board, game.LastDrawOrDefault, level);

//                // apply the draw to the chess board and check if the game is over
//                game.ApplyDraw(draw);
//                gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);
//                Console.WriteLine(draw.ToString());

//                if (++count > maxDraws) { break; }
//            }
//            while ((!gameStatus.IsGameOver() || gameStatus == CheckGameStatus.UnsufficientPieces) && !game.ContainsLoop());

//            return count;
//        }

//        #endregion Tests
//    }
//}
