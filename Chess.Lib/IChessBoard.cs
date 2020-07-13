using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// An interface exposing standard chess board functionality that may be commonly used.
    /// </summary>
    public interface IChessBoard
    {
        #region Members

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        IEnumerable<ChessPieceAtPos> WhitePieces { get; }

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        IEnumerable<ChessPieceAtPos> BlackPieces { get; }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        ChessPieceAtPos WhiteKing { get; }

        /// <summary>
        /// Selects the black king from the chess pieces list. (computed operation)
        /// </summary>
        ChessPieceAtPos BlackKing { get; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        bool IsCapturedAt(ChessPosition position);

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        bool IsCapturedAt(byte position);

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        ChessPiece GetPieceAt(ChessPosition position);

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        ChessPiece GetPieceAt(byte position);

        /// <summary>
        /// Retrieve all chess pieces of the given player's side.
        /// </summary>
        /// <param name="side">The player's side</param>
        /// <returns>a list of all chess pieces of the given player's side</returns>
        IEnumerable<ChessPieceAtPos> GetPiecesOfColor(ChessColor side);

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        ChessBoard ApplyDraw(ChessDraw draw);

        /// <summary>
        /// Draw the chess pieces to the given positions on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draws">The chess draws to be executed</param>
        ChessBoard ApplyDraws(IList<ChessDraw> draws);

        #endregion Methods
    }
}
