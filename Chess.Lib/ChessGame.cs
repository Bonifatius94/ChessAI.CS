using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess game including a chess board and a chess draws history.
    /// </summary>
    public class ChessGame
    {
        #region Constructor

        /// <summary>
        /// Create a chess game instance that initializes the chess board with the start formation. 
        /// Furthermore start the game clock and make the white side draw first.
        /// </summary>
        public ChessGame()
        {
            Board = ChessBoard.StartFormation;
            StartOfGame = DateTime.UtcNow;
            SideToDraw = ChessColor.White;
        }
        
        #endregion Constructor

        #region Members

        /// <summary>
        /// The chess board with the current game situation.
        /// </summary>
        public ChessBoard Board { get; set; }

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

        #endregion Members

        #region Methods

        /// <summary>
        /// Apply the chess draw to the current game situation on the chess board.
        /// Furthermore change the side that has to draw and store the chess draw in the chess draws history (stack).
        /// </summary>
        /// <param name="draw">The chess draw to be made</param>
        /// <param name="validate">Indicates whether the chess draw should be validated</param>
        public void ApplyDraw(ChessDraw draw, bool validate = false)
        {
            // info: Validate() throws an exception if the draw is invalid -> catch this exception and make use of the exception message
            if (!validate || (draw.IsValid(Board, _drawHistory?.Count > 0 ? (ChessDraw?)_drawHistory.Peek() : null)))
            {
                // draw the chess piece
                Board.ApplyDraw(draw);

                // apply the chess draw to the chess draws history
                _drawHistory.Push(draw);

                // change the side that has to draw
                SideToDraw = SideToDraw == ChessColor.White ? ChessColor.Black : ChessColor.White;
            }
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
            var board = new ChessBoard();
            _drawHistory.Reverse().ToList().ForEach(x => board.ApplyDraw(x));
            // TODO: test if Reverse() brings the correct behaviour
            
            // change the side that has to draw
            SideToDraw = SideToDraw == ChessColor.White ? ChessColor.Black : ChessColor.White;
        }

        #endregion Methods
    }
}
