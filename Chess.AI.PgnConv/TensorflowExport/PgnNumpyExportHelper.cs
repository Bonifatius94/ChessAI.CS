using Chess.Lib;
using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Chess.AI.PgnConv.TensorflowExport
{
    /// <summary>
    /// Helper functionality for export of chess draws into python array data.
    /// </summary>
    public class PgnNumpyExportHelper
    {
        #region Methods

        /// <summary>
        /// Export the chess draws of the given games to the given file as python code.
        /// </summary>
        /// <param name="filePath">The output file path</param>
        /// <param name="games">A list of chess games containing the chess draws to be exported</param>
        public void ExportAsPythonCode(string filePath, IEnumerable<ChessGame> games)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // declare empty array
                writer.WriteLine("chessdata = []");
                writer.WriteLine();

                // write game data parallel
                games.AsParallel().ToList().ForEach(game => {

                    // extract data lines from the game and write it to the output file
                    string lines = getDataLinesFromGame(game);
                    writer.WriteLine(lines);
                });
            }
        }

        private string getDataLinesFromGame(ChessGame game)
        {
            // create a chess board in start formation, representing the game situation
            var board = ChessBoard.StartFormation;

            // create a string builder to buffer data lines
            var builder = new StringBuilder();

            // loop through all chess draws
            foreach (var draw in game.AllDraws)
            {
                // get the chess board data
                var boardAsIntArray = convertBytesToInt32Array(board.ToBitboard().BinaryData);
                var boardData = boardAsIntArray.Select(x => formatDouble(x)).Aggregate((x, y) => $"{ x }, { y }");

                // calculate the score of the draw
                double score = new MinimaxChessDrawAI().RateDraw(board, draw, ChessDifficultyLevel.Medium);

                // transform data into a data line
                string dataLine = $"chessdata.append([{ formatDouble(draw.GetHashCode()) }, { boardData }, { formatDouble(score) }])";
                builder.AppendLine(dataLine);

                // apply the draw to the chess board
                board.ApplyDraw(draw);
            }

            return builder.ToString();
        }

        #region Helpers

        private int[] convertBytesToInt32Array(byte[] bytes)
        {
            // create output array
            int length = bytes.Length / 4 + ((bytes.Length % 4 > 0) ? 1 : 0);
            int[] data = new int[length];

            // loop through input byte array (4 byte steps)
            for (int i = 0; i < length; i++)
            {
                // set all bits to zero
                uint temp = 0;

                // apply the bits from the byte array
                for (int j = 0; j < 4; j++)
                {
                    temp = temp << 8;
                    temp = (i * 4 + j < bytes.Length) ? temp | bytes[i * 4 + j] : temp;
                }

                // apply the 4 bytes (as int32) to the output array
                data[i] = (int)temp;
            }

            return data;
        }

        private static readonly CultureInfo CULTURE_EN_US = CultureInfo.CreateSpecificCulture("en-US");

        private string formatDouble(double value)
        {
            return value.ToString("f2", CULTURE_EN_US);
        }

        #endregion Helpers

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
