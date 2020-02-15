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
        /// <param name="side"></param>
        public HumanChessPlayer(ChessColor side) { _side = side; }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The drawing side.
        /// </summary>
        private ChessColor _side;

        #endregion Members

        #region Methods

        /// <summary>
        /// Make a human user put the next draw via CLI.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? previousDraw)
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
                        if (board.IsCapturedAt(oldPosition) && board.GetPieceAt(oldPosition).Color == _side) { break; }
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
                && (_side == ChessColor.White && newPosition.Row == 7) || (_side == ChessColor.Black && newPosition.Row == 0))
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
                            case 'r': promotionPieceType = ChessPieceType.Rook;   break;
                            case 'q': promotionPieceType = ChessPieceType.Queen;  break;
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
