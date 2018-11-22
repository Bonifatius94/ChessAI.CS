using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chess.CLI
{
    public class Program
    {
        #region Main

        // TODO: transform this program into an interactive chess console game

        public static void Main(string[] args)
        {
            int testsCount = 10000;
            var spans = new TimeSpan[testsCount];
            var stopwatch = new Stopwatch();

            for (int i = 0; i < testsCount; i++)
            {
                stopwatch.Restart();
                test();
                stopwatch.Stop();

                spans[i] = new TimeSpan(stopwatch.ElapsedTicks);
                Console.WriteLine($"time for applying 6 draws: { spans[i].TotalMilliseconds } ms");
            }

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"average time per draw: { spans.Select(x => x.TotalMilliseconds).Average() / 6 } ms");

            // wait for exit
            Console.Write("Program finished. Exit with ENTER.");
            Console.ReadLine();
        }

        private static void test()
        {
            // create a new chess game instance and print it to the console
            var game = new ChessGame();
            //Console.WriteLine("initial chess game situation:");
            //Console.WriteLine();
            //Console.WriteLine(game.Board);
            //Console.WriteLine();

            // define some draws to test if the chess pieces behave correctly
            var moves = new List<Tuple<ChessPosition, ChessPosition>>() {
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E2"), new ChessPosition("E4")), // test peasant two foreward
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E7"), new ChessPosition("E6")), // test peasant one foreward
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("D1"), new ChessPosition("F3")), // test queen / bishop
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("B8"), new ChessPosition("C6")), // test knight
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("F3"), new ChessPosition("F5")), // test queen / rock
                new Tuple<ChessPosition, ChessPosition>(new ChessPosition("E8"), new ChessPosition("E7")), // test king
            };
            
            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                var draw = new ChessDraw(game.Board, move.Item1, move.Item2);
                game.ApplyDraw(draw, true);

                //// print board again and check if the draw was applied correctly
                //Console.WriteLine($"chess game situation after drawing { draw.ToString() }:");
                //Console.WriteLine();
                //Console.WriteLine(game.Board);
                //Console.WriteLine();
            }
        }

        #endregion Main
    }
}
