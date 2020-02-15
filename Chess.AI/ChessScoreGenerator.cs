﻿using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chess.AI
{
    /// <summary>
    /// Provides operations for evaluating a player's score according to the current game situation.
    /// </summary>
    public class ChessScoreGenerator
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
        private ChessScoreGenerator() { }

        /// <summary>
        /// Get singleton object reference.
        /// </summary>
        public static readonly ChessScoreGenerator Instance = new ChessScoreGenerator();

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
        public double GetScore(ChessBoard board, ChessColor sideToDraw)
        {
            // get allied pieces and calculate the score
            double allyScore = board.GetPiecesOfColor(sideToDraw).Select(x => getPieceScore(board, x.Position)).Sum();
            double enemyScore = board.GetPiecesOfColor(sideToDraw.Opponent()).Select(x => getPieceScore(board, x.Position)).Sum();

            // calculate the relative score: the own score compared to the opponent's score
            return allyScore - enemyScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getPieceScore(ChessBoard board, ChessPosition position)
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
        private double getKingScore(ChessBoard board, ChessPosition position)
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
        private double getQueenScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            return BASE_SCORE_QUEEN + getMovabilityBonus(board, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getRookScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            return BASE_SCORE_ROOK + getMovabilityBonus(board, position);

            //double score = BASE_SCORE_ROOK;

            ////// bonus for developing piece
            ////var piece = board.GetPieceAt(position).Value;
            ////if (piece.WasMoved) { score += 0.2; }

            //return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getBishopScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            return BASE_SCORE_BISHOP + getMovabilityBonus(board, position);

            //double score = BASE_SCORE_BISHOP;

            ////// bonus for developing piece
            ////var piece = board.GetPieceAt(position).Value;
            ////if (piece.WasMoved) { score += 0.2; }

            //return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getKnightScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic
            return BASE_SCORE_KNIGHT + getMovabilityBonus(board, position);

            //double score = BASE_SCORE_KNIGHT;

            //// bonus for developing piece
            //var piece = board.GetPieceAt(position).Value;
            //if (piece.WasMoved) { score += 0.2; }

            //// malus for moving to the margin of the chess board
            //bool isAtMargin = position.Row == 0 || position.Row == 7 || position.Column == 0 || position.Column == 7;
            //if (isAtMargin) { score -= 0.5; }

            //return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getPeasantScore(ChessBoard board, ChessPosition position)
        {
            // Shannon's chess piece evaluation
            // TODO: check this heuristic

            double score = BASE_SCORE_PEASANT;
            var piece = board.GetPieceAt(position);

            // bonus the more the peasant advances (punish if peasant is not drawn)
            int advanceFactor = (piece.Color == ChessColor.White) ? (position.Row - 4) : (5 - position.Row);
            score += advanceFactor * 0.1;

            //// bonus for connected peasants / malus for an isolated peasant
            //int protectedRow = (piece.Color == ChessColor.White) ? (position.Row + 1) : (position.Row - 1);
            //bool isConnected =
            //       (ChessPosition.AreCoordsValid(protectedRow, position.Column - 1) && board.GetPieceAt(new ChessPosition(protectedRow, position.Column - 1))?.Color == piece.Color)
            //    || (ChessPosition.AreCoordsValid(protectedRow, position.Column + 1) && board.GetPieceAt(new ChessPosition(protectedRow, position.Column + 1))?.Color == piece.Color);
            //score += (isConnected ? 1 : -1) * 0.05;

            //// malus for doubled peasants
            //bool isDoubled = board.GetPiecesOfColor(piece.Color).Any(x => x.Piece.Type == ChessPieceType.Peasant && x.Position.Column == position.Column && x.Position.Row != position.Row);
            //if (isConnected) { score -= 0.1; }

            //// malus if peasant was passed by an enemy peasant
            //bool isPassed = allPieces.Any(x => x.Piece.Color == piece.Color.Opponent()
            //    && x.Piece.Type == ChessPieceType.Peasant && Math.Abs(x.Position.Column - position.Column) == 1 
            //    && ((x.Position.Row < position.Row && x.Piece.Color == ChessColor.White) || (x.Position.Row > position.Row && x.Piece.Color == ChessColor.Black))
            //);
            //if (isPassed) { score -= 0.1; }

            return score;
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double getMovabilityBonus(ChessBoard board, ChessPosition position, double factor = 0.01)
        {
            int drawsCount = ChessDrawGenerator.Instance.GetDraws(board, position, null, false).Count();
            return factor * drawsCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isKingAtOuterMarginOfBaseRow(ChessBoard board, ChessPosition position)
        {
            var king = board.GetPieceAt(position);
            int baseRow = (king.Color == ChessColor.White) ? 0 : 7;
            return king.WasMoved && (position.Column < 3 || position.Column > 5) && position.Row == baseRow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isKingCovered(ChessBoard board, ChessPosition position)
        {
            var king = board.GetPieceAt(position);
            int coveringPieces = 0;
            int coveringRow = (king.Color == ChessColor.White) ? 1 : 6;

            for (int i = -1; i < 2; i++)
            {
                if (ChessPosition.AreCoordsValid(coveringRow, position.Column + i))
                {
                    var coveringPosition = new ChessPosition(coveringRow, position.Column + i);
                    var coveringPiece = board.GetPieceAt(coveringPosition);
                    if (board.IsCapturedAt(coveringPosition) && coveringPiece.Color == king.Color) { coveringPieces++; }
                }
            }

            return coveringPieces >= 2;
        }

        #endregion Helpers

        #endregion Methods
    }
}