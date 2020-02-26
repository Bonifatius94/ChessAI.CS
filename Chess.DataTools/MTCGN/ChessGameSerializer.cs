using Chess.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chess.DataTools
{
    /// <summary>
    /// Helps serializing chess games between the chess game format.
    /// </summary>
    public class ChessGameFileSerializer
    {
        #region Methods

        /// <summary>
        /// Write the given chess games to the given file path.
        /// </summary>
        /// <param name="filePath">The output chess game file path.</param>
        /// <param name="games">The games to write to serialize.</param>
        public void Serialize(string filePath, IEnumerable<ChessGame> games)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var game in games)
                {
                    // serialize draws from the game as a list of comma separated hex strings
                    string drawsContent = game.AllDraws.Select(x => x.GetHashCode().ToString("X4")).Aggregate((x, y) => x + ", " + y);
                    string winnerInfo = $"w={ ((game.Winner == ChessColor.White) ? "w" : (game.Winner == ChessColor.Black ? "b" : "t")) }";
                    string gameContent = $"{ drawsContent }, { winnerInfo }";
                    writer.WriteLine(gameContent);
                }
            }
        }

        /// <summary>
        /// Read games from chess games format.
        /// </summary>
        /// <param name="filePath">The file path of the chess games file.</param>
        /// <returns>a list of chess games</returns>
        public IEnumerable<ChessGame> Deserialize(string filePath)
        {
            var games = new List<ChessGame>();

            using (var reader = new StreamReader(filePath))
            {
                string buffer;

                while ((buffer = reader.ReadLine()) != null)
                {
                    var game = new ChessGame();

                    // parse winner
                    char winnerContent = buffer[buffer.IndexOf('w') + 2];
                    buffer = buffer.Substring(0, buffer.LastIndexOf(','));

                    switch (winnerContent)
                    {
                        case 'w': game.Winner = ChessColor.White; break;
                        case 'b': game.Winner = ChessColor.Black; break;
                        default:  game.Winner = null;             break;
                    }

                    // parse the draws and apply them to the game
                    var drawsOfGame = buffer.Trim().Split(',').Select(x => new ChessDraw(int.Parse(x.Trim(), System.Globalization.NumberStyles.HexNumber))).ToList();
                    drawsOfGame.ForEach(x => game.ApplyDraw(x));

                    games.Add(game);
                }
            }

            return games;
        }

        #endregion Methods
    }
}
