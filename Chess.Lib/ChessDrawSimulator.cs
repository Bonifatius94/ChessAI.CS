using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    public enum CheckGameStatus
    {
        None,
        Check,
        Checkmate,
        Stalemate,
        UnsufficientPieces,
        Tie
    }

    public class ChessDrawSimulator
    {
        #region Methods

        /// <summary>
        /// Simulate the given chess draw on the given chess board and determine whether it draws into an allied check situation.
        /// </summary>
        /// <param name="board">The chess board with the game situation to evaluate</param>
        /// <param name="draw">The chess draw to be evaluated</param>
        /// <returns>boolean that indicates whether the draw would cause an allied check situation</returns>
        public bool IsDrawIntoCheck(ChessBoard board, ChessDraw draw)
        {
            // TODO: remove clone operation if it causes performance issues

            // clone chess board and simulate the draw
            var simulatedBoard = (ChessBoard)board.Clone();
            simulatedBoard.ApplyDraw(draw);

            // get all enemy chess pieces and their possible answers
            var enemyPieces = (draw.DrawingSide == ChessColor.White) ? simulatedBoard.BlackPieces : simulatedBoard.WhitePieces;
            var possibleEnemyAnswers = enemyPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(simulatedBoard, piece.Position, draw, false));
            
            // find out if the allied king could be taken by at least one enemy answer
            var alliedKing = (draw.DrawingSide == ChessColor.White) ? simulatedBoard.WhiteKing : simulatedBoard.BlackKing;
            bool ret = possibleEnemyAnswers.Any(x => x.NewPosition == alliedKing.Position);

            return ret;
        }

        /// <summary>
        /// Check whether the game situation on the given chess board is a simple check, checkmate or tie situation.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="precedingEnemyDraw"></param>
        /// <returns>a chess status</returns>
        public CheckGameStatus GetCheckGameStatus(ChessBoard board, ChessDraw precedingEnemyDraw)
        {
            var alliedSide = (precedingEnemyDraw.DrawingSide == ChessColor.White) ? ChessColor.Black : ChessColor.White;
            var enemySide = precedingEnemyDraw.DrawingSide;

            // analyze the chess piece types on the board => determine whether any player can even achieve a checkmate with his remaining pieces
            bool canAllyCheckmate = canAchieveCheckmate(board, alliedSide);
            bool canEnemyCheckmate = canAchieveCheckmate(board, enemySide);

            // quit game status analysis if ally has lost due to unsufficient pieces
            if (!canAllyCheckmate && canEnemyCheckmate) { return CheckGameStatus.UnsufficientPieces; }

            // TODO: check if this is even possible
            if (!canAllyCheckmate && !canEnemyCheckmate) { return CheckGameStatus.Tie; }

            // find out if any allied chess piece can draw
            var alliedPieces = (alliedSide == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            bool canAllyDraw = alliedPieces.Any(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, precedingEnemyDraw, true).Count() > 0);

            // find out whether the allied king is checked
            var alliedKing = (alliedSide == ChessColor.White) ? board.WhiteKing : board.BlackKing;
            var enemyPieces = (alliedSide == ChessColor.White) ? board.BlackPieces : board.WhitePieces;
            bool isAlliedKingChecked = enemyPieces.Any(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, null, false).Any(y => y.NewPosition == alliedKing.Position));
            
            // none:      ally can draw and is not checked
            // check:     ally is checked, but can at least draw
            // stalemate: ally cannot draw but is also not checked (end of game)
            // checkmate: ally is checked and cannot draw anymore (end of game)
            
            var status = 
                canAllyDraw
                    ? (isAlliedKingChecked ? CheckGameStatus.Check : CheckGameStatus.None) 
                    : (isAlliedKingChecked ? CheckGameStatus.Checkmate : CheckGameStatus.Stalemate);
            
            return status;
        }
        
        private bool canAchieveCheckmate(ChessBoard board, ChessColor side)
        {
            // TODO: validate this logic

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
            var alliedPieces = (side == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            
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
}
