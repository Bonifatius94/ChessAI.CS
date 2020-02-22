using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.Tools;
using Chess.Tools.SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Chess.AI.PgnWinPercentages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // task 1: parse games from pgn format and write them as mtcgn format
            string mtcgnFilePath = "all_games.mtcgn";
            pgnToMtcgn(Path.Combine("GameData", "pgn"), mtcgnFilePath);

            // task 2: compute win percentages of draws and write the to database
            mtcgnToWinPercentagesXml(mtcgnFilePath, "win_rates.db");
        }

        private static void pgnToMtcgn(string pgnsDirectory, string outputFilePath)
        {
            // get chess game data from pgn files
            int parsedFilesCount = 0;
            var pgnFilePaths = Directory.GetFiles(pgnsDirectory).Where(x => Path.GetExtension(x).ToUpper().Equals(".PGN")).ToList();

            var games = pgnFilePaths.AsParallel().SelectMany(pgnFilePath => {
                var games = new PgnParser().ParsePgnFile(pgnFilePath);
                Console.Write($"\rparsing pgn files: { ++parsedFilesCount } / { pgnFilePaths.Count }");
                return games;
            }).ToList();

            // write games to the custom chess game format
            new ChessGameFileSerializer().Serialize(outputFilePath, games);
        }

        private static void mtcgnToWinPercentagesXml(string mtcgnFilePath, string outputFilePath)
        {
            // parse games from custom chess game format
            var games = new ChessGameFileSerializer().Deserialize(mtcgnFilePath);

            // compute win percentages
            var winRates = new WinRateInfoSerializer().GamesToWinRates(games);

            // write win rates to sqlite
            var context = new WinRateDataContext(outputFilePath);
            context.InsertWinRates(winRates);
        }
    }
}
