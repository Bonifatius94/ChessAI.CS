using Chess.CLI.Player;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chess.CLI
{
    /// <summary>
    /// Represents a chess game session that hosts a chess game between two players. It can also handle the game execution.
    /// </summary>
    public class ChessGameSession
    {
        #region Constructor

        /// <summary>
        /// Initialize a new chess game session instance with the two players.
        /// </summary>
        /// <param name="whitePlayer">The first player drawing the white chess pieces.</param>
        /// <param name="blackPlayer">The second player drawing the black chess pieces.</param>
        public ChessGameSession(IChessPlayer whitePlayer, IChessPlayer blackPlayer)
        {
            _whitePlayer = whitePlayer;
            _blackPlayer = blackPlayer;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The white chess player of this session.
        /// </summary>
        private IChessPlayer _whitePlayer;

        /// <summary>
        /// The black chess player of this session.
        /// </summary>
        private IChessPlayer _blackPlayer;

        #endregion Members

        #region Methods

        /// <summary>
        /// Start a new game with the two players of the session and continue until the game is over.
        /// </summary>
        /// <returns>return the chess game log with all draws etc.</returns>
        public ChessGame ExecuteGame()
        {
            var game = new ChessGame();

            // print the chess board in start formation
            Console.WriteLine(game.Board.ToString());
            Console.WriteLine();

            while (!game.GameStatus.IsGameOver())
            {
                // determin the drawing player
                var drawingPlayer = game.SideToDraw == ChessColor.White ? _whitePlayer : _blackPlayer;

                // init loop variables
                bool isDrawValid;
                ChessDraw draw;
                Stopwatch drawWatch = new Stopwatch();

                do
                {
                    // get the draw from the player
                    drawWatch.Start();
                    draw = drawingPlayer.GetNextDraw(game.Board, game.LastDrawOrDefault);
                    drawWatch.Stop();

                    // apply the draw to the game (and validate it)
                    isDrawValid = game.ApplyDraw(draw, true);
                    if (!isDrawValid) { Console.Write("The draw you have put is invalid! "); }
                }
                while (!isDrawValid);

                if (isDrawValid)
                {
                    // print new chess board
                    var timespan = new TimeSpan(drawWatch.ElapsedTicks);
                    Console.WriteLine($"{ game.SideToDraw.Opponent() } player drew { draw }, took { timespan.Minutes }m { timespan.Seconds }s");
                    Console.WriteLine();
                    Console.WriteLine(game.Board.ToString());
                    Console.WriteLine();
                }
            }

            return game;
        }

        #endregion Methods
    }
}
