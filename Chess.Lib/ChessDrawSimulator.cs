using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration representing the possible chess game outcomes.
    /// </summary>
    public enum ChessGameStatus
    {
        /// <summary>
        /// Nothing special. The drawing player is not checked and has options to draw.
        /// </summary>
        None,

        /// <summary>
        /// The drawing player is checked.
        /// </summary>
        Check,

        /// <summary>
        /// The drawing player is checkmate. (is checked and cannot defend the king)
        /// </summary>
        Checkmate,

        /// <summary>
        /// The drawing player is not checked, but has also no draw options left.
        /// </summary>
        Stalemate,

        /// <summary>
        /// The drawing player has not enough resources left to end the game with checkmate / stalemate.
        /// </summary>
        UnsufficientPieces,

        /// <summary>
        /// Both players do not have enough resources left to end the game (similar to unsufficient pieces).
        /// </summary>
        Tie
    }

    /// <summary>
    /// Helps simulating chess draws and their potential outcome.
    /// </summary>
    public class ChessDrawSimulator
    {
        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private ChessDrawSimulator() { }

        /// <summary>
        /// Get of singleton object reference.
        /// </summary>
        public static readonly ChessDrawSimulator Instance = new ChessDrawSimulator();

        #endregion Singleton

        #region Methods

        /// <summary>
        /// Simulate the given chess draw on the given chess board and determine whether it draws into an allied check situation.
        /// </summary>
        /// <param name="board">The chess board with the game situation to evaluate</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <returns>boolean that indicates whether the draw would cause an allied check situation</returns>
        public bool IsDrawIntoCheck(IChessBoard board, ChessDraw draw)
        {
            // TODO: remove clone operation if it causes performance issues

            // clone chess board and simulate the draw
            var simulatedBoard = board.ApplyDraw(draw);

            // get all enemy chess pieces and their possible answers
            var enemyPieces = simulatedBoard.GetPiecesOfColor(draw.DrawingSide.Opponent());
            var possibleEnemyAnswers = enemyPieces.SelectMany(piece => ChessDrawGenerator.Instance.GetDraws(simulatedBoard, piece.Position, draw, false));
            
            // find out if the allied king could be taken by at least one enemy answer
            var alliedKing = (draw.DrawingSide == ChessColor.White) ? simulatedBoard.WhiteKing : simulatedBoard.BlackKing;
            bool ret = possibleEnemyAnswers.Any(x => x.NewPosition == alliedKing.Position);

            return ret;
        }

        /// <summary>
        /// Check whether the game situation on the given chess board is a simple check, checkmate or tie situation.
        /// </summary>
        /// <param name="board">the chess board to be evaluated</param>
        /// <param name="precedingEnemyDraw">the preceding opponent chess draw</param>
        /// <returns>a check game status</returns>
        public ChessGameStatus GetCheckGameStatus(IChessBoard board, ChessDraw precedingEnemyDraw)
        {
            var alliedSide = (precedingEnemyDraw.DrawingSide == ChessColor.White) ? ChessColor.Black : ChessColor.White;
            var enemySide = precedingEnemyDraw.DrawingSide;

            // analyze the chess piece types on the board => determine whether any player can even achieve a checkmate with his remaining pieces
            bool canAllyCheckmate = canAchieveCheckmate(board, alliedSide);
            bool canEnemyCheckmate = canAchieveCheckmate(board, enemySide);

            // quit game status analysis if ally has lost due to unsufficient pieces
            if (!canAllyCheckmate && canEnemyCheckmate) { return ChessGameStatus.UnsufficientPieces; }
            if (!canAllyCheckmate && !canEnemyCheckmate) { return ChessGameStatus.Tie; }

            // find out if any allied chess piece can draw
            var alliedPieces = board.GetPiecesOfColor(alliedSide);
            bool canAllyDraw = alliedPieces.Any(piece => ChessDrawGenerator.Instance.GetDraws(board, piece.Position, precedingEnemyDraw, true).Count() > 0);

            // find out whether the allied king is checked
            var alliedKing = (alliedSide == ChessColor.White) ? board.WhiteKing : board.BlackKing;
            var enemyPieces = board.GetPiecesOfColor(alliedSide.Opponent());
            bool isAlliedKingChecked = enemyPieces.Any(piece => ChessDrawGenerator.Instance.GetDraws(board, piece.Position, null, false).Any(y => y.NewPosition == alliedKing.Position));
            
            // none:      ally can draw and is not checked
            // check:     ally is checked, but can at least draw
            // stalemate: ally cannot draw but is also not checked (end of game)
            // checkmate: ally is checked and cannot draw anymore (end of game)
            
            var status = 
                canAllyDraw
                    ? (isAlliedKingChecked ? ChessGameStatus.Check : ChessGameStatus.None) 
                    : (isAlliedKingChecked ? ChessGameStatus.Checkmate : ChessGameStatus.Stalemate);
            
            return status;
        }
        
        private bool canAchieveCheckmate(IChessBoard board, ChessColor side)
        {
            // minimal pieces required for checkmate: 
            // ======================================
            //  (1) king + queen
            //  (2) king + rook
            //  (3) king + 2 bishops (onto different chess field colors)
            //  (4) king + bishop + knight
            //  (5) king + 3 knights
            //  (6) king + peasant (with promotion)
            //
            // source: http://www.eudesign.com/chessops/basics/cpr-mate.htm
            
            // get all allied pieces
            var alliedPieces = board.GetPiecesOfColor(side);
            
            // determine whether the allied side can still achieve a checkmate
            bool ret = (
                // check if ally at least has his king + another piece (precondition for all options)
                alliedPieces.Count() >= 2 && alliedPieces.Any(x => x.Piece.Type == ChessPieceType.King)
                && 
                (
                    // check for options 1, 2, 6
                    alliedPieces.Any(x => x.Piece.Type == ChessPieceType.Queen || x.Piece.Type == ChessPieceType.Rook || x.Piece.Type == ChessPieceType.Peasant)
                    ||
                    // check for options 3, 4, 5
                    (
                        // check precondition of options 3, 4, 5
                        alliedPieces.Count() >= 3
                        && (
                            // check for option 3
                            alliedPieces.Where(x => x.Piece.Type == ChessPieceType.Bishop).Select(x => x.Position.ColorOfField)?.Distinct().Count() == 2
                            ||
                            // check for option 4
                            alliedPieces.Any(x => x.Piece.Type == ChessPieceType.Bishop) && alliedPieces.Any(x => x.Piece.Type == ChessPieceType.Knight)
                            ||
                            // check for option 5
                            alliedPieces.Where(x => x.Piece.Type == ChessPieceType.Knight).Count() >= 3
                        )
                    )
                )
            );

            return ret;
        }

        #endregion Methods
    }

    /// <summary>
    /// An extension evaluating whether the check game status indicates that the chess game is over.
    /// </summary>
    public static class CheckGameStatusGameOverEx
    {
        #region Methods

        /// <summary>
        /// Evaluates whether the given check game status indicates that the game is over.
        /// </summary>
        /// <param name="status">the game status to be evaluated</param>
        /// <returns>a boolean indicating whether the game is over</returns>
        public static bool IsGameOver(this ChessGameStatus status)
        {
            return !(status == ChessGameStatus.None || status == ChessGameStatus.Check);
        }

        #endregion Methods
    }
}
