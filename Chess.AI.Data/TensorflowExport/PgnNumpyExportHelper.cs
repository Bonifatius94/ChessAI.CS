using Chess.Lib;
using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chess.AI.Data.TensorflowExport
{
    public class PgnNumpyExportHelper
    {
        #region Methods

        public void ExportAsPythonCode(string filePath, IList<ChessGame> data)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // declare empty array
                writer.WriteLine("chessdata = []");

                // write all games
                foreach (var game in data)
                {
                    // extract data lines from the game
                    var lines = getDataLines(game);

                    // write all data lines to the output file
                    lines.ForEach(line => writer.WriteLine(line));
                }
            }
        }

        private List<string> getDataLines(ChessGame game)
        {
            // create a chess board in start formation, representing the game situation
            var board = ChessBoard.StartFormation;

            // loop through all chess draws
            foreach (var draw in game.AllDraws)
            {
                // apply the draw to the chess board
                board.ApplyDraw(draw);

                // get the hash string of the chess board
                string hash = board.ToHash();

                // calculate the score of the draw
                new MinimaxChessDrawAI()
            }
        }

        //public void ExportGamesAsNumpy(string filePath, IList<ChessGame> data)
        //{
        //    using (var output = File.Create(filePath))
        //    {
        //        using (var writer = new BinaryWriter(output, Encoding.ASCII))
        //        {
        //            // write magic string
        //            writer.Write("\x93NUMPY");

        //            // write numpy version
        //            writer.Write("\x01\x00");

        //            // write dictionary header
        //            writer.Write("{" + $"'descr': '<f8', 'fortran_order': True, 'shape': ({ data.Count }, 3), " + "}");

        //            //len(magic string) + 2 + len(length) + HEADER_LEN
        //            // TODO: write header length

        //            // write each game to the 
        //            foreach (var game in data)
        //            {
        //                writeGame(writer, game);
        //            }
        //        }
        //    }
        //}

        //private void writeGame(BinaryWriter writer, ChessGame game)
        //{

        //}

        #endregion Methods
    }
}
