/*
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
using System.Text;

namespace Chess.CLI.Player
{
    /// <summary>
    /// Representing a human chess player who puts his chess draws via CLI.
    /// </summary>
    public class HumanChessPlayer : IChessPlayer
    {
        #region Constructor

        /// <summary>
        /// Initialize a new human chess player instance drawing the chess pieces of the given color.
        /// </summary>
        /// <param name="side">The side that the player draws.</param>
        public HumanChessPlayer(ChessColor side) { Side = side; }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The drawing side.
        /// </summary>
        public ChessColor Side { get; private set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Make a human user put the next draw via CLI.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(IChessBoard board, ChessDraw? previousDraw)
        {
            ChessPosition oldPosition;
            ChessPosition newPosition;
            ChessPieceType? promotionPieceType = null;

            do
            {
                // get draw from user input
                Console.Write("Please make your next draw (e.g. 'e2-e4): ");
                string userInput = Console.ReadLine().Trim().ToLower();

                // parse user input
                if (userInput.Length == 5)
                {
                    // parse coord strings
                    string oldPosString = userInput.Substring(0, 2);
                    string newPosString = userInput.Substring(3, 2);

                    // validate coord strings
                    if (ChessPosition.AreCoordsValid(oldPosString) && ChessPosition.AreCoordsValid(newPosString))
                    {
                        // init chess positions
                        oldPosition = new ChessPosition(oldPosString);
                        newPosition = new ChessPosition(newPosString);

                        // make sure that the player possesses the the chess piece he is moving (simple validation)
                        if (board.IsCapturedAt(oldPosition) && board.GetPieceAt(oldPosition).Color == Side) { break; }
                        else { Console.Write("There is no chess piece to be moved onto the field you put! "); }
                    }
                }
                else
                {
                    Console.Write("Your input needs to be of the syntax in the example! ");
                }
            }
            while (true);

            // handle peasant promotion
            if (board.IsCapturedAt(oldPosition) && board.GetPieceAt(oldPosition).Type == ChessPieceType.Peasant
                && (Side == ChessColor.White && newPosition.Row == 7) || (Side == ChessColor.Black && newPosition.Row == 0))
            {
                do
                {
                    // get draw from user input
                    Console.Write("You have put a promotion draw. Please choose the type you want to promote to (options: Bishop=B, Knight=N, Rook=R, Queen=Q): ");
                    string userInput = Console.ReadLine().Trim().ToLower().ToLower();

                    if (userInput.Length == 1)
                    {
                        switch (userInput[0])
                        {
                            case 'b': promotionPieceType = ChessPieceType.Bishop; break;
                            case 'n': promotionPieceType = ChessPieceType.Knight; break;
                            case 'r': promotionPieceType = ChessPieceType.Rook; break;
                            case 'q': promotionPieceType = ChessPieceType.Queen; break;
                        }

                        if (promotionPieceType == null) { Console.Write("Your input needs to be a letter like in the example! "); }
                    }
                }
                while (promotionPieceType == null);
            }

            return new ChessDraw(board, oldPosition, newPosition, promotionPieceType);
        }

        #endregion Methods
    }
}
