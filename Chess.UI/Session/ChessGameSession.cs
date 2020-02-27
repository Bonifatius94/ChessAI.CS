using Chess.UI.Player;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chess.UI.Session
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
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The white chess player of this session.
        /// </summary>
        public IChessPlayer WhitePlayer { get; private set; }

        /// <summary>
        /// The black chess player of this session.
        /// </summary>
        public IChessPlayer BlackPlayer { get; private set; }

        /// <summary>
        /// The chess board representing the current chess position of the game.
        /// </summary>
        public ChessBoard Board { get; private set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Start a new game with the two players of the session and continue until the game is over.
        /// </summary>
        /// <returns>return the chess game log with all draws etc.</returns>
        public ChessGame ExecuteGame()
        {
            // initialize new chess game
            var game = new ChessGame();

            // continue until the game is over
            while (!game.GameStatus.IsGameOver())
            {
                // determin the drawing player
                var drawingPlayer = game.SideToDraw == ChessColor.White ? WhitePlayer : BlackPlayer;

                // init loop variables
                bool isDrawValid;
                ChessDraw draw;

                do
                {
                    // get the draw from the player
                    draw = drawingPlayer.GetNextDraw(game.Board, game.LastDrawOrDefault);
                    isDrawValid = game.ApplyDraw(draw, true);
                }
                while (!isDrawValid);

                // update the board property
                Board = game.Board;
            }

            // return the chess game, so it can be logged, etc.
            return game;
        }

        #endregion Methods
    }
}
