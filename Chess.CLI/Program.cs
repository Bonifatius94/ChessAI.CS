﻿using Chess.AI;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chess.CLI
{
    class Program
    {
        #region Main

        public static void Main(string[] args)
        {
            var gameWatch = new Stopwatch();
            gameWatch.Start();

            // init new game
            var game = new ChessGame();
            var gameStatus = CheckGameStatus.None;
            
            try
            {
                do
                {
                    var drawWatch = new Stopwatch();
                    
                    // select the best draw considering the next couple of draws
                    drawWatch.Start();
                    var draw = MinimaxChessDrawAI.Instance.GetNextDraw(game.Board, game.LastDrawOrDefault, ChessDifficultyLevel.Medium);
                    drawWatch.Stop();

                    // apply the draw to the chess board and  check if the game is over
                    game.ApplyDraw(draw);
                    gameStatus = ChessDrawSimulator.Instance.GetCheckGameStatus(game.Board, draw);

                    // print draw and board after draw was applied
                    Console.WriteLine();
                    Console.WriteLine($"{ draw }{ (gameStatus == CheckGameStatus.None ? string.Empty : " " + gameStatus.ToString().ToLower()) }");
                    Console.WriteLine($"took { Math.Round(new TimeSpan(drawWatch.ElapsedTicks).TotalSeconds, 2) } seconds");
                    Console.WriteLine(game.Board.ToString());
                }
                while (!gameStatus.IsGameOver() && !game.ContainsLoop());
            }
            finally
            {
                if (gameStatus.IsGameOver())
                {
                    Console.WriteLine($"game over. { (gameStatus == CheckGameStatus.Stalemate ? "tied" : $"{ game.SideToDraw.Opponent().ToString().ToLower() } wins") }.");
                    Console.WriteLine("======================");
                }
                else if (game.ContainsLoop())
                {
                    Console.WriteLine($"loop encountered. game is undecided.");
                    Console.WriteLine("======================");
                }
            }

            gameWatch.Stop();
            var gameTime = new TimeSpan(gameWatch.ElapsedTicks);
            Console.WriteLine($"game took { Math.Round(gameTime.TotalMinutes, 2) } minutes");

            // wait for exit
            Console.WriteLine();
            Console.Write("Exit with ENTER");
            Console.ReadLine();
        }
        
        #endregion Main
    }
}
