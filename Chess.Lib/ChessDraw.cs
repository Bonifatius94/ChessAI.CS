using System;
using System.Linq;

namespace Chess.Lib
{
    public enum ChessDrawType
    {
        Standard,
        Rochade,
        EnPassant,
        PeasantPromotion
    }

    /// <summary>
    /// This class represents a chess draw that is made by a chess player.
    /// </summary>
    public readonly struct ChessDraw
    {
        #region Constructor
        
        /// <summary>
        /// Create a new chess draw instance representing a chess draw.
        /// </summary>
        /// <param name="board">The chess board that contains the detailed information</param>
        /// <param name="oldPos">The old position of the drawing chess piece</param>
        /// <param name="newPos">The new position of the drawing chess piece</param>
        /// <param name="peasantPromotionType">The new type of the peasant after its promotion (only relevant for peasant promotion)</param>
        public ChessDraw(ChessBoard board, ChessPosition oldPos, ChessPosition newPos, ChessPieceType? peasantPromotionType = null)
        {
            // get the drawing piece
            var piece = board.GetPieceAt(oldPos).Value;

            // set all properties
            Type = ChessDrawType.Standard;
            DrawingSide = piece.Color;
            DrawingPieceType = piece.Type;
            OldPosition = oldPos;
            NewPosition = newPos;
            PeasantPromotionPieceType = peasantPromotionType;

            // determine the draw type (first assignment is only a default value)
            Type = getDrawType(board, oldPos, newPos, peasantPromotionType);
        }

        #endregion Constructor

        #region Members
        
        /// <summary>
        /// The type of the draw (default: standrad).
        /// </summary>
        public ChessDrawType Type { get; }

        /// <summary>
        /// The side that is drawing.
        /// </summary>
        public ChessColor DrawingSide { get; }

        /// <summary>
        /// The piece type that is drawing.
        /// </summary>
        public ChessPieceType DrawingPieceType { get; }
        
        /// <summary>
        /// The old position of the chess piece to be moved.
        /// </summary>
        public ChessPosition OldPosition { get; }

        /// <summary>
        /// The new position of the chess piece to be moved.
        /// </summary>
        public ChessPosition NewPosition { get; }

        /// <summary>
        /// The chess piece type that the peasant is promoting to.
        /// </summary>
        public ChessPieceType? PeasantPromotionPieceType { get; }

        ///// <summary>
        ///// The timestamp when the chess player finalized the chess draw. Time is measured for UTC timezone.
        ///// </summary>
        //public DateTime? Timestamp { get; set; }
        
        #endregion Members

        #region Methods

        private ChessDrawType getDrawType(ChessBoard board, ChessPosition oldPos, ChessPosition newPos, ChessPieceType? peasantPromotionType)
        {
            var piece = board.GetPieceAt(oldPos).Value;
            var type = ChessDrawType.Standard;

            // check for a peasant promotion
            if (peasantPromotionType != null && piece.Type == ChessPieceType.Peasant && ((newPos.Row == 7 && piece.Color == ChessColor.White) || (newPos.Row == 0 && piece.Color == ChessColor.Black)))
            {
                type = ChessDrawType.PeasantPromotion;
            }
            // check for a rochade
            else if (piece.Type == ChessPieceType.King && Math.Abs(oldPos.Column - newPos.Column) == 2)
            {
                type = ChessDrawType.Rochade;
            }
            // check for an en-passant
            else if (piece.Type == ChessPieceType.Peasant && board.GetPieceAt(newPos) == null)
            {
                type = ChessDrawType.EnPassant;
            }

            return type;
        }

        /// <summary>
        /// Compute whether the draw is valid or not. If it is invalid, an exception with an according message is thrown.
        /// </summary>
        /// <param name="board">the chess board where the draw should be applied to</param>
        /// <param name="predecedingEnemyDraw">The last draw that the oponent made</param>
        /// <returns>boolean whether the draw is valid</returns>
        public bool Validate(ChessBoard board, ChessDraw predecedingEnemyDraw)
        {
            // get the piece to be drawn
            var piece = board.GetPieceAt(OldPosition);

            // make sure that there is a chess piece of the correct color that can be drawn
            if (piece == null) { throw new ArgumentException($"There is no chess piece on { OldPosition.FieldName }."); }
            if (piece.Value.Color != DrawingSide) { throw new ArgumentException($"The chess piece on { OldPosition.FieldName } is owned by the opponent."); }

            // compute the possible chess draws for the given chess piece
            var possibleDraws = new ChessDrawPossibilitiesHelper().GetPossibleDraws(board, piece.Value, this, true);

            // make sure there is at least one possible draw for the given chess piece
            if (possibleDraws?.Count <= 0) { throw new ArgumentException($"The chess piece on { OldPosition.FieldName } can not draw at all."); }

            // check if there is a possible draw with the same new position and type (this implies that the given draw is valid)
            var type = Type;
            var position = NewPosition;
            bool ret = possibleDraws.Any(x => x.Type == type && x.NewPosition.Equals(position));

            return ret;
        }

        /// <summary>
        /// Get the name of the draw.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string ret;
            string color = DrawingSide.ToString().ToLower();
            string drawingPiece = DrawingPieceType.ToString().ToLower();

            switch (Type)
            {
                case ChessDrawType.Standard:
                    ret = $"{ color } { drawingPiece } { OldPosition.ToString() }-{ NewPosition.ToString() }";
                    break;
                case ChessDrawType.Rochade:
                    bool isLeftSide = (NewPosition.Column == 2 && DrawingSide == ChessColor.White) || (OldPosition.Column == 6 && DrawingSide == ChessColor.Black);
                    ret = $"{ color } { (isLeftSide ? "left" : "right") }-side rochade";
                    break;
                case ChessDrawType.EnPassant:
                    ret = $"{ color } { drawingPiece } { OldPosition.FieldName }-{ NewPosition.FieldName } (en-passant)";
                    break;
                case ChessDrawType.PeasantPromotion:
                    ret = $"{ color } peasant promotion on { NewPosition.FieldName } ({ PeasantPromotionPieceType.ToString().ToLower() })";
                    break;
                default:
                    throw new ArgumentException("Unsupported chess draw type detected!");
            }
            
            return ret;
        }
        
        #endregion Methods
    }
}
