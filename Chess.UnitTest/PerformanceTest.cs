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

        [Fact(Skip = "test would take too long")]
        public void PlaymakingTest()
        {
            bool failed = false;

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    
                    output.WriteLine("starting game:");
                    output.WriteLine("==============");
                    var draws = playGame();
                    draws.ForEach(x => output.WriteLine(x.ToString()));
                    output.WriteLine("=========================================");
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
            bool isEndOfGame = false;

            //// print start situation
            //output.WriteLine("start of game");
            //output.WriteLine("");
            //output.WriteLine(game.Board.ToString());
            //output.WriteLine("");

            do
            {
                var alliedPieces = (game.SideToDraw == ChessColor.White ? game.Board.WhitePieces : game.Board.BlackPieces);

                // stop if one player one has his king
                if (alliedPieces.Count() == 1)
                {
                    gameStatus = CheckGameStatus.Stalemate;
                    break;
                }

                // get all possible draws
                var possibleDraws = alliedPieces.SelectMany(piece => new ChessDrawGenerator().GetDraws(game.Board, piece.Position, draw, true));
                
                // select one of the possible draws (random)
                int index = _random.Next(0, possibleDraws.Count());
                draw = possibleDraws.ElementAt(index);

                // check if the enemy king would get taken (-> checkmate) => GetCheckGameStatus() is not working correctly 
                if (game.Board.IsCapturedAt(draw.NewPosition) && game.Board.GetPieceAt(draw.NewPosition).Value.Type == ChessPieceType.King)
                {
                    gameStatus = CheckGameStatus.Checkmate;
                    break;
                }
                
                // apply the draw to the chess board
                game.ApplyDraw(draw);
                allDraws.Add(draw);
                
                //// print the name of the chess draw and the chess board after the draw was made
                //output.WriteLine($"{ draw.ToString() }");
                //output.WriteLine("");
                //output.WriteLine(game.Board.ToString());
                //output.WriteLine("");

                gameStatus = new ChessDrawSimulator().GetCheckGameStatus(game.Board, draw);
                isEndOfGame = game.Board.Pieces.Count() == 2 || gameStatus == CheckGameStatus.Checkmate || gameStatus == CheckGameStatus.Stalemate;
            }
            while (!isEndOfGame);

            // print result of game
            output.WriteLine($"game over. { (gameStatus == CheckGameStatus.Stalemate ? "tied" : $"{ game.SideToDraw.ToString().ToLower() } wins") }.");

            return allDraws;
        }

        #endregion Tests
    }
}
