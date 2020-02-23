using Chess.Lib;
using Chess.Lib.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tools.SQLite
{
    /// <summary>
    /// REpresents a SQLite data conext for writing / retrieving win rates of chess draws.
    /// </summary>
    public class WinRateDataContext : SqliteDataContextBase
    {
        #region Constructor

        /// <summary>
        /// Create a new instance that handles the SQLite database at the given file path. An empty database is created if the file does not exist.
        /// </summary>
        /// <param name="dbFilePath">The file path of the SQLite database.</param>
        public WinRateDataContext(string dbFilePath) : base(dbFilePath)
        {
            // init data schema if required
            if (!File.Exists(dbFilePath)) { executeScript(Path.Combine("SQLite", "scripts", "create_schema_v1.sql")); }
        }

        #endregion Constructor

        #region Methods

        #region WinRates

        /// <summary>
        /// Insert the given win rates into database.
        /// </summary>
        /// <param name="winRates">A list of win rates to be inserted into database.</param>
        public void InsertWinRates(IEnumerable<WinRateInfo> winRates)
        {
            var US_CULTURE = CultureInfo.CreateSpecificCulture("en-US");

            using (var connection = createConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var winRatesToInsert = winRates.Where(x => x.WinRate > 0).ToList();

                    var insertValues = winRatesToInsert.Select(x =>
                        $"'{ x.Draw.GetHashCode().ToString("X4") }', " +
                        $"{ x.Draw.GetHashCode() }, " +
                        $"'{ x.BoardHash }', " +
                        $"'{ char.ToLower(x.Draw.DrawingSide.ToChar()) }', " +
                        $"{ x.WinRate.ToString(US_CULTURE) }, " +
                        $"{ x.AnalyzedGames }"
                    ).ToList();

                    foreach (var insert in insertValues)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $"INSERT INTO WinRateInfo (DrawHash, DrawHashNumeric, BoardBeforeHash, DrawingSide, WinRate, AnalyzedGames) VALUES ({ insert });";
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Retrieve the best chess draw for the given situation (board + previous draw) from the cache. If the situation is not in the cache, null is returned.
        /// </summary>
        /// <param name="board">The chess board representing the situation.</param>
        /// <param name="predecedingDraw">The previous draw made by the enemy.</param>
        /// <returns>The best chess draw (if there is one in the cache)</returns>
        public ChessDraw? GetBestDraw(ChessBoard board, ChessDraw? predecedingDraw)
        {
            // get possible draws from database (with win rate)
            var drawingSide = predecedingDraw?.DrawingSide.Opponent() ?? ChessColor.White;

            string sql =
                  $"WITH Situation AS( "
                + $"    SELECT "
                + $"        DrawHash, "
                + $"        WinRate, "
                + $"        AnalyzedGames, "
                + $"        WinRate * AnalyzedGames AS Score "
                + $"    FROM WinRateInfo "
                + $"    WHERE DrawingSide = '{ char.ToLower(drawingSide.ToChar()) }' AND BoardBeforeHash = '{ board.ToHash() }' "
                + $") "

                + $"SELECT "
                + $"    DrawHash, "
                + $"    Score "
                + $"FROM Situation "
                + $"WHERE Score = (SELECT MAX(Score) FROM Situation)";

            var bestDraws = queryItems(sql).Select(x => new Tuple<ChessDraw, double>(new ChessDraw(int.Parse(x["DrawHash"] as string, NumberStyles.HexNumber)), (double)x["Score"])).ToList();
            return bestDraws?.Count > 0 ? (ChessDraw?)bestDraws.ChooseRandom().Item1 : null;

            // query for testing
            // ====================================
            // WITH Situation AS(
            //     SELECT
            //         DrawHashMetadata.*,
            //         WinRate,
            //         AnalyzedGames
            //     FROM WinRateInfo
            //     INNER JOIN DrawHashMetadata ON WinRateInfo.DrawHash = DrawHashMetadata.DrawHash
            //     WHERE WinRateInfo.DrawingSide = 'w' AND BoardBeforeHash = '19482090A3318C6318C60000000000000000000000000000000000000000B5AD6B5AD69D6928D2B3'
            // )
            // 
            // SELECT
            //     *,
            //     WinRate* AnalyzedGames AS Score
            // FROM Situation
            // ORDER BY Score DESC
        }

        #endregion WinRates

        #endregion Methods
    }
}
