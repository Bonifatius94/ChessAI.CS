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
//using System.Linq;
//using System.Text;
//using Xunit;
//using Xunit.Abstractions;

//namespace Chess.UnitTest
//{
//    public class PerformanceTest : TestBase
//    {
//        #region Constructor

//        public PerformanceTest(ITestOutputHelper output) : base(output) { }

//        #endregion Constructor

//        #region Tests

//        [Fact/*(Skip = "test would take too long")*/]
//        public void PlaymakingTest()
//        {
//            bool failed = false;

//            for (int i = 0; i < 100; i++)
//            {
//                try
//                {
//                    playGame(i + 1);
//                }
//                catch (Exception ex)
//                {
//                    failed = true;
//                    output.WriteLine(ex.ToString());
//                    break;
//                }
//            }

//            Assert.True(!failed);
//        }

//        private void playGame(int id)
//        {
//            // init new game
//            var game = new ChessGame();
//            var gameStatus = CheckGameStatus.None;
//            var draws = new List<Tuple<ChessDraw, CheckGameStatus>>();

//            try
//            {
//                do
//                {
//                    // select the best draw considering the next couple of draws
//                    var draw = new MinimaxChessDrawAI().GetNextDraw(game.Board, game.LastDrawOrDefault, ChessDifficultyLevel.Medium);

//                    // apply the draw to the chess board and check if the game is over
//                    game.ApplyDraw(draw);
//                    gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);

//                    draws.Add(new Tuple<ChessDraw, CheckGameStatus>(game.LastDraw, gameStatus));
//                }
//                while (!gameStatus.IsGameOver() && !game.ContainsLoop());
//            }
//            finally
//            {
//                // print draws and result of game
//                output.WriteLine($"starting game { id }:");
//                output.WriteLine("======================");
//                draws.ForEach(x => output.WriteLine($"{ x.Item1 }{ (x.Item2 == CheckGameStatus.None ? string.Empty : " " + x.Item2.ToString().ToLower()) }"));
//                output.WriteLine("======================");
//                output.WriteLine(game.Board.ToString());
//                output.WriteLine("======================");

//                if (gameStatus.IsGameOver())
//                {
//                    output.WriteLine($"game over. { (gameStatus == CheckGameStatus.Stalemate ? "tied" : $"{ game.SideToDraw.Opponent().ToString().ToLower() } wins") }.");
//                    output.WriteLine("======================");
//                }
//                else if (game.ContainsLoop())
//                {
//                    output.WriteLine($"loop encountered. game is undecided.");
//                    output.WriteLine("======================");
//                }
//            }
//        }

//        #endregion Tests
//    }
//}
