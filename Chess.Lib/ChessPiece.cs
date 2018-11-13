using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public class ChessPiece
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
        [JsonIgnore]
        public ChessFieldPosition Position { get; set; }

        /// <summary>
        /// A link to the chess board instance of the chess piece.
        /// </summary>
        [JsonIgnore]
        public ChessBoard Board { get; set; }

        /// <summary>
        /// Indicates whether the chess piece was already drawn.
        /// </summary>
        public bool WasAlreadyDrawn { get; set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Compute all possible draws for the given chess piece.
        /// </summary>
        /// <param name="precedingEnemyDraw">The preceding enemy draw</param>
        /// <returns>a list of all possible draws</returns>
        public List<ChessDraw> GetPossibleDraws(ChessDraw precedingEnemyDraw)
        {
            throw new NotImplementedException("Please implement the GetPossibleDraws() function!");
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken.
        /// </summary>
        /// <param name="newPosition"></param>
        public void Draw(ChessFieldPosition newPosition)
        {
            // get the destination chess field instance of the chess board
            var originalField = Board.Fields[Position.Row, Position.Column];
            var destinationField = Board.Fields[newPosition.Row, newPosition.Column];

            // take enemy piece (if there is one)
            if (destinationField.IsCapturedByPiece && destinationField.Piece.Color != Color)
            {
                Board.Pieces.Remove(destinationField.Piece);
            }

            // move piece from original field to the destination
            originalField.Piece = null;
            destinationField.Piece = this;

            // setter of ChessField.Piece already updates the Position property of this instance
            //Position = newPosition;
        }

        #endregion Methods
    }
}
