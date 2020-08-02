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

using Chess.CLI.Player;
using Chess.GameLib;
using Chess.GameLib.Player;
using Chess.GameLib.Session;
using Chess.Lib;
using System;
using System.Diagnostics;
using System.IO;

namespace Chess.CLI
{
    /// <summary>
    /// Executes a chess game via command line interface.
    /// </summary>
    public class Program
    {
        #region Main

        /// <summary>
        /// Main routine that executes a chess game.
        /// </summary>
        /// <param name="args">The CLI arguments passed by the user.</param>
        public static void Main(string[] args)
        {
            // parse args
            var startupArgs = new StartupArgs().Init(args);

            // make sure the program is not running in help mode
            if (!startupArgs.IsHelp)
            {
                // init game session
                var session = initChessGame(startupArgs);

                // bind to BoardChanged event to print the draws that the players made
                var drawWatch = new Stopwatch();
                drawWatch.Start();
                session.BoardChanged += (IChessBoard newBoard) => {

                    // print new chess board
                    var timespan = new TimeSpan(drawWatch.ElapsedTicks);
                    Console.WriteLine($"{ session.Game.SideToDraw.Opponent() } player drew { session.Game.LastDraw }, took { timespan.Minutes }m { timespan.Seconds }s");
                    Console.WriteLine();
                    Console.WriteLine(session.Game.Board.ToString());
                    Console.WriteLine();
                    drawWatch.Restart();
                };

                // execute game
                var watch = new Stopwatch();
                watch.Start();
                var game = session.ExecuteGame();
                watch.Stop();
                var timespan = new TimeSpan(watch.ElapsedTicks);

                // write game result
                Console.WriteLine($"Game is over, took { timespan.Minutes }m { timespan.Seconds }s, { game.LastDraw.DrawingSide } player wins!");

                // write gamelog
                using (var logfile = new StreamWriter("gamelog.txt"))
                {
                    logfile.WriteLine("Chess Game Log");
                    logfile.WriteLine("=================");
                    game.AllDraws.ForEach(x => logfile.WriteLine(x.ToString()));
                    logfile.WriteLine($"final game status: { game.GameStatus.ToString() }!");
                }
            }
        }

        /// <summary>
        /// Initialize a new game session with the given startup arguments.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        /// <returns>a new game session</returns>
        private static ChessGameSession initChessGame(StartupArgs args)
        {
            IChessPlayer whitePlayer;
            IChessPlayer blackPlayer;

            // init players
            if (args.GameMode == ChessGameMode.PvC)
            {
                var humanPlayerSide = new Random().Next(0, 2) == 0 ? ChessColor.White : ChessColor.Black;
                
                whitePlayer = humanPlayerSide == ChessColor.White 
                    ? (IChessPlayer)new HumanChessPlayer(ChessColor.White) 
                    : new ArtificialChessPlayer(ChessColor.White, (ChessDifficultyLevel)args.ComputerLevel);
                
                blackPlayer = humanPlayerSide != ChessColor.White 
                    ? (IChessPlayer)new HumanChessPlayer(ChessColor.Black) 
                    : new ArtificialChessPlayer(ChessColor.Black, (ChessDifficultyLevel)args.ComputerLevel);
            }
            else if (args.GameMode == ChessGameMode.CvC)
            {
                whitePlayer = new ArtificialChessPlayer(ChessColor.White, (ChessDifficultyLevel)args.ComputerLevel);
                blackPlayer = new ArtificialChessPlayer(ChessColor.Black, (ChessDifficultyLevel)args.ComputerLevel);
            }
            else
            {
                whitePlayer = new HumanChessPlayer(ChessColor.White);
                blackPlayer = new HumanChessPlayer(ChessColor.Black);
            }

            return new ChessGameSession(whitePlayer, blackPlayer);
        }

        #endregion Main
    }
}
