using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            Board = new ChessBoard();
            StartOfGame = DateTime.UtcNow;
            SideToDraw = ChessPieceColor.White;
        }

        /// <summary>
        /// Create a chess game instance out of a list of all chess draws that have already been made by the chess players.
        /// </summary>
        /// <param name="draws"></param>
        //public ChessGame(List<ChessDraw> draws) : this()
        //{
        //    StartOfGame = draws.First().Timestamp;
        //    draws.ForEach(draw => ApplyDraw(draw));
        //}

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
        public ChessPieceColor SideToDraw { get; set; }

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
        public void ApplyDraw(ChessDraw draw)
        {
            // info: Validate() throws an exception if the draw is invalid -> catch this exception and make use of the exception message
            if (draw.Validate(Board, _drawHistory.Peek()))
            {
                // draw the chess piece
                Board.ApplyDraw(draw);

                // apply the chess draw to the chess draws history
                _drawHistory.Push(draw);

                // change the side that has to draw
                SideToDraw = SideToDraw == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
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
            
            // change the side that has to draw
            SideToDraw = SideToDraw == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        }

        #endregion Methods
    }
}
