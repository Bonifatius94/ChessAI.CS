using Chess.CLI.Player;
using Chess.Lib;
using System;
using System.Diagnostics;

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

                // execute game
                var watch = new Stopwatch();
                watch.Start();
                var game = session.ExecuteGame();
                watch.Stop();
                var timespan = new TimeSpan(watch.ElapsedTicks);

                // write game result
                Console.WriteLine($"Game is over, took { timespan.Minutes }m { timespan.Seconds }s, { game.LastDraw.DrawingSide } player wins!");
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
                whitePlayer = humanPlayerSide == ChessColor.White ? (IChessPlayer)new HumanChessPlayer(ChessColor.White) : new ArtificialChessPlayer(args.ComputerLevel);
                blackPlayer = humanPlayerSide != ChessColor.White ? (IChessPlayer)new HumanChessPlayer(ChessColor.Black) : new ArtificialChessPlayer(args.ComputerLevel);
            }
            else if (args.GameMode == ChessGameMode.PvC)
            {
                whitePlayer = new ArtificialChessPlayer(args.ComputerLevel);
                blackPlayer = new ArtificialChessPlayer(args.ComputerLevel);
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
