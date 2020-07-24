using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess.AI.Score
{
    /// <summary>
    /// Provides operations for evaluating a player's score according to the current game situation.
    /// </summary>
    public class HeuristicChessScoreEstimator : IChessScoreEstimator
    {
        #region Constants

        // Shannon's chess piece evaluation
        // source: https://en.wikipedia.org/wiki/Chess_piece_relative_value

        /// <summary>
        /// The base score value when a player possesses a peasant according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_PEASANT =   1.00;

        /// <summary>
        /// The base score value when a player possesses a knight according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_KNIGHT  =   3.00;

        /// <summary>
        /// The base score value when a player possesses a bishop according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_BISHOP  =   3.00;

        /// <summary>
        /// The base score value when a player possesses a rook according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_ROOK    =   5.00;

        /// <summary>
        /// The base score value when a player possesses a queen according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_QUEEN   =   9.00;

        /// <summary>
        /// The base score value when a player possesses a king according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_KING    = 200.00;

        #endregion Constants

        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private HeuristicChessScoreEstimator() { }

        /// <summary>
        /// Get singleton object reference.
        /// </summary>
        public static readonly IChessScoreEstimator Instance = new HeuristicChessScoreEstimator();

        #endregion Singleton

        #region Methods

        /// <summary>
        /// Measure the value of the chess pieces on the given chess board of the given player relative to his opponent's chess pieces.
        /// Therefore both player's scores are measured and then subtracted from each other. 
        /// A negative value means that the given player is behind in score and positive value obviously means that he is leading.
        /// </summary>
        /// <param name="board">The chess board to be evaluated</param>
        /// <param name="sideToDraw">The chess player to be evaluated</param>
        /// <returns>the score of the chess player's game situation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetScore(IChessBoard board, ChessColor sideToDraw)
        {
            // get allied pieces and calculate the score
            double allyScore = board.GetPiecesOfColor(sideToDraw).Select(x => getPieceScore(board, x.Position)).Sum();
            double enemyScore = board.GetPiecesOfColor(sideToDraw.Opponent()).Select(x => getPieceScore(board, x.Position)).Sum();

            // calculate the relative score: the own score compared to the opponent's score
            return allyScore - enemyScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getPieceScore(IChessBoard board, ChessPosition position)
        {
            double score;
            var piece = board.GetPieceAt(position);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getKingScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            double score = BASE_SCORE_KING;

            //// grant bonus if the king is protected at least by two pieces standing in front of him
            //// grant bonus if the king is likely moved to the outer base line region => make rochade more attractive
            //if (isKingAtOuterMarginOfBaseRow(board, position) && isKingCovered(board, position)) { score += 5; }

            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getQueenScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_QUEEN;

            //// bonus for mobility
            //score += getMovabilityBonus(board, position);

            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getRookScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_ROOK;

            //// bonus for developing and gained mobility
            //if (board.GetPieceAt(position).WasMoved) { score += getMovabilityBonus(board, position); }

            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getBishopScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_BISHOP;

            // bonus for developing and gained mobility
            if (board.GetPieceAt(position).WasMoved) { score += getMovabilityBonus(board, position); }

            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getKnightScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_KNIGHT;

            // bonus for developing and gained mobility
            if (board.GetPieceAt(position).WasMoved) { score += getMovabilityBonus(board, position); }

            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getPeasantScore(IChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_PEASANT;
            var piece = board.GetPieceAt(position);

            // bonus the more the peasant advances (but don't punish if peasant is not drawn)
            int advanceFactor = (piece.Color == ChessColor.White) ? (position.Row - 2) : (5 - position.Row);
            if (advanceFactor > 0) { score += advanceFactor * 0.04; }

            //// bonus for connected peasants / malus for an isolated peasant
            //int protectedRow = (piece.Color == ChessColor.White) ? (position.Row + 1) : (position.Row - 1);
            //var posLeft = new ChessPosition(protectedRow, position.Column - 1);
            //var posRight = new ChessPosition(protectedRow, position.Column + 1);
            //bool isConnected =
            //       (ChessPosition.AreCoordsValid(protectedRow, position.Column - 1) && board.IsCapturedAt(posLeft) && board.GetPieceAt(posLeft).Color == piece.Color)
            //    || (ChessPosition.AreCoordsValid(protectedRow, position.Column + 1) && board.IsCapturedAt(posRight) && board.GetPieceAt(posRight).Color == piece.Color);
            //score += (isConnected ? 1 : -1) * 0.05;

            //// malus for doubled peasants
            //bool isDoubled = board.GetPiecesOfColor(piece.Color).Any(x => x.Piece.Type == ChessPieceType.Peasant && x.Position.Column == position.Column && x.Position.Row != position.Row);
            //if (isConnected) { score -= 0.1; }

            //// malus if peasant was passed by an enemy peasant
            //bool isPassed = board.GetPiecesOfColor(piece.Color.Opponent()).Any(x =>
            //    x.Piece.Type == ChessPieceType.Peasant && Math.Abs(x.Position.Column - position.Column) == 1
            //    && ((x.Position.Row < position.Row && x.Piece.Color == ChessColor.White) || (x.Position.Row > position.Row && x.Piece.Color == ChessColor.Black))
            //);
            //if (isPassed) { score -= 0.1; }

            return score;
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getMovabilityBonus(IChessBoard board, ChessPosition position, double factor = 0.02)
        {
            int drawsCount = ChessDrawGenerator.Instance.GetDraws(board, position, null, false).Count();
            return factor * drawsCount;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private bool isKingAtOuterMarginOfBaseRow(ChessBoard board, ChessPosition position)
        //{
        //    var king = board.GetPieceAt(position);
        //    int baseRow = (king.Color == ChessColor.White) ? 0 : 7;
        //    return king.WasMoved && (position.Column < 3 || position.Column > 5) && position.Row == baseRow;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private bool isKingCovered(ChessBoard board, ChessPosition position)
        //{
        //    var king = board.GetPieceAt(position);
        //    int coveringPieces = 0;
        //    int coveringRow = (king.Color == ChessColor.White) ? 1 : 6;

        //    for (int i = -1; i < 2; i++)
        //    {
        //        if (ChessPosition.AreCoordsValid(coveringRow, position.Column + i))
        //        {
        //            var coveringPosition = new ChessPosition(coveringRow, position.Column + i);
        //            var coveringPiece = board.GetPieceAt(coveringPosition);
        //            if (board.IsCapturedAt(coveringPosition) && coveringPiece.Color == king.Color) { coveringPieces++; }
        //        }
        //    }

        //    return coveringPieces >= 2;
        //}

        #endregion Helpers

        #endregion Methods
    }
}
