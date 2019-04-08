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
            var games = logs.AsParallel().Select(log => parseGameLog(log)).ToList();

            return games;
        }

        private IEnumerable<string> extractGameLogs(string content)
        {
            // remove \r characters for multi-plattform compatibility
            content = content.Replace("\r", "");

            // extract the lines containing the game log data
            var logs = content.Split("\n1.").Select(x => "1." + x).Select(x => x.Substring(0, x.IndexOf("\n"))).ToList();
            logs.RemoveAt(0);

            return logs;
        }

        private ChessGame parseGameLog(string log)
        {
            var game = new ChessGame();
            log = log.Trim();

            int i = 0;
            int nextComment = log.IndexOf('{');

            // parse all draws of the game log
            while (i < log.Length)
            {
                // determine the start and end of the draw data to be parsed
                int start = log.IndexOf('.', i) + 1;
                int end = (log.IndexOf('.', start) > 0) ? (log.IndexOf('.', start) - 1) : (log.Length);

                // get text of the draw data (usually 1 or 2 draws)
                string drawText = ((end > 0) ? log.Substring(start, end - start) : log.Substring(start)).Trim();

                // ignore comments
                if (nextComment > 0 && end > nextComment)
                {
                    int commentEnd = log.IndexOf('}', nextComment);
                    end = (log.IndexOf('.', commentEnd) > 0) ? (log.IndexOf('.', commentEnd) - 1) : (log.Length);
                    drawText = log.Substring(start, nextComment - start).Trim() + log.Substring(commentEnd + 1, end - commentEnd - 1);
                    nextComment = log.IndexOf('{', commentEnd);
                }

                // parse draws of this round (each white and black side)
                var draws = parseRound(game, drawText);
                
                // apply white and black draw to the chess board
                foreach (var draw in draws) { game.ApplyDraw(draw); }

                // stop if there was a parsing error / no draw content is left to be parsed
                if (draws?.Count() == 0) { break; }

                // update index
                i = end;
            }

            return game;
        }

        private IEnumerable<ChessDraw> parseRound(ChessGame game, string content)
        {
            var gameCopy = game.Clone() as ChessGame;
            var draws = new List<ChessDraw>();

            // split draw text into parts separated by white spaces (first two parts contain the positions)
            var drawParts = content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (drawParts.Length >= 1)
            {
                // parse white player's draw
                ChessDraw? whiteDraw = parseDraw(gameCopy, drawParts[0]);

                if (whiteDraw != null && drawParts.Length >= 2)
                {
                    // apply white player's draw to the chess game copy
                    gameCopy.ApplyDraw(whiteDraw.Value);
                    draws.Add(whiteDraw.Value);

                    // parse black player's draw
                    ChessDraw? blackDraw = parseDraw(gameCopy, drawParts[1]);
                    if (blackDraw != null) { draws.Add(blackDraw.Value); }
                }
            }

            return draws;
        }

        private const string LITTLE_ROCHADE = "O-O";
        //private const string BIG_ROCHADE = "O-O-O";

        private ChessDraw? parseDraw(ChessGame game, string content)
        {
            return content.Contains(LITTLE_ROCHADE) ? parseRochade(game, content) as ChessDraw? : parseMetadataDraw(game, content);
        }

        private ChessDraw? parseRochade(ChessGame game, string content)
        {
            int row = (game.SideToDraw == ChessColor.White) ? 0 : 7;
            int column = content.Equals(LITTLE_ROCHADE) ? 6 : 2;

            var oldPos = new ChessPosition(row, 4);
            var newPos = new ChessPosition(row, column);

            ChessDraw? draw = new ChessDraw(game.Board, oldPos, newPos);
            return draw;
        }

        private ChessDraw? parseMetadataDraw(ChessGame game, string content)
        {
            ChessDraw? draw = null;
            string original = content;

            try
            {
                // remove not significant markups ('x', '+', '#', '=')
                content = content.Replace("x", "");
                content = content.Replace("+", "");
                content = content.Replace("#", "");
                content = content.Replace("=", "");

                // parse drawing piece type
                ChessPieceType type = (char.IsUpper(content[0])) ? parseType(content[0]) : ChessPieceType.Peasant;
                content = char.IsUpper(content[0]) ? content.Substring(1, content.Length - 1) : content;

                // parse promotion piece type
                ChessPieceType? promotionType = (char.IsUpper(content[content.Length - 1])) ? (ChessPieceType?)parseType(content[content.Length - 1]) : null;
                content = char.IsUpper(content[content.Length - 1]) ? content.Substring(0, content.Length - 1) : content;

                // parse row / column hints
                int hintRow;
                hintRow = (content.Length == 3 && int.TryParse(content.Substring(0, 1), out hintRow)) ? (hintRow - 1) : -1;
                int hintColumn = (content.Length == 3 && content[0] >= 'a' && content[0] <= 'h') ? (content[0] - 'a') : -1;
                content = (content.Length == 3) ? content.Substring(1, content.Length - 1) : content;

                // make sure that the content has only 2 characters left
                if (content.Length > 2 || !ChessPosition.AreCoordsValid(content)) { return null; }

                // determine the old and new position of the drawing chess piece
                var newPos = new ChessPosition(content);

                // compute all possible allied draws
                var alliedDraws = game.GetDraws(true);

                // find the draw instance in the list of all possible draws
                draw = alliedDraws
                    .Where(x =>
                        x.DrawingPieceType == type && x.NewPosition == newPos && x.PeasantPromotionPieceType == promotionType
                        && (hintRow == -1 || x.OldPosition.Row == hintRow) && (hintColumn == -1 || x.OldPosition.Column == hintColumn)
                    ).First();

                // TODO: implement parser logic for en-passant
            }
            catch (Exception)
            {
                Console.WriteLine($"unable to parse \"{ original }\"\n{ game.Board.ToString() }\n");
            }

            return draw;
        }

        private ChessPieceType parseType(char c)
        {
            ChessPieceType type;

            switch (c)
            {
                case 'K': type = ChessPieceType.King;    break;
                case 'Q': type = ChessPieceType.Queen;   break;
                case 'R': type = ChessPieceType.Rook;    break;
                case 'B': type = ChessPieceType.Bishop;  break;
                case 'N': type = ChessPieceType.Knight;  break;
                case 'P': type = ChessPieceType.Peasant; break;
                default: throw new NotImplementedException($"unknown chess piece type detected! { c }");
            }

            return type;
        }

        #endregion Methods
    }
}
