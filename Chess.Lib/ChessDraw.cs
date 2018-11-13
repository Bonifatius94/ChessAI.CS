using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// The chess piece to be moved.
        /// When it's a rochade draw, always specify the rock (king is unambiguous).
        /// </summary>
        [JsonIgnore]
        public ChessPiece Piece { get; set; }

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
        /// Compute whether the draw is valid or not.
        /// </summary>
        /// <param name="board">the chess board where the draw should be applied to</param>
        /// <returns>boolean whether the draw is valid</returns>
        public bool IsValid(ChessBoard board)
        {
            throw new NotImplementedException("Please implement draw validation logic!");
        }

        /// <summary>
        /// Get the name of the draw.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string color = Piece.Color.ToString().ToLower();

            return 
                IsRochade 
                    ? $"{ color } { (OldPosition.Column == 0 && Piece.Color == ChessPieceColor.White || OldPosition.Column == 7 && Piece.Color == ChessPieceColor.Black ? "left" : "right") }-side rochade"
                    : $"{ color } { Piece.Type.ToString().ToLower() } { OldPosition.FieldName }-{ NewPosition.FieldName }";
        }

        #endregion Methods
    }
}
