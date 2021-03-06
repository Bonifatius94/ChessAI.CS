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
using Chess.GameLib.Session;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Chess.UI.Extensions
{
    public class UIChessPlayer : IChessPlayer
    {
        #region Constructor

        /// <summary>
        /// Create a new instance of a UI chess player. This constructor may be called by the UI thread.
        /// </summary>
        /// <param name="side">The side the player is drawing.</param>
        public UIChessPlayer(ChessColor side)
        {
            Side = side;
        }

        #endregion Constructor

        #region Members

        #region ConcurrencyControl

        // mutex handles for concurrency control
        private Mutex _mutex = new Mutex(true);

        private bool _canGetNextDraw = true;

        /// <summary>
        /// A cache for the chess draw to be applied to the session by this player.
        /// </summary>
        private ChessDraw _temp;

        #endregion ConcurrencyControl

        public ChessColor Side { get; private set; }

        public delegate void OpponentDrawMadeHandler(IChessBoard board, ChessDraw? opponentDraw);
        public event OpponentDrawMadeHandler OpponentDrawMade;

        #endregion Members

        #region Methods

        public void PassDraw(ChessDraw draw)
        {
            // cache the draw to be passed to the session
            _temp = draw;

            // release the mutex that makes GetNextDraw wait -> signal that the draw was made
            _canGetNextDraw = false;
            _mutex.ReleaseMutex();

            // make sure that GetNextDraw captures the mutex first
            while (_canGetNextDraw) { Thread.Sleep(10); }

            // recapture mutex, so the next invokation of GetNextDraw can wait again
            _mutex.WaitOne();
            _canGetNextDraw = true;
        }

        /// <summary>
        /// Gets the next draw from a player via a graphical user interface. This method should only be called by a background thread running a ChessGameSession instance.
        /// </summary>
        /// <param name="board">The board representing the current chess situation.</param>
        /// <param name="previousDraw">The previous draw made by the opponent (may be null if the drawing player is making the first draw with white pieces)</param>
        /// <returns>The chess draw made by the chess player</returns>
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? previousDraw)
        {
            // wait until mutex is ready
            while (!_canGetNextDraw) { Thread.Sleep(10); }

            // signal opponent's draw to all subscribers via event handler
            OpponentDrawMade?.Invoke(board, previousDraw);

            // wait until the user made his draw
            _mutex.WaitOne();
            _canGetNextDraw = false;
            _mutex.ReleaseMutex();

            return _temp;
        }

        #endregion Methods
    }
}
