﻿using Chess.Lib;
using System;

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

            // draw white peasant E2-E4
            var draw = new ChessDraw() {
                Type = ChessDrawType.Standard,
                DrawingSide = ChessPieceColor.White,
                DrawingPieceType = ChessPieceType.Peasant,
                OldPosition = new ChessFieldPosition("E2"),
                NewPosition = new ChessFieldPosition("E4"),
                TakenEnemyPiece = null,
                Timestamp = DateTime.UtcNow
            };

            game.ApplyDraw(draw);

            // print board again and check if the draw was applied correctly
            Console.WriteLine($"chess game situation after drawing { draw.ToString() }:");
            Console.WriteLine();
            Console.WriteLine(game.Board);
            Console.WriteLine();

            // wait for exit
            Console.Write("Program finished. Exit with ENTER.");
            Console.ReadLine();
        }

        #endregion Main
    }
}
