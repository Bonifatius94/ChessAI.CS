﻿using System;
using System.Linq;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration representing the chess draw types.
    /// </summary>
    public enum ChessDrawType
    {
        /// <summary>
        /// Representing a standard draw.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Representing a rochade / casteling draw.
        /// </summary>
        Rochade = 1,

        /// <summary>
        /// Representing a en-passant draw.
        /// </summary>
        EnPassant = 2,

        /// <summary>
        /// Representing a peasant promotion draw.
        /// </summary>
        PeasantPromotion = 3
    }

    /// <summary>
    /// Represents a chess draw made by a chess player.
    /// </summary>
    public readonly struct ChessDraw
    {
        #region Constants

        // define the trailing bits after the data bits
        private const int DRAWING_SIDE_TRAILING_BITS         = 23;
        private const int DRAW_TYPE_TRAILING_BITS            = 21;
        private const int DRAWING_PIECE_TYPE_TRAILING_BITS   = 18;
        private const int TAKEN_PIECE_TYPE_TRAILING_BITS     = 15;
        private const int PROMOTION_PIECE_TYPE_TRAILING_BITS = 12;
        private const int OLD_POSITION_TRAILING_BITS         =  6;

        // define which bits of the hash code store the data
        private const int BITS_OF_DRAWING_SIDE          = 0b_100000000000000000000000; // bits: 10000000 00000000 00000000
        private const int BITS_OF_DRAW_TYPE             = 0b_011000000000000000000000; // bits: 01100000 00000000 00000000
        private const int BITS_OF_DRAWING_PIECE_TYPE    = 0b_000111000000000000000000; // bits: 00011100 00000000 00000000
        private const int BITS_OF_TAKEN_PIECE_TYPE      = 0b_000000111000000000000000; // bits: 00000011 10000000 00000000
        private const int BITS_OF_PROMOTION_PIECE_TYPE  = 0b_000000000111000000000000; // bits: 00000000 01110000 00000000
        private const int BITS_OF_OLD_POSITION          = 0b_000000000000111111000000; // bits: 00000000 00001111 11000000
        private const int BITS_OF_NEW_POSITION          = 0b_000000000000000000111111; // bits: 00000000 00000000 00111111

        // define null value of chess piece type
        private const byte CHESS_PIECE_TYPE_NULL = 6;

        #endregion Constants

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

            // determine all property values
            var type = getDrawType(board, oldPos, newPos, peasantPromotionType);
            var drawingSide = piece.Color;
            var drawingPieceType = piece.Type;

            var takenPieceType = 
                (type == ChessDrawType.EnPassant) 
                    ? (ChessPieceType?)ChessPieceType.Peasant 
                    : (board.IsCapturedAt(newPos) ? (ChessPieceType?)board.GetPieceAt(newPos).Value.Type : null);

            // transform property values to a hash code
            _hashCode = toHashCode(type, drawingSide, drawingPieceType, takenPieceType, peasantPromotionType, oldPos, newPos);
        }

        /// <summary>
        /// Create a new chess draw instance by passing the given hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the new chess draw instance</param>
        public ChessDraw(int hashCode)
        {
            // make sure the hash code is within value range
            if (hashCode < 0 || hashCode >= 0b_1000000000000000000000000) { throw new ArgumentException("invalid hash code detected (expected value of set { 0, 1, ..., 2^24 - 1 })"); }

            _hashCode = hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The binary representation containing the chess draw data.
        /// 
        /// The code consists of 24 bits: 
        /// 1 bit for drawing side, 2 bits for draw type, 3 bits for each drawing piece type, taken piece type and promotion piece type, 6 bits for each old and new position.
        /// (taken piece type and promotion type use 6 as null)
        /// 
        /// |   unused  | side | draw type | piece type | taken piece type | promotion type | old position | new position |
        /// |  xxxxxxxx |    x |        xx |        xxx |              xxx |            xxx |       xxxxxx |       xxxxxx |
        /// </summary>
        private readonly int _hashCode;
        
        /// <summary>
        /// The type of the draw.
        /// </summary>
        public ChessDrawType Type { get { return (ChessDrawType)((_hashCode & BITS_OF_DRAW_TYPE) >> DRAW_TYPE_TRAILING_BITS); } }

        /// <summary>
        /// The side that is drawing.
        /// </summary>
        public ChessColor DrawingSide { get { return (ChessColor)((_hashCode & BITS_OF_DRAWING_SIDE) >> DRAWING_SIDE_TRAILING_BITS); } }

        /// <summary>
        /// The piece type that is drawing.
        /// </summary>
        public ChessPieceType DrawingPieceType { get { return (ChessPieceType)((_hashCode & BITS_OF_DRAWING_PIECE_TYPE) >> DRAWING_PIECE_TYPE_TRAILING_BITS); } }
        
        /// <summary>
        /// The chess piece type of the enemy chess piece being taken.
        /// </summary>
        public ChessPieceType? TakenPieceType
        {
            get
            {
                int digits = (_hashCode & BITS_OF_TAKEN_PIECE_TYPE) >> TAKEN_PIECE_TYPE_TRAILING_BITS;
                return (digits != CHESS_PIECE_TYPE_NULL) ? (ChessPieceType?)((ChessPieceType)digits) : null;
            }
        }

        /// <summary>
        /// The chess piece type that the peasant is promoting to.
        /// </summary>
        public ChessPieceType? PeasantPromotionPieceType
        {
            get
            {
                int digits = (_hashCode & BITS_OF_PROMOTION_PIECE_TYPE) >> PROMOTION_PIECE_TYPE_TRAILING_BITS;
                return (digits != CHESS_PIECE_TYPE_NULL) ? (ChessPieceType?)((ChessPieceType)digits) : null;
            }
        }

        /// <summary>
        /// The old position of the chess piece to be moved.
        /// </summary>
        public ChessPosition OldPosition { get { return new ChessPosition((byte)((_hashCode & BITS_OF_OLD_POSITION) >> OLD_POSITION_TRAILING_BITS)); } }

        /// <summary>
        /// The new position of the chess piece to be moved.
        /// </summary>
        public ChessPosition NewPosition { get { return new ChessPosition((byte)(_hashCode & BITS_OF_NEW_POSITION)); } }

        #endregion Members

        #region Methods

        private static int toHashCode(ChessDrawType drawType, ChessColor drawingSide, ChessPieceType drawingPieceType, ChessPieceType? takenPieceType,
            ChessPieceType? promotionPieceType, ChessPosition oldPosition, ChessPosition newPosition)
        {
            // shift the bits to the right position (preparation for bitwise OR)
            int drawTypeBits = ((int)drawType) << DRAW_TYPE_TRAILING_BITS;
            int drawingSideBits = ((int)drawingSide) << DRAWING_SIDE_TRAILING_BITS;
            int drawingPieceTypeBits = ((int)drawingPieceType) << DRAWING_PIECE_TYPE_TRAILING_BITS;
            int takenPieceTypeBits = ((takenPieceType != null) ? (int)takenPieceType.Value : CHESS_PIECE_TYPE_NULL) << TAKEN_PIECE_TYPE_TRAILING_BITS;
            int promotionPieceTypeBits = ((promotionPieceType != null) ? (int)promotionPieceType.Value : CHESS_PIECE_TYPE_NULL) << PROMOTION_PIECE_TYPE_TRAILING_BITS;
            int oldPositionBits = oldPosition.GetHashCode() << OLD_POSITION_TRAILING_BITS;
            int newPositionBits = newPosition.GetHashCode();

            // fuse the shifted bits to the hash code (by bitwise OR)
            int hashCode = (drawTypeBits | drawingSideBits | drawingPieceTypeBits | takenPieceTypeBits | promotionPieceTypeBits | oldPositionBits | newPositionBits);

            return hashCode;
        }

        private static ChessDrawType getDrawType(ChessBoard board, ChessPosition oldPos, ChessPosition newPos, ChessPieceType? peasantPromotionType)
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
            else if (piece.Type == ChessPieceType.Peasant && board.GetPieceAt(newPos) == null && Math.Abs(oldPos.Column - newPos.Column) == 1)
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
        public bool IsValid(ChessBoard board, ChessDraw? predecedingEnemyDraw = null)
        {
            // get the piece to be drawn
            var piece = board.GetPieceAt(OldPosition);

            // make sure that there is a chess piece of the correct color that can be drawn
            if (piece == null) { throw new ArgumentException($"There is no chess piece on { OldPosition.FieldName }."); }
            if (piece.Value.Color != DrawingSide) { throw new ArgumentException($"The chess piece on { OldPosition.FieldName } is owned by the opponent."); }

            // compute the possible chess draws for the given chess piece
            var possibleDraws = ChessDrawGenerator.Instance.GetDraws(board, OldPosition, predecedingEnemyDraw, true);
            
            // check if there is a possible draw with the same new position and type (this implies that the given draw is valid)
            var type = Type;
            var position = NewPosition;
            bool ret = (possibleDraws?.Count() > 0) && possibleDraws.Any(x => x.Type == type && x.NewPosition == position);

            return ret;
        }

        /// <summary>
        /// Retrieve the name of the chess draw.
        /// </summary>
        /// <returns>the name of the chess draw</returns>
        public override string ToString()
        {
            string ret;
            string color = DrawingSide.ToString().ToLower();
            string drawingPiece = DrawingPieceType.ToString().ToLower().PadRight(7, ' ');
            string drawNameBase = $"{ color } { drawingPiece } { OldPosition.FieldName }-{ NewPosition.FieldName }";

            switch (Type)
            {
                case ChessDrawType.Standard:
                    ret = drawNameBase;
                    break;
                case ChessDrawType.Rochade:
                    bool isLeftSide = (NewPosition.Column == 2 && DrawingSide == ChessColor.White) || (NewPosition.Column == 6 && DrawingSide == ChessColor.Black);
                    ret = $"{ drawNameBase } ({ (isLeftSide ? "left" : "right") }-side rochade)";
                    break;
                case ChessDrawType.EnPassant:
                    ret = $"{ drawNameBase } (en-passant)";
                    break;
                case ChessDrawType.PeasantPromotion:
                    ret = $"{ drawNameBase } ({ PeasantPromotionPieceType.ToString().ToLower() })";
                    break;
                default:
                    throw new ArgumentException("Unsupported chess draw type detected!");
            }

            return ret;
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the properties.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns>a boolean that indicates whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof(ChessDraw)) && (((ChessDraw)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore the hash code of the instance is returned.
        /// </summary>
        /// <returns>a hash code that is unique for each chess draw</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Implements the '==' operator for comparing chess draws.
        /// </summary>
        /// <param name="c1">The first chess draw to compare</param>
        /// <param name="c2">The second chess draw to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are equal</returns>
        public static bool operator ==(ChessDraw c1, ChessDraw c2)
        {
            return c1.GetHashCode() == c2.GetHashCode();
        }

        /// <summary>
        /// Implements the '!=' operator for comparing chess draws.
        /// </summary>
        /// <param name="c1">The first chess draw to compare</param>
        /// <param name="c2">The second chess draw to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are not equal</returns>
        public static bool operator !=(ChessDraw c1, ChessDraw c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }
        
        #endregion Methods
    }
}
