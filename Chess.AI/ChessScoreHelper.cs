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

        private double BASE_SCORE_PEASANT =   1.00;
        private double BASE_SCORE_KNIGHT  =   3.20;
        private double BASE_SCORE_BISHOP  =   3.33;
        private double BASE_SCORE_ROOK    =   5.10;
        private double BASE_SCORE_QUEEN   =   8.80;
        private double BASE_SCORE_KING    = 100.00;

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
            double whiteScore = board.WhitePieces.Select(x => getPieceScore(board, x.Position)).Sum();
            double blackScore = board.BlackPieces.Select(x => getPieceScore(board, x.Position)).Sum();

            // calculate the relative score compared to the opponent
            double diff = (sideToDraw == ChessColor.White) ? (whiteScore - blackScore) : (blackScore - whiteScore);
            return diff;
        }

        private double getPieceScore(ChessBoard board, ChessPosition position)
        {
            double score;
            var piece = board.GetPieceAt(position).Value;

            switch (piece.Type)
            {
                case ChessPieceType.King:    score = getKingScore(board, position);    break;
                case ChessPieceType.Queen:   score = getQueenScore(board, position);   break;
                case ChessPieceType.Rook:    score = getRookScore(board, position);    break;
                case ChessPieceType.Bishop:  score = getBishopScore(board, position);  break;
                case ChessPieceType.Knight:  score = getKnightScore(board, position);  break;
                case ChessPieceType.Peasant: score = getPeasantScore(board, position); break;
                default: throw new ArgumentException("unknown chess piece type detected!");
            }

            return score;
        }

        private double getKingScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_KING;
        }

        private double getQueenScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_QUEEN;
        }

        private double getRookScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_ROOK;
        }

        private double getBishopScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_BISHOP;
        }

        private double getKnightScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_KNIGHT;
        }

        private double getPeasantScore(ChessBoard board, ChessPosition position)
        {
            // TODO: implement more precise score heuristic
            return BASE_SCORE_PEASANT;
        }

        #endregion Methods
    }
}
