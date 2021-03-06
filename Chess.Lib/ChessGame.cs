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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess game including a chess board and a chess draws history.
    /// </summary>
    public class ChessGame : ICloneable
    {
        #region Constructor

        /// <summary>
        /// Create a chess game instance that initializes the chess board with the start formation. 
        /// Furthermore start the game clock and make the white side draw first.
        /// </summary>
        public ChessGame()
        {
            Board = ChessBitboard.StartFormation;
            StartOfGame = DateTime.UtcNow;
            SideToDraw = ChessColor.White;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The chess board with the current game situation.
        /// </summary>
        public IChessBoard Board { get; set; }

        /// <summary>
        /// The time when the game was started. Time is measured for UTC time zone.
        /// </summary>
        public DateTime StartOfGame { get; set; }

        /// <summary>
        /// The side that has to draw.
        /// </summary>
        public ChessColor SideToDraw { get; set; }

        /// <summary>
        /// A stack with all chess draws that have been applied to this chess game instance.
        /// </summary>
        private readonly Stack<ChessDraw> _drawHistory = new Stack<ChessDraw>();

        /// <summary>
        /// The last chess draw that was made.
        /// </summary>
        public ChessDraw LastDraw { get { return _drawHistory.Peek(); } }

        /// <summary>
        /// The last chess draw that was made. (null if no draw has been applied yet)
        /// </summary>
        public ChessDraw? LastDrawOrDefault { get { return (_drawHistory.Count > 0) ? (ChessDraw?)_drawHistory.Peek() : null; } }

        /// <summary>
        /// A list of all chess draws starting with the first and ending with the last draw that has been made. (computed)
        /// </summary>
        public List<ChessDraw> AllDraws { get { return _drawHistory.Reverse().ToList(); } }

        /// <summary>
        /// The current status of the chess game.
        /// </summary>
        public ChessGameStatus GameStatus { get { return _drawHistory.Count > 0 ? ChessDrawSimulator.Instance.GetCheckGameStatus(Board, LastDraw) : ChessGameStatus.None; } }

        /// <summary>
        /// The winner of the chess game (if there is one).
        /// </summary>
        public ChessColor? Winner { get; set; }
        // TODO: maybe rework this attibute as this is quite "quick n dirty"

        #endregion Members

        #region Methods

        /// <summary>
        /// Apply the chess draw to the current game situation on the chess board.
        /// Furthermore change the side that has to draw and store the chess draw in the chess draws history (stack).
        /// </summary>
        /// <param name="draw">The chess draw to be made</param>
        /// <param name="validate">Indicates whether the chess draw should be validated</param>
        /// <returns>boolean whether the draw could be applied</returns>
        public bool ApplyDraw(ChessDraw draw, bool validate = false)
        {
            var lastDraw = (_drawHistory?.Count > 0) ? (ChessDraw?)_drawHistory.Peek() : null;
            bool isDrawValid = !validate || draw.IsValid(Board, lastDraw);

            // info: Validate() throws an exception if the draw is invalid -> catch this exception and make use of the exception message
            if (isDrawValid)
            {
                // draw the chess piece
                Board = Board.ApplyDraw(draw);

                // apply the chess draw to the chess draws history
                _drawHistory.Push(draw);

                // change the side that has to draw
                SideToDraw = SideToDraw.Opponent();
            }

            return isDrawValid;
        }

        /// <summary>
        /// Revert the last chess draw by restoring the game situation before the draw was made from the chess draws history.
        /// </summary>
        public void RevertLastDraw()
        {
            // make sure the chess draws stack is not empty (otherwise throw exception)
            if (_drawHistory.Count == 0) { throw new InvalidOperationException("There are no draws to be reverted. Stack is empty."); }

            // remove the last chess draw from chess draws history
            _drawHistory.Pop();

            // create a new chess board and apply all previous chess draws (-> this results in the situation before the last chess draw was applied)
            Board = ChessBoard.StartFormation.ApplyDraws(_drawHistory.Reverse().ToList());
            
            // change the side that has to draw
            SideToDraw = SideToDraw.Opponent();
        }

        /// <summary>
        /// Retrieve all chess draws that the drawing side can make.
        /// </summary>
        /// <param name="analyzeDrawIntoCheck">indicates whether draw into check is analyzed (default: true)</param>
        /// <returns>a list of all possible chess draws for the drawing side</returns>
        public IEnumerable<ChessDraw> GetDraws(bool analyzeDrawIntoCheck = true)
        {
            var alliedPieces = Board.GetPiecesOfColor(SideToDraw);
            var draws = alliedPieces.SelectMany(piece => ChessDrawGenerator.Instance.GetDraws(Board, piece.Position, LastDrawOrDefault, analyzeDrawIntoCheck));

            return draws;
        }

        /// <summary>
        /// Determines whether there are loops in the chess draws.
        /// </summary>
        /// <returns></returns>
        public bool ContainsLoop()
        {
            // TODO: move this function to an extension class as it's not required as a base functionality

            int loopSize = 4;
            var draws = _drawHistory.ToArray();

            // search for all loops sizes ()
            while (loopSize < draws.Length / 2)
            {
                // determine the loop subsequence and the draws to compare
                var restDraws = draws.Reverse().Take(draws.Length - loopSize).ToArray();
                var loopDraws = draws.Take(loopSize).Reverse().ToArray();

                for (int diff = 0; diff <= restDraws.Length - loopDraws.Length; diff++)
                {
                    int i;
                    
                    for (i = 0; i < loopSize; i++)
                    {
                        if (restDraws[diff + i] != loopDraws[i]) { break; }
                    }

                    bool matchFound = (i == loopSize);
                    if (matchFound) { return true; }
                }

                loopSize++;
            }

            return false;
        }

        /// <summary>
        /// Create a deep copy of this instance.
        /// </summary>
        /// <returns>a deep copy of this instance</returns>
        public object Clone()
        {
            var game = new ChessGame();
            AllDraws.ForEach(draw => game.ApplyDraw(draw));
            return game;
        }

        #endregion Methods
    }
}
