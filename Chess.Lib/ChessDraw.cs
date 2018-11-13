using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a chess draw that is made by a chess player.
    /// </summary>
    public class ChessDraw
    {
        #region Members

        /// <summary>
        /// The side that is drawing.
        /// </summary>
        public ChessPieceColor DrawingSide { get; set; }

        /// <summary>
        /// The piece type that is drawing.
        /// </summary>
        public ChessPieceType DrawingPieceType { get; set; }
        
        /// <summary>
        /// The old position of the chess piece to be moved.
        /// </summary>
        public ChessFieldPosition OldPosition { get; set; }

        /// <summary>
        /// The new position of the chess piece to be moved.
        /// </summary>
        public ChessFieldPosition NewPosition { get; set; }

        /// <summary>
        /// The enemy piece that was taken during this draw. The value is null if no enemy piece was taken.
        /// </summary>
        public ChessPieceType? TakenEnemyPiece { get; set; }

        /// <summary>
        /// Indicates whether the draw is a rochade.
        /// </summary>
        public bool IsRochade { get; set; }

        /// <summary>
        /// The timestamp when the chess player finalized the chess draw. Time is measured for UTC timezone.
        /// </summary>
        public DateTime Timestamp { get; set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Compute whether the draw is valid or not. If it is invalid, an exception with an according message is thrown.
        /// </summary>
        /// <param name="board">the chess board where the draw should be applied to</param>
        /// <param name="predecedingEnemyDraw">The last draw that the oponent made</param>
        /// <returns>boolean whether the draw is valid</returns>
        public bool Validate(ChessBoard board, ChessDraw predecedingEnemyDraw)
        {
            // get the piece to be drawn
            var piece = board.Fields[OldPosition.Row, OldPosition.Column].Piece;

            // make sure that there is a chess piece of the correct color that can be drawn
            if (piece == null) { throw new ArgumentException($"There is no chess piece on { OldPosition.FieldName }."); }
            if (piece.Color != DrawingSide) { throw new ArgumentException($"The chess piece on { OldPosition.FieldName } is owned by the opponent."); }

            // compute the possible chess draws for the given chess piece
            var possibleDraws = piece.GetPossibleDraws(predecedingEnemyDraw);

            // make sure there is at least one possible draw for the given chess piece
            if (possibleDraws?.Count <= 0) { throw new ArgumentException($"The chess piece on { OldPosition.FieldName } can not draw at all."); }

            // check if there is a possible draw with the same new position and is rochade flag (this implies that the given draw is valid)
            bool ret = possibleDraws.Any(x => x.IsRochade == IsRochade && x.NewPosition.Equals(NewPosition));

            return ret;
        }

        /// <summary>
        /// Get the name of the draw.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string color = DrawingSide.ToString().ToLower();

            return 
                IsRochade 
                    ? $"{ color } { ((OldPosition.Column == 0 && DrawingSide == ChessPieceColor.White) || (OldPosition.Column == 7 && DrawingSide == ChessPieceColor.Black) ? "left" : "right") }-side rochade"
                    : $"{ color } { DrawingPieceType.ToString().ToLower() } { OldPosition.FieldName }-{ NewPosition.FieldName }";
        }

        #endregion Methods
    }
}
