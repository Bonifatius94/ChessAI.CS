using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.UI.Player
{
    /// <summary>
    /// Representing a human chess player who puts his chess draws via CLI.
    /// </summary>
    public class UIChessPlayer : IChessPlayer
    {
        #region Constructor

        /// <summary>
        /// Initialize a new human chess player instance drawing the chess pieces of the given color.
        /// </summary>
        /// <param name="nextDrawCallback">The callback for retrieving the next chess draw.</param>
        public UIChessPlayer(Func<ChessBoard, ChessDraw?, ChessDraw> nextDrawCallback)
        {
            _nextDrawCallback = nextDrawCallback;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The callback for retrieving the next chess draw.
        /// </summary>
        private Func<ChessBoard, ChessDraw?, ChessDraw> _nextDrawCallback;

        #endregion Members

        #region Methods

        /// <summary>
        /// Make a human user put the next draw via CLI.
        /// </summary>
        /// <param name="board">The chess board representing the current game situation.</param>
        /// <param name="previousDraw">The preceding draw made by the enemy.</param>
        /// <returns>the next chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? previousDraw)
        {
            // return the draw
            return _nextDrawCallback.Invoke(board, previousDraw);
        }

        #endregion Methods
    }
}
