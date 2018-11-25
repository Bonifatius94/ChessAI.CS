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
        Stalemate
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
            var possibleEnemyAnswers = enemyPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(simulatedBoard, x.Position, draw, false));

            // find out if the allied king could be taken by at least one enemy answer
            var alliedKing = (draw.DrawingSide == ChessColor.White) ? simulatedBoard.WhiteKing : simulatedBoard.BlackKing;
            bool ret = possibleEnemyAnswers.Any(x => x.NewPosition == alliedKing.Position);

            return ret;
        }

        /// <summary>
        /// Check whether the game situation on the given chess board is a simple check, check-mate or patt situation.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="precedingEnemyDraw"></param>
        /// <returns>a chess status</returns>
        public CheckGameStatus GetCheckGameStatus(ChessBoard board, ChessDraw precedingEnemyDraw)
        {
            // find out if any allied chess piece can draw
            var alliedSide = (precedingEnemyDraw.DrawingSide == ChessColor.White) ? ChessColor.Black : ChessColor.White;
            var alliedPieces = (alliedSide == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            //bool canAllyDraw = alliedPieces.Any(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, precedingEnemyDraw, true).Count() > 0);

            var alliedDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, precedingEnemyDraw, true));
            bool canAllyDraw = alliedDraws.Count() > 0;

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

        #endregion Methods
    }
}
