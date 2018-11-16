using System;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration of all chess piece types. The enumerations are represented by a character value:
    /// 
    ///  - King (K)
    ///  - Queen (Q)
    ///  - Rock (R)
    ///  - Bishop (B)
    ///  - Knight (H)
    ///  - Peasant (P)
    /// </summary>
    public enum ChessPieceType
    {
        King     = 'K',
        Queen    = 'Q',
        Rock     = 'R',
        Bishop   = 'B',
        Knight   = 'H', // 'H' like hourse (because 'K' is already taken by king)
        Peasant  = 'P'
    }

    /// <summary>
    /// An enumeration of all chess piece colors. The enumerations are represented by a character value:
    /// 
    ///  - Black (B)
    ///  - White (W)
    /// </summary>
    public enum ChessPieceColor
    {
        Black = 'B',
        White = 'W'
    }

    public class ChessPiece : ICloneable
    {
        #region Members

        /// <summary>
        /// The type of the chess piece.
        /// </summary>
        public ChessPieceType Type { get; set; }

        /// <summary>
        /// The color of the chess piece.
        /// </summary>
        public ChessPieceColor Color { get; set; }

        /// <summary>
        /// The position of the chess piece on the chess board.
        /// </summary>
        public ChessFieldPosition Position { get; set; }
        
        /// <summary>
        /// Indicates whether the chess piece was already drawn.
        /// </summary>
        public bool WasAlreadyDrawn { get; set; }

        #endregion Members

        #region Methods
        
        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        public object Clone()
        {
            var piece = new ChessPiece() {
                Type = Type,
                Color = Color,
                Position = (ChessFieldPosition)Position.Clone(),
                WasAlreadyDrawn = WasAlreadyDrawn
            };

            return piece;
        }

        #endregion Methods
    }
}
