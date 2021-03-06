﻿/*
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

using Chess.GameLib.Player;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chess.GameLib.Session
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
        /// The chess game that this session is currently running.
        /// </summary>
        public ChessGame Game { get; private set; }

        /// <summary>
        /// The chess board representing the current chess position of the game.
        /// </summary>
        public IChessBoard Board { get { return Game.Board; } }

        #endregion Members

        #region Events

        /// <summary>
        /// A delegate for implementations of board changed handlers.
        /// </summary>
        /// <param name="newBoard">The new chess board.</param>
        public delegate void BoardChangedHandler(IChessBoard newBoard);

        /// <summary>
        /// An event that is signaled when the chess board changed (due to a player making a draw).
        /// </summary>
        public event BoardChangedHandler BoardChanged;

        #endregion Events

        #region Methods

        /// <summary>
        /// Start a new game with the two players of the session and continue until the game is over.
        /// </summary>
        /// <returns>return the chess game log with all draws etc.</returns>
        public ChessGame ExecuteGame()
        {
            // initialize new chess game
            Game = new ChessGame();

            // continue until the game is over
            while (!Game.GameStatus.IsGameOver())
            {
                // determin the drawing player
                var drawingPlayer = Game.SideToDraw == ChessColor.White ? WhitePlayer : BlackPlayer;

                // init loop variables
                bool isDrawValid;
                ChessDraw draw;

                do
                {
                    // get the draw from the player
                    draw = drawingPlayer.GetNextDraw(Game.Board, Game.LastDrawOrDefault);
                    isDrawValid = Game.ApplyDraw(draw, true);
                }
                while (!isDrawValid);

                // raise board changed event
                BoardChanged?.Invoke(Game.Board);
            }

            // return the chess game, so it can be logged, etc.
            return Game;
        }

        #endregion Methods
    }
}
