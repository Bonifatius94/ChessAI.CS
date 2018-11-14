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
                // get the chess piece to be drawn
                var piece = Board.Fields[draw.OldPosition].Piece;

                // draw the chess piece
                piece.Draw(draw.NewPosition);

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

            // retrieve the last draw from the draw history stack (and remove it from the stack; with pop operation)
            var draw = _drawHistory.Pop();

            // get the chess piece that was drawn and move it back
            var piece = Board.Fields[draw.NewPosition].Piece;
            piece.Draw(draw.OldPosition);

            // put the enemy chess piece back on the chess board (if an enemy piece was taken during the draw)
            if (draw.TakenEnemyPiece != null)
            {
                var enemyPiece = new ChessPiece() { Board = this.Board, Color = SideToDraw, Type = draw.TakenEnemyPiece.Value };
                Board.Pieces.Add(enemyPiece);
                enemyPiece.Draw(draw.NewPosition);
            }

            // change the side that has to draw
            SideToDraw = SideToDraw == ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        }

        #endregion Methods
    }
}
