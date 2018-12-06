using Chess.AI;
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

        static void Main(string[] args)
        {
            // init new game
            var game = new ChessGame();
            var gameStatus = CheckGameStatus.None;
            var draws = new List<Tuple<ChessDraw, CheckGameStatus>>();

            try
            {
                do
                {
                    // select the best draw considering the next couple of draws
                    var draw = new ChessDrawAI().GetNextDraw(game.Board, game.LastDrawOrDefault, ChessDifficultyLevel.Medium);

                    // apply the draw to the chess board and  check if the game is over
                    game.ApplyDraw(draw);
                    gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);
                    draws.Add(new Tuple<ChessDraw, CheckGameStatus>(game.LastDraw, gameStatus));

                    // print draw and board after draw was applied
                    Console.WriteLine();
                    Console.WriteLine($"{ draw }{ (gameStatus == CheckGameStatus.None ? string.Empty : " " + gameStatus.ToString().ToLower()) }");
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

            // wait for exit
            Console.WriteLine();
            Console.Write("Exit with ENTER");
            Console.ReadLine();
        }
        
        #endregion Main
    }
}
