using Chess.Lib;
using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Chess.DataTools
{
    /// <summary>
    /// A data object class providing the win rate of a given (board, draw) tuple including the total amount of games measured.
    /// </summary>
    public class WinRateInfo
    {
        #region Members

        /// <summary>
        /// The compact representation of a chess board as hex-string. (it's not really a hash, as no information is lost by 'hashing')
        /// </summary>
        public string BoardHash { get; set; }

        /// <summary>
        /// The chess draw to be applied to the given board.
        /// </summary>
        public ChessDraw Draw { get; set; }

        /// <summary>
        /// The win rate of the (board, draw) tuple as percentage.
        /// </summary>
        public double WinRate { get; set; }

        /// <summary>
        /// The total count of games, containing the (board, draw) tuple. Can be used to measure how significant the win rate is.
        /// </summary>
        public int AnalyzedGames { get; set; }

        /// <summary>
        /// A chess board property for converting between hex-string and the ChessBoard format (computed operation, data is stored as hex-string).
        /// </summary>
        public IChessBoard Board
        {
            get { return BoardHash.HashToBoard(); }
            set { BoardHash = value.ToHash(); }
        }

        #endregion Members
    }

    /// <summary>
    /// A helper class providing functionality for computing win rates related to (board, draw) tuples using a set of chess games.
    /// </summary>
    public class WinRateInfoHelper
    {
        #region Methods

        /// <summary>
        /// <para>Compute the win rate of each (board, draw) tuple onto the given chess games by grouping those tuples for multiple games.</para>
        /// WARNING: This function might use lots of RAM to speed up the computation!!! (about 11 GB for evaluating 360000 games)
        /// </summary>
        /// <param name="games">The chess games to be evaluated.</param>
        /// <returns>a list of win rates for each (board, draw) tuple onto the given chess games</returns>
        public IEnumerable<WinRateInfo> GamesToWinRates(IEnumerable<ChessGame> games)
        {
            // TODO: use Trill API here ... instead of Linq ...

            var drawsCache = games.Where(game => game.Winner != null).AsParallel().SelectMany(game => {
                
                var drawsXWinner = new List<Tuple<Tuple<string, ChessDraw>, ChessColor>>();

                var winningSide = game.Winner.Value;
                var tempGame = new ChessGame();

                foreach (var draw in game.AllDraws)
                {
                    var board = (IChessBoard)((ICloneable)tempGame.Board).Clone();
                    drawsXWinner.Add(new Tuple<Tuple<string, ChessDraw>, ChessColor>(new Tuple<string, ChessDraw>(board.ToHash(), draw), winningSide));
                    tempGame.ApplyDraw(draw);
                }

                return drawsXWinner;
            }).ToList();

            var winRates = drawsCache.GroupBy(x => x.Item1).Where(x => x.Count() >= 5).AsParallel().Select(group => {

                int drawingSideWins = group.Count(x => x.Item2 == group.Key.Item2.DrawingSide);
                int totalGames = group.Count();

                double winRate = (double)drawingSideWins / totalGames;
                return new WinRateInfo() { Draw = group.Key.Item2, BoardHash = group.Key.Item1, WinRate = winRate, AnalyzedGames = totalGames };
            })
            .ToList();

            return winRates;
        }

        #endregion Methods
    }
}
