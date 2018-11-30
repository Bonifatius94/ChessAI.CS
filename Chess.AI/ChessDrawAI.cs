using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.AI
{
    /// <summary>
    /// An enumeration of chess game difficulties from easy to godlike. The enumeration values are assigned by numbers between 1 (easy) and 5 (godlike).
    /// </summary>
    public enum ChessDifficultyLevel
    {
        Stupid  = 0,
        Easy    = 1,
        Medium  = 2,
        Hard    = 3,
        Extreme = 4,
        Godlike = 5
    }

    public interface IChessDrawAI
    {
        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level);
    }

    public class ChessDrawAI : IChessDrawAI
    {
        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw (null on white-side's first draw)</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw? precedingEnemyDraw, ChessDifficultyLevel level)
        {
            // prepare draws to be analyzed
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
            var alliedPieces = board.GetPiecesOfColor(drawingSide);
            var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true));

            // make sure that the ally can draw
            if (draws.Count() == 0) { throw new ArgumentException("the given side cannot draw."); }

            // prepare minimax depth / alpha / beta
            int depth = ((int)level) * 2;
            double alpha = double.MinValue;
            double beta = double.MaxValue;

            // get the best draw
            var drawsWithScores = draws.AsParallel().Select(draw => new Tuple<ChessDraw, double>(draw, minimax(board, draw, depth, alpha, beta)));
            double maxScore = drawsWithScores.Max(x => x.Item2);
            var bestDraw = drawsWithScores.Where(x => x.Item2 == maxScore).First().Item1;
            
            return bestDraw;
        }
        
        private double minimax(ChessBoard board, ChessDraw? precedingEnemyDraw, int depth, double alpha, double beta, bool maximize = true)
        {
            // TODO: optimize this code, so it runs really fast

            double score;
            var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;

            // recursion anchor (leaf node)
            if (depth == 0)
            {
                score = new ChessScoreHelper().GetScore(board, drawingSide);
            }
            // recursion call (parent node)
            else
            {
                // init score with negative infinity when maximizing / positive infinity when minimizing
                score = maximize ? double.MinValue : double.MaxValue;

                // get all draws (child nodes)
                var alliedPieces = board.GetPiecesOfColor(drawingSide);
                var draws = alliedPieces.SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, true)).Shuffle().ToArray();

                for (int i = 0; i < draws.Length; i++)
                {
                    // simulate the draw
                    var simDraw = draws[i];
                    var simBoard = (ChessBoard)board.Clone();
                    simBoard.ApplyDraw(simDraw);

                    // update the minimax score of recursions
                    double minimaxScore = minimax(simBoard, simDraw, depth - 1, alpha, beta, !maximize);
                    score = maximize ? Math.Max(score, minimaxScore) : Math.Min(score, minimaxScore);

                    // update alpha / beta
                    if (maximize) { alpha = Math.Max(score, alpha); }
                    else          { beta  = Math.Min(score, beta);  }     
                    
                    // cut-off when alpha and beta overlap
                    if (alpha >= beta) { break; }
                }
            }

            return score;
        }

        //private List<Tuple<ChessDraw, double>> getChessDrawScores(ChessBoard board, ChessDraw? precedingEnemyDraw, int steps)
        //{
        //    // init variables
        //    var drawingSide = precedingEnemyDraw?.DrawingSide.Opponent() ?? ChessColor.White;
        //    double scoreAtStart = new ChessScoreHelper().GetScore(board, drawingSide);

        //    // get all possible chess draws
        //    var alliedPieces = board.GetPiecesOfColor(drawingSide);
        //    var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, precedingEnemyDraw, true)).ToList();

        //    // get the score for each draw as (draw, score) tuple
        //    var scores = possibleDraws.Select(draw => {

        //        var tempBoard = new ChessBoard(board.Pieces);
        //        tempBoard.ApplyDraw(draw);
        //        double tempScore = new ChessScoreHelper().GetScore(tempBoard, drawingSide);
        //        return new Tuple<ChessDraw, double>(draw, tempScore);

        //    // only retrieve draws that have a relatively positive impact on the player's score
        //    }).Where(x => x.Item2 >= scoreAtStart - 1).ToList();

        //    // go to the next level
        //    if (steps > 0)
        //    {
        //        // evaluate the chess draws by taking the next level in consideration
        //        var nextDrawScores = scores.Select(score => {

        //            // simulate the draw
        //            var tempDraw = score.Item1;
        //            var tempBoard = new ChessBoard(board.Pieces);
        //            tempBoard.ApplyDraw(tempDraw);

        //            // evaluate the scores and select the best ones
        //            var tempScores = getChessDrawScores(tempBoard, tempDraw, steps - 1);
        //            var tempMax = tempScores.Max(y => y.Item2);

        //            return new Tuple<Tuple<ChessDraw, double>, double>(score, tempMax);
        //        });

        //        scores = nextDrawScores.OrderByDescending(x => x.Item2).Select(x => x.Item1).ToList();
        //    }

        //    return scores;
        //}

        #endregion Methods
    }
}
