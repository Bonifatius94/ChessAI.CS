using Chess.Lib;
using Chess.Lib.Extensions;
using Chess.Tools;
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
            //// task 1: parse games from pgn format and 
            //pgnToMtcgn("GameData", "all_games.mtcgn");

            // task 2: compute win percentages of draws
            mtcgnToWinPercentagesXml(Path.Combine("GameData", "all_games.mtcgn"), "win_percentages.xml");

            //Console.WriteLine(ChessBoard.StartFormation.ToHash());
        }

        //private static void pgnToMtcgn(string pgnsDirectory, string outputFilePath)
        //{
        //    // get chess game data
        //    //var games = new PgnParser().ParsePgnFile("GameData\\ficsgamesdb_search_56516.pgn");
        //    var games = Directory.GetFiles(pgnsDirectory).SelectMany(pgnFilePath => new PgnParser().ParsePgnFile(pgnFilePath));

        //    // write games to the custom chess game format
        //    new ChessGameFileSerializer().Serialize(outputFilePath, games);
        //}

        private static void mtcgnToWinPercentagesXml(string mtcgnFilePath, string outputFilePath)
        {
            // parse games from custom chess game format
            var games = new ChessGameFileSerializer().Deserialize(mtcgnFilePath);

            // compute win percentages and write them to a xml file
            new WinRateInfoSerializer().Serialize(outputFilePath, games);
        }
    }
}
