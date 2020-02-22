using System;
using System.IO;
using System.Linq;
using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.Tools;
using Chess.Tools.SQLite;
using Microsoft.Data.Sqlite;

namespace Chess.Tools.PgnImport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"{ new ChessDraw("18029A") }");
            //Console.WriteLine($"start formation board hash: { ChessBoard.StartFormation.ToHash() }");

            // supported conversions:
            //  - input: an explicit pgn file OR a directory with several pgn files
            //  - output: a file of the portable mtcgn format OR a sqlite database file

            //// load chess games from file
            //var start = DateTime.Now;
            //var games = new ChessGameFileSerializer().Deserialize("all_games.mtcgn");
            //Console.WriteLine($"Loaded { games.Count() } chess games, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            //GC.Collect();

            //// compute win rates of draws in games
            //start = DateTime.Now;
            //var winRates = new WinRateInfoSerializer().GamesToWinRates(games);
            //Console.WriteLine($"Loaded { games.Count() } win rates, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            //GC.Collect();

            //// create new cache database with loaded games
            //start = DateTime.Now;
            var cache = new WinRateDataContext("win_rates.db");
            //cache.InsertWinRates(winRates);
            //Console.WriteLine($"Successfully created a sqlite database with all win rates, took { (int)(DateTime.Now - start).TotalMinutes }m { (int)(DateTime.Now - start).TotalSeconds }s");
            //GC.Collect();

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
