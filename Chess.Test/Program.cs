using Chess.Lib;
using System;
using System.Collections.Generic;

namespace Chess.Test
{
    public class Program
    {
        #region Main

        public static void Main(string[] args)
        {
            // create a new chess game instance and print it to the console
            var game = new ChessGame();
            Console.WriteLine("initial chess game situation:");
            Console.WriteLine();
            Console.WriteLine(game.Board);
            Console.WriteLine();

            // define some draws to test if the chess pieces behave correctly
            var draws = new List<ChessDraw>() {
                new ChessDraw(ChessPieceColor.White, ChessPieceType.Peasant, new ChessFieldPosition("E2"), new ChessFieldPosition("E4")), // test peasant two foreward
                new ChessDraw(ChessPieceColor.Black, ChessPieceType.Peasant, new ChessFieldPosition("E7"), new ChessFieldPosition("E6")), // test peasant one foreward
                new ChessDraw(ChessPieceColor.White, ChessPieceType.Queen,   new ChessFieldPosition("D1"), new ChessFieldPosition("F3")), // test queen / bishop
                new ChessDraw(ChessPieceColor.Black, ChessPieceType.Knight,  new ChessFieldPosition("B8"), new ChessFieldPosition("C6")), // test knight
                new ChessDraw(ChessPieceColor.White, ChessPieceType.Queen,   new ChessFieldPosition("F3"), new ChessFieldPosition("F5")), // test queen / rock
            };

            foreach (var draw in draws)
            {
                game.ApplyDraw(draw);

                // print board again and check if the draw was applied correctly
                Console.WriteLine($"chess game situation after drawing { draw.ToString() }:");
                Console.WriteLine();
                Console.WriteLine(game.Board);
                Console.WriteLine();
            }

            // wait for exit
            Console.Write("Program finished. Exit with ENTER.");
            Console.ReadLine();
        }

        #endregion Main
    }
}
