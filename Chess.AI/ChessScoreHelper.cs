using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.AI
{
    public class ChessScoreHelper
    {
        #region Constants

        private double BASE_SCORE_PEASANT = 1.00;
        private double BASE_SCORE_KNIGHT  = 3.20;
        private double BASE_SCORE_BISHOP  = 3.33;
        private double BASE_SCORE_ROOK    = 5.10;
        private double BASE_SCORE_QUEEN   = 8.80;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Measure the value of the chess pieces on the given chess board of the given player.
        /// </summary>
        /// <param name="board">The chess board to be evaluated</param>
        /// <param name="sideToDraw">The chess player to be evaluated</param>
        /// <returns>the score of the chess player's game situation</returns>
        public double GetScore(ChessBoard board, ChessColor sideToDraw)
        {
            // get allied pieces and calculate the score
            var alliedPieces = (sideToDraw == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            double score = alliedPieces.Select(x => getPieceScore(board, x.Position)).Sum();

            return score;
        }

        private double getPieceScore(ChessBoard board, ChessPosition position)
        {
            double score = 0;
            var piece = board.GetPieceAt(position).Value;

            switch (piece.Type)
            {
                case ChessPieceType.King:                                              break;
                case ChessPieceType.Queen:   score = getQueenScore(board, position);   break;
                case ChessPieceType.Rook:    score = getRookScore(board, position);    break;
                case ChessPieceType.Bishop:  score = getBishopScore(board, position);  break;
                case ChessPieceType.Knight:  score = getKnightScore(board, position);  break;
                case ChessPieceType.Peasant: score = getPeasantScore(board, position); break;
            }

            return score;
        }

        private double getQueenScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise scores
            return BASE_SCORE_QUEEN;
        }

        private double getRookScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise scores
            return BASE_SCORE_ROOK;
        }

        private double getBishopScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise scores
            return BASE_SCORE_BISHOP;
        }

        private double getKnightScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise scores
            return BASE_SCORE_KNIGHT;
        }

        private double getPeasantScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise scores
            return BASE_SCORE_PEASANT;
        }

        #endregion Methods
    }
}
