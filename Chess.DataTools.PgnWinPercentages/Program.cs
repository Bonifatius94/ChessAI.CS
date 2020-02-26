using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.DataTools;
using Chess.DataTools.SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Chess.DataTools.PGN;

namespace Chess.DataTools.PgnWinPercentages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO: implement program args instead of hard coded file paths

            // task 1: parse games from pgn format and write them as mtcgn format
            string mtcgnFilePath = "all_games.mtcgn";
            pgnToMtcgn(Path.Combine("GameData", "pgn"), mtcgnFilePath);

            // task 2: compute win percentages of draws and write the to database
            mtcgnToWinPercentagesXml(mtcgnFilePath, "win_rates.db");
        }

        private static void pgnToMtcgn(string pgnsDirectory, string outputFilePath)
        {
            Console.WriteLine($"converting pgn files to mtcgn format");

            // get chess game data from pgn files
            int parsedFilesCount = 0;
            int parsedGamesCount = 0;
            var pgnFilePaths = Directory.GetFiles(pgnsDirectory).Where(x => Path.GetExtension(x).ToUpper().Equals(".PGN")).ToList();

#if DEBUG
            var games = pgnFilePaths./*AsParallel().*/SelectMany(pgnFilePath => {
#else
            var games = pgnFilePaths.AsParallel().SelectMany(pgnFilePath => {
#endif
                var games = new StreamPgnParser().ParsePgnFile(pgnFilePath);
                Console.Write($"\rparsing pgn files: { ++parsedFilesCount } / { pgnFilePaths.Count }, total games: { parsedGamesCount += games.Count() }");
                return games;
            }).ToList();

            // write games to the custom chess game format
            new ChessGameFileSerializer().Serialize(outputFilePath, games);
        }

        private static void mtcgnToWinPercentagesXml(string mtcgnFilePath, string outputFilePath)
        {
            // board and draw hashes for test purposes
            Console.WriteLine($"start formation board hash: { ChessBoard.StartFormation.ToHash() }");
            Console.WriteLine($"draw hash of 'white peasant E2-E4': 0x18031C or { new ChessDraw("18031C").GetHashCode() }");
            
            // supported conversions:
            //  - input: an explicit pgn file OR a directory with several pgn files
            //  - output: a file of the portable mtcgn format OR a sqlite database file

            // load chess games from file
            var start = DateTime.Now;
            var games = new ChessGameFileSerializer().Deserialize(mtcgnFilePath);
            Console.WriteLine($"Loaded { games.Count() } chess games, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            GC.Collect();

            // compute win rates of draws in games
            Console.WriteLine("Start the computation of win rates.");
            Console.WriteLine("WARNING: The next step will use several GB of memory!");
            start = DateTime.Now;
            var winRates = new WinRateInfoHelper().GamesToWinRates(games);
            Console.WriteLine($"Loaded { games.Count() } win rates, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            GC.Collect();

            // create new cache database with loaded games
            start = DateTime.Now;
            var cache = new WinRateDataContext("win_rates.db");
            cache.InsertWinRates(winRates);
            Console.WriteLine($"Successfully created a sqlite database with all win rates, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            GC.Collect();

            // test if a draw can be retrieved for start formation
            var draw = cache.GetBestDraw(ChessBoard.StartFormation, null);
            Console.WriteLine($"Cache database advised to draw '{ draw }' from start formation.");

            // test sql commands
            // ========================================
            // -- get the best draw for start formation
            // SELECT DrawHash, WinRate FROM WinRateInfo
            // WHERE DrawingSide = 'w' AND BoardBeforeHash = '19482090A3318C6318C60000000000000000000000000000000000000000B5AD6B5AD69D6928D2B3'
        }
    }
}
