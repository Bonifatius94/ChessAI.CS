using Chess.Lib;
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

            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    playGame(i + 1);
                }
                catch (Exception ex)
                {
                    failed = true;
                    output.WriteLine(ex.ToString());
                    break;
                }
            }

            Assert.True(!failed);
        }

        private void playGame(int id)
        {
            // init new game
            var game = new ChessGame();
            var gameStatus = CheckGameStatus.None;
            
            try
            {
                var draw = new ChessDraw();
                bool abort = false;

                do
                {
                    // get all possible draws
                    var alliedPieces = (game.SideToDraw == ChessColor.White) ? game.Board.WhitePieces : game.Board.BlackPieces;
                    var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(game.Board, piece.Position, draw, true));
                    
                    // select one of the possible draws (randomly)
                    int index = _random.Next(0, possibleDraws.Count());
                    draw = possibleDraws.ElementAt(index);

                    // apply the draw to the chess board
                    game.ApplyDraw(draw);

                    // check if the game is over (and exit in case)
                    gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);
                    abort = (gameStatus != CheckGameStatus.Check && gameStatus != CheckGameStatus.None);
                }
                while (!abort);
            }
            finally
            {
                // print draws and result of game
                output.WriteLine($"starting game { id }:");
                output.WriteLine("======================");
                game.AllDraws.ForEach(x => output.WriteLine(x.ToString()));
                output.WriteLine("======================");
                output.WriteLine(game.Board.ToString());
                output.WriteLine("======================");
                output.WriteLine($"game over. { (gameStatus == CheckGameStatus.Stalemate ? "tied" : $"{ game.SideToDraw.ToString().ToLower() } wins") }.");
                output.WriteLine("======================");
            }
        }

        #endregion Tests
    }
}
