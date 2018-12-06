using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.AI
{
    /// <summary>
    /// Provides operations for evaluating a player's score according to the current game situation.
    /// </summary>
    public class ChessScoreHelper
    {
        #region Constants

        // Shannon's chess piece evaluation
        // source: https://en.wikipedia.org/wiki/Chess_piece_relative_value
        private const double BASE_SCORE_PEASANT =   1.00;
        private const double BASE_SCORE_KNIGHT  =   3.00;
        private const double BASE_SCORE_BISHOP  =   3.00;
        private const double BASE_SCORE_ROOK    =   5.00;
        private const double BASE_SCORE_QUEEN   =   9.00;
        private const double BASE_SCORE_KING    = 200.00;
        
        #endregion Constants

        #region Methods

        /// <summary>
        /// Measure the value of the chess pieces on the given chess board of the given player relative to his opponent's chess pieces.
        /// Therefore both player's scores are measured and then subtracted from each other. 
        /// A negative value means that the given player is behind in score and positive value obviously means that he is leading.
        /// </summary>
        /// <param name="board">The chess board to be evaluated</param>
        /// <param name="sideToDraw">The chess player to be evaluated</param>
        /// <returns>the score of the chess player's game situation</returns>
        public double GetScore(ChessBoard board, ChessColor sideToDraw)
        {
            // get allied pieces and calculate the score
            double allyScore = board.GetPiecesOfColor(sideToDraw).Select(x => getPieceScore(board, x.Position)).Sum();
            double enemyScore = board.GetPiecesOfColor(sideToDraw.Opponent()).Select(x => getPieceScore(board, x.Position)).Sum();

            // calculate the relative score compared to the opponent
            return allyScore - enemyScore;
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
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            double score = BASE_SCORE_KING;

            // bonus for rochade (king should be already moved and positioned at the margin of the base row)
            var king = board.GetPieceAt(position).Value;
            int baseRow = (king.Color == ChessColor.White) ? 0 : 7;
            bool isKingSafe = king.WasMoved && (position.Column < 2 || position.Column > 5) && position.Row == baseRow;
            if (isKingSafe) { score += 3; }
            
            return BASE_SCORE_KING;
        }

        private double getQueenScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            //int drawsCount = new ChessDrawGenerator().GetDraws(board, position, null, false).Count();
            return BASE_SCORE_QUEEN /*+ (0.05 * drawsCount)*/;
        }

        private double getRookScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            //int drawsCount = new ChessDrawGenerator().GetDraws(board, position, null, false).Count();
            //return BASE_SCORE_ROOK + (0.05 * drawsCount);

            double score = BASE_SCORE_ROOK;

            // bonus for developing piece
            var piece = board.GetPieceAt(position).Value;
            if (piece.WasMoved) { score += 0.2; }

            return score;
        }

        private double getBishopScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            //int drawsCount = new ChessDrawGenerator().GetDraws(board, position, null, false).Count();
            //return BASE_SCORE_BISHOP + (0.1 * drawsCount);

            double score = BASE_SCORE_BISHOP;

            // bonus for developing piece
            var piece = board.GetPieceAt(position).Value;
            if (piece.WasMoved) { score += 0.2; }

            return score;
        }

        private double getKnightScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            //int drawsCount = new ChessDrawGenerator().GetDraws(board, position, null, false).Count();
            //return BASE_SCORE_KNIGHT + (0.1 * drawsCount);

            double score = BASE_SCORE_KNIGHT;

            // bonus for developing piece
            var piece = board.GetPieceAt(position).Value;
            if (piece.WasMoved) { score += 0.2; }

            return score;
        }

        private double getPeasantScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_PEASANT;
            var piece = board.GetPieceAt(position).Value;
            
            // bonus the more the peasant advances (punish if peasant is not drawn)
            int advanceFactor = (piece.Color == ChessColor.White) ? (position.Row - 3) : (6 - position.Row);
            score += advanceFactor * 0.05;

            //var allPieces = board.Pieces.ToArray();

            //// bonus for connected peasants / malus for an isolated peasant
            //bool isConnected = allPieces.Any(x => x.Piece.Color == piece.Color && x.Piece.Type == ChessPieceType.Peasant && Math.Abs(x.Position.Column - position.Column) == 1);
            //score += (isConnected ? 1 : -1) * 0.05;

            //// malus for doubled peasants
            //bool isDoubled = allPieces.Any(x => x.Piece.Color == piece.Color && x.Piece.Type == ChessPieceType.Peasant && x.Position.Column == position.Column && x.Position.Row != position.Row);
            //if (isConnected) { score -= 0.1; }

            //// malus if peasant was passed by an enemy peasant
            //bool isPassed = allPieces.Any(x => x.Piece.Color == piece.Color.Opponent()
            //    && x.Piece.Type == ChessPieceType.Peasant && Math.Abs(x.Position.Column - position.Column) == 1 
            //    && ((x.Position.Row < position.Row && x.Piece.Color == ChessColor.White) || (x.Position.Row > position.Row && x.Piece.Color == ChessColor.Black))
            //);
            //if (isPassed) { score -= 0.1; }

            return score;
        }

        #endregion Methods
    }
}
