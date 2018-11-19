﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        // TODO: try to model the different chess draw types better or at least improve the constructors so they do not get distinguished when being used

        // TODO: remove rochade / en-passant constructor and express the draw type by using king's / peasant's new position

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

        ///// <summary>
        ///// Create a new chess draw instance representing a standard chess draw.
        ///// </summary>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="drawingPieceType">The type of the drawing chess piece</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece</param>
        ///// <param name="newPosition">The new position of the drawing chess piece</param>
        ///// <param name="takenEnemyPiece">The type of the taken enemy piece</param>
        //public ChessDraw(ChessPieceColor drawingSide, ChessPieceType drawingPieceType, ChessFieldPosition oldPosition, ChessFieldPosition newPosition, ChessPieceType? takenEnemyPiece = null)
        //{
        //    Type = ChessDrawType.Standard;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = drawingPieceType;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = takenEnemyPiece;

        //    PeasantPromotionPieceType = null;
        //}

        ///// <summary>
        ///// Create a new chess draw instance representing a rochade or an en-passant.
        ///// </summary>
        ///// <param name="type">The type of the chess draw</param>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece (rochade: rock)</param>
        ///// <param name="newPosition">The new position of the drawing chess piece (rochade: rock)</param>
        //public ChessDraw(ChessDrawType type, ChessPieceColor drawingSide, ChessFieldPosition oldPosition, ChessFieldPosition newPosition)
        //{
        //    // make sure the chess draw type is an en-passant or rochade
        //    if (type != ChessDrawType.EnPassant && type != ChessDrawType.Rochade) { throw new ArgumentException($"Illegal chess draw type { type.ToString() } detected (expected EnPassant or Rochade)."); }

        //    Type = type;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = (type == ChessDrawType.EnPassant) ? ChessPieceType.Peasant : ChessPieceType.Rock;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = (type == ChessDrawType.EnPassant) ? (ChessPieceType?)ChessPieceType.Peasant : null;

        //    PeasantPromotionPieceType = null;
        //}

        ///// <summary>
        ///// Create a new chess draw instance representing a peasant promotion.
        ///// </summary>
        ///// <param name="type">The type of the chess draw</param>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="drawingPieceType">The type of the drawing chess piece</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece</param>
        ///// <param name="newPosition">The new position of the drawing chess piece</param>
        ///// <param name="peasantPromotionPieceType">The new type of the peasant after the promotion</param>
        ///// <param name="takenEnemyPiece">The type of the taken enemy piece</param>
        //public ChessDraw(ChessDrawType type, ChessPieceColor drawingSide, ChessPieceType drawingPieceType, ChessFieldPosition oldPosition, ChessFieldPosition newPosition, 
        //    ChessPieceType peasantPromotionPieceType, ChessPieceType? takenEnemyPiece = null)
        //{
        //    // make sure the chess draw type is a peasant promotion
        //    if (type != ChessDrawType.PeasantPromotion) { throw new ArgumentException($"Illegal chess draw type { type.ToString() } detected (expected PeasantPromotion)."); }

        //    Type = type;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = drawingPieceType;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = takenEnemyPiece;
        //    PeasantPromotionPieceType = peasantPromotionPieceType;
        //}        ///// <summary>
        ///// Create a new chess draw instance representing a standard chess draw.
        ///// </summary>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="drawingPieceType">The type of the drawing chess piece</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece</param>
        ///// <param name="newPosition">The new position of the drawing chess piece</param>
        ///// <param name="takenEnemyPiece">The type of the taken enemy piece</param>
        //public ChessDraw(ChessPieceColor drawingSide, ChessPieceType drawingPieceType, ChessFieldPosition oldPosition, ChessFieldPosition newPosition, ChessPieceType? takenEnemyPiece = null)
        //{
        //    Type = ChessDrawType.Standard;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = drawingPieceType;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = takenEnemyPiece;

        //    PeasantPromotionPieceType = null;
        //}

        ///// <summary>
        ///// Create a new chess draw instance representing a rochade or an en-passant.
        ///// </summary>
        ///// <param name="type">The type of the chess draw</param>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece (rochade: rock)</param>
        ///// <param name="newPosition">The new position of the drawing chess piece (rochade: rock)</param>
        //public ChessDraw(ChessDrawType type, ChessPieceColor drawingSide, ChessFieldPosition oldPosition, ChessFieldPosition newPosition)
        //{
        //    // make sure the chess draw type is an en-passant or rochade
        //    if (type != ChessDrawType.EnPassant && type != ChessDrawType.Rochade) { throw new ArgumentException($"Illegal chess draw type { type.ToString() } detected (expected EnPassant or Rochade)."); }

        //    Type = type;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = (type == ChessDrawType.EnPassant) ? ChessPieceType.Peasant : ChessPieceType.Rock;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = (type == ChessDrawType.EnPassant) ? (ChessPieceType?)ChessPieceType.Peasant : null;

        //    PeasantPromotionPieceType = null;
        //}

        ///// <summary>
        ///// Create a new chess draw instance representing a peasant promotion.
        ///// </summary>
        ///// <param name="type">The type of the chess draw</param>
        ///// <param name="drawingSide">The side that has to draw</param>
        ///// <param name="drawingPieceType">The type of the drawing chess piece</param>
        ///// <param name="oldPosition">The old position of the drawing chess piece</param>
        ///// <param name="newPosition">The new position of the drawing chess piece</param>
        ///// <param name="peasantPromotionPieceType">The new type of the peasant after the promotion</param>
        ///// <param name="takenEnemyPiece">The type of the taken enemy piece</param>
        //public ChessDraw(ChessDrawType type, ChessPieceColor drawingSide, ChessPieceType drawingPieceType, ChessFieldPosition oldPosition, ChessFieldPosition newPosition, 
        //    ChessPieceType peasantPromotionPieceType, ChessPieceType? takenEnemyPiece = null)
        //{
        //    // make sure the chess draw type is a peasant promotion
        //    if (type != ChessDrawType.PeasantPromotion) { throw new ArgumentException($"Illegal chess draw type { type.ToString() } detected (expected PeasantPromotion)."); }

        //    Type = type;
        //    DrawingSide = drawingSide;
        //    DrawingPieceType = drawingPieceType;
        //    OldPosition = oldPosition;
        //    NewPosition = newPosition;
        //    TakenEnemyPiece = takenEnemyPiece;
        //    PeasantPromotionPieceType = peasantPromotionPieceType;
        //}

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

        ///// <summary>
        ///// The type of the enemy chess piece that gets taken by this draw. The value is null if no enemy piece was taken.
        ///// </summary>
        //public ChessPieceType? TakenEnemyPiece { get; }
        
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
                    // TODO: rework this using the new position of the king as indicator
                    bool isLeftSide = (OldPosition.Column == 0 && DrawingSide == ChessColor.White) || (OldPosition.Column == 7 && DrawingSide == ChessColor.Black);
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
