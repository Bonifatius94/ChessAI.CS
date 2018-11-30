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

    public interface IChessDrawHelper
    {
        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        ChessDraw GetNextDraw(ChessBoard board, ChessDraw precedingEnemyDraw, ChessDifficultyLevel level);
    }

    public class ChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the next chess draw according to the given difficulty level.
        /// </summary>
        /// <param name="board">the chess board representing the current game situation</param>
        /// <param name="precedingEnemyDraw">the opponent's last draw</param>
        /// <param name="level">the difficulty level</param>
        /// <returns>one possible chess draw</returns>
        public ChessDraw GetNextDraw(ChessBoard board, ChessDraw precedingEnemyDraw, ChessDifficultyLevel level)
        {
            // get all draws ordered by score and select the best one
            int steps = ((int)level) * 2;
            var bestDraw = getChessDrawScores(board, precedingEnemyDraw, steps).Select(x => x.Item1).First();

            // TODO: fix issue with drawing side in recursion case (steps > 0)
            
            return bestDraw;
        }

        private List<Tuple<ChessDraw, double>> getChessDrawScores(ChessBoard board, ChessDraw precedingEnemyDraw, int steps)
        {
            // init variables
            var lastDraw = precedingEnemyDraw;
            double scoreAtStart = new ChessScoreHelper().GetScore(board, lastDraw.DrawingSide);

            // get all possible chess draws
            var alliedPieces = (lastDraw.DrawingSide == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(board, piece.Position, lastDraw, true)).ToList();

            // get the score for each draw as (draw, score) tuple
            var scores = possibleDraws.Select(draw => {

                var tempBoard = new ChessBoard(board.Pieces);
                tempBoard.ApplyDraw(draw);
                double tempScore = new ChessScoreHelper().GetScore(tempBoard, lastDraw.DrawingSide);
                return new Tuple<ChessDraw, double>(draw, tempScore);

            // only retrieve draws that have a relatively positive impact on the player's score
            }).Where(x => x.Item2 >= scoreAtStart - 1).ToList();
            
            // go to the next level
            if (steps > 0)
            {
                // evaluate the chess draws by taking the next level in consideration
                var nextDrawScores = scores.Select(x => {

                    // simulate the draw
                    var tempDraw = x.Item1;
                    var tempBoard = new ChessBoard(board.Pieces);
                    tempBoard.ApplyDraw(tempDraw);

                    // evaluate the scores and select the best ones
                    var tempScores = getChessDrawScores(tempBoard, tempDraw, steps - 1);
                    var tempMax = tempScores.Max(y => y.Item2);

                    return new Tuple<Tuple<ChessDraw, double>, double>(x, tempMax);
                });
                
                scores = nextDrawScores.OrderByDescending(x => x.Item2).Select(x => x.Item1).ToList();
            }

            return scores;
        }

        #endregion Methods
    }
}
