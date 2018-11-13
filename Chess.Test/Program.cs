using Chess.Lib;
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
            Console.WriteLine(game.Board);
            Console.ReadLine();
        }

        #endregion Main
    }
}
