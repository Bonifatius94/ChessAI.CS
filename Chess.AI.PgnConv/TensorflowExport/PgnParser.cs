using Chess.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Chess.AI.PgnConv.TensorflowExport
{
    // source: https://de.wikipedia.org/wiki/Schachnotation

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
            var games = logs./*AsParallel().*/Select(log => parseGameLog(log)).ToList();

            return games;
        }

        private IEnumerable<string> extractGameLogs(string content)
        {
            //int offset = 0;
            //var logs = new List<string>();

            //do
            //{
            //    // determine the start and end of the next log
            //    int logStart = content.IndexOf("\n1.", offset) + 1;
            //    int logEnd = content.IndexOf("\r\n", logStart);

            //    logEnd = logEnd > 0 ? logEnd : content.Length;
            //    offset = logEnd;

            //    try
            //    {
            //        // extract the log and apply it to the output list
            //        string s = content.Substring(logStart, logEnd);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"{ offset } / { content.Length }, { logStart } - { logEnd }");
            //    }
            //}
            //while (offset < content.Length);
            
            var logs = content.Split("\n1.").Select(x => "1." + x).ToList();
            logs.RemoveAt(0);

            return logs;
        }

        private ChessGame parseGameLog(string log)
        {
            var game = new ChessGame();

            int i = 0;
            int nextComment = log.IndexOf('{');

            while (i < log.Length)
            {
                int start = log.IndexOf('.', i) + 1;
                int end = log.IndexOf('.', start);

                // get text of the draw
                string drawText = ((end > 0) ? log.Substring(start, end) : log.Substring(start)).Trim();

                // ignore dots in comments
                if (end > nextComment)
                {
                    int commentEnd = log.IndexOf('}', nextComment);
                    drawText = log.Substring(start, nextComment).Trim() + log.Substring(commentEnd + 1, log.IndexOf('.', commentEnd));
                }

                // split draw text into parts separated by white spaces (first two parts contain the positions)
                var drawParts = 
                    drawText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x.Length >= 2).Select(x => x.Substring(x.Length - 2, 2)).ToArray();

                // parse white and black draw
                var whiteDraw = parseDraw(drawParts[0]);
                var blackDraw = parseDraw(drawParts[1]);
                
                // apply white and black draw to the chess board
                game.ApplyDraw(whiteDraw);
                game.ApplyDraw(blackDraw);
            }

            return game;
        }

        private ChessDraw parseDraw(ChessGame game, string content)
        {
            ChessPosition oldPos;
            ChessPosition newPos;

            var alliedDraws = game.GetDraws(true);

            if (content.Length == 2)
            {
                // only new position given, no metadata
                newPos = new ChessPosition(content);
                oldPos = alliedDraws.Where(x => x.NewPosition == newPos).First().OldPosition;
            }
            else
            {
                // evaluate a draw with metadata

                // evaluate first char
                if (char.IsUpper(content[0]))
                {
                    content = content.Substring(1, content.Length);

                }
            }
            
            var oldPos = new ChessPosition();

            // TODO: take care of peasant promotion type
            var draw = new ChessDraw(game.Board, oldPos, newPos);
        }

        private ChessPieceType parseType(string )
        {

        }

        #endregion Methods
    }
}
