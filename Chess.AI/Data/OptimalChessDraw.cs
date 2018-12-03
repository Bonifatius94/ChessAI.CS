using Chess.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Chess.AI.Data
{
    /// <summary>
    /// Representing an entity of an optimal chess draw and the corresponding game situation.
    /// </summary>
    public class OptimalChessDraw
    {
        #region Constructor

        /// <summary>
        /// Create an empty instance (for EF).
        /// </summary>
        public OptimalChessDraw() { }

        /// <summary>
        /// Create a new optimal chess draw instance with the given chess board and chess draw.
        /// </summary>
        /// <param name="board">the chess board with the game situation</param>
        /// <param name="optimalDraw">the optimal chess draw</param>
        public OptimalChessDraw(ChessBoard board, ChessDraw optimalDraw)
        {
            Board = board;
            OptimalDraw = optimalDraw;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The numberic ID column (primary key).
        /// </summary>
        [Key]
        public ulong ID { get; set; }

        /// <summary>
        /// The unique game situation hash containing the drawing side and the chess board with the game situation.
        /// </summary>
        [StringLength(64)]
        public string GameSituationHash { get; set; }

        /// <summary>
        /// The hash of the optimal chess draw for the given game situation.
        /// </summary>
        [StringLength(3)]
        public int OptimalDrawHash { get; set; }

        /// <summary>
        /// The chess board with the game situation.
        /// </summary>
        [NotMapped]
        public ChessBoard Board
        {
            get { return GameSituationHash.ToBoard(); }
            set { GameSituationHash = value.ToHash(); }
        }

        /// <summary>
        /// The optimal chess draw for the given game situation.
        /// </summary>
        [NotMapped]
        public ChessDraw OptimalDraw
        {
            get { return new ChessDraw(OptimalDrawHash); }
            set { OptimalDrawHash = value.GetHashCode(); }
        }

        #endregion Members
    }

    /// <summary>
    /// An extension for conversions between a hash code string and a chess board instance (both directions).
    /// </summary>
    public static class ChessBoardHashEx
    {
        #region Constants

        private const byte CHESS_PIECE_NULL = 0xFF;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Help converting a chess board instance to a hash string.
        /// </summary>
        /// <param name="board">the chess board to be converted</param>
        /// <returns>a hash string containing the data from the chess board</returns>
        public static string ToHash(this ChessBoard board)
        {
            string hash = string.Empty;

            for (byte pos = 0; pos < 64; pos++)
            {
                var piece = board.GetPieceAt(new ChessPosition(pos));
                hash += (char)(piece.HasValue ? piece.Value.GetHashCode() : CHESS_PIECE_NULL);
            }

            return hash;
        }

        /// <summary>
        /// Help converting a hash string to a chess board instance.
        /// </summary>
        /// <param name="hash">the hash string to be converted</param>
        /// <returns>a new chess board instance containing the data from the hash</returns>
        public static ChessBoard ToBoard(this string hash)
        {
            var board = new ChessBoard();

            for (byte pos = 0; pos < 64; pos++)
            {
                byte data = (byte)hash[pos];
                var piece = (data == CHESS_PIECE_NULL) ? (ChessPiece?)null : new ChessPiece(data);
                board.UpdatePieceAt(new ChessPosition(pos), piece);
            }

            return board;
        }

        #endregion Methods
    }
}
