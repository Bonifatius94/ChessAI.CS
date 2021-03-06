﻿/*
 * MIT License
 *
 * Copyright(c) 2020 Marco Tröster
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
    public class SimpleChessScoreEstimator : IChessScoreEstimator
    {
        #region Constants

        // Shannon's chess piece evaluation
        // source: https://en.wikipedia.org/wiki/Chess_piece_relative_value

        /// <summary>
        /// The base score value when a player possesses a peasant according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_PEASANT = 1.00;

        /// <summary>
        /// The base score value when a player possesses a knight according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_KNIGHT = 3.00;

        /// <summary>
        /// The base score value when a player possesses a bishop according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_BISHOP = 3.00;

        /// <summary>
        /// The base score value when a player possesses a rook according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_ROOK = 5.00;

        /// <summary>
        /// The base score value when a player possesses a queen according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_QUEEN = 9.00;

        /// <summary>
        /// The base score value when a player possesses a king according to Shannon's scoring system.
        /// </summary>
        public const double BASE_SCORE_KING = 200.00;

        #endregion Constants

        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private SimpleChessScoreEstimator() { }

        /// <summary>
        /// Get singleton object reference.
        /// </summary>
        public static readonly IChessScoreEstimator Instance = new SimpleChessScoreEstimator();

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
                case ChessPieceType.King:    score = BASE_SCORE_KING;    break;
                case ChessPieceType.Queen:   score = BASE_SCORE_QUEEN;   break;
                case ChessPieceType.Rook:    score = BASE_SCORE_ROOK;    break;
                case ChessPieceType.Bishop:  score = BASE_SCORE_BISHOP;  break;
                case ChessPieceType.Knight:  score = BASE_SCORE_KNIGHT;  break;
                case ChessPieceType.Peasant: score = BASE_SCORE_PEASANT; break;
                default: throw new ArgumentException("unknown chess piece type detected!");
            }

            return score;
        }

        #endregion Methods
    }
}
