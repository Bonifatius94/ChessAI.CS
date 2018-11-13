using System;
using System.Collections.Generic;
using System.Text;

namespace ChessAI.Lib
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
        /// A link to the chess board instance of the chess piece.
        /// </summary>
        public ChessBoard Board { get; set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Compute all possible draws for the given chess piece.
        /// </summary>
        /// <returns>a list of all possible draws</returns>
        public List<ChessDraw> GetPossibleDraws()
        {
            throw new NotImplementedException("Please implement the GetPossibleDraws() function!");
        }

        #endregion Methods
    }
}
