using Chess.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Chess.AI.Data.TensorflowExport
{
    public class PgnParser
    {
        #region Methods

        public IList<ChessGame> ParsePgnFile(string filePath)
        {
            string content;

            // load content from pgn file
            using (var reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
            }

            // extract the draws per game as raw text (game log)
            var logs = extractGameLogs(content);

            // parse raw text into chess game format
            var games = logs.Select(log => parseGameLog(log)).ToList();

            return games;
        }

        private List<string> extractGameLogs(string content)
        {
            // split content into lines and leave of empty lines
            var lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // remove empty / metadata lines
            var logLines = lines.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x) || !x.StartsWith("["));

            // put game log lines together to retrieve the raw game data
            var rawGameData = logLines.Aggregate((x, y) => x + y);

            // retrieve game logs
            var logs = rawGameData.Split("1.", StringSplitOptions.RemoveEmptyEntries).Select(x => "1." + x).ToList();
            
            return logs;
        }

        private ChessGame parseGameLog(string log)
        {
            var game = new ChessGame();

            int i = 0;
            //int nextComment = log.IndexOf('{');
            
            while (i < log.Length)
            {
                int start = log.IndexOf('.', i) + 1;
                int end = log.IndexOf('.', start);

                // get text of the draw
                string drawText = ((end > 0) ? log.Substring(start, end) : log.Substring(start)).Trim();

                //// ignore dots in comments
                //if (end > nextComment)
                //{
                //    int commentEnd = log.IndexOf('}', nextComment);
                //    drawText = log.Substring(start, nextComment).Trim() + log.Substring(commentEnd + 1, log.IndexOf('.', commentEnd));
                //}

                // split draw text into parts separated by white spaces (first two parts contain the positions)
                var drawParts = 
                    drawText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x.Length >= 2).Select(x => x.Substring(x.Length - 2, 2)).ToArray();
                
                var oldPos = new ChessPosition(drawParts[0]);
                var newPos = new ChessPosition(drawParts[1]);

                // TODO: take care of peasant promotion type
                var draw = new ChessDraw(game.Board, oldPos, newPos);
                game.ApplyDraw(draw);
            }

            return game;
        }

        #endregion Methods
    }
}
