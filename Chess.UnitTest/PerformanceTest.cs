﻿using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class PerformanceTest : TestBase
    {
        #region Constructor

        public PerformanceTest(ITestOutputHelper output) : base(output) { }

        #endregion Constructor

        #region Tests

        private static readonly Random _random = new Random();

        [Fact/*(Skip = "test would take too long")*/]
        public void PlaymakingTest()
        {
            bool failed = false;

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    playGame();
                }
                catch (Exception ex)
                {
                    failed = true;
                    output.WriteLine(ex.ToString());
                }
            }

            Assert.True(!failed);
        }

        private List<ChessDraw> playGame()
        {
            // init new game
            var game = new ChessGame();
            var draw = new ChessDraw();
            var allDraws = new List<ChessDraw>();
            var gameStatus = CheckGameStatus.None;
            bool abort = false;
            
            do
            {
                var alliedPieces = (game.SideToDraw == ChessColor.White ? game.Board.WhitePieces : game.Board.BlackPieces);

                // stop if a player only has his king left
                if (alliedPieces.Count() == 1)
                {
                    gameStatus = CheckGameStatus.Stalemate;
                    break;
                }

                // get all possible draws
                var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(game.Board, piece.Position, draw, true));
                
                // select one of the possible draws (randomly)
                int index = _random.Next(0, possibleDraws.Count());
                draw = possibleDraws.ElementAt(index);

                //// check if the enemy king would get taken (-> checkmate) => GetCheckGameStatus() is not working correctly 
                //if (game.Board.IsCapturedAt(draw.NewPosition) && game.Board.GetPieceAt(draw.NewPosition).Value.Type == ChessPieceType.King)
                //{
                //    gameStatus = CheckGameStatus.Checkmate;
                //    break;
                //}
                
                // apply the draw to the chess board
                game.ApplyDraw(draw);
                allDraws.Add(draw);
                
                gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);
                abort = gameStatus == CheckGameStatus.Checkmate || gameStatus == CheckGameStatus.Stalemate || gameStatus == CheckGameStatus.UnsufficientPieces;
            }
            while (!abort);

            // print draws and result of game
            output.WriteLine("starting game:");
            output.WriteLine("==============");
            allDraws.ForEach(x => output.WriteLine(x.ToString()));
            output.WriteLine($"game over. { (gameStatus == CheckGameStatus.Stalemate ? "tied" : $"{ game.SideToDraw.ToString().ToLower() } wins") }.");
            output.WriteLine("=========================================");

            return allDraws;
        }

        #endregion Tests
    }
}
