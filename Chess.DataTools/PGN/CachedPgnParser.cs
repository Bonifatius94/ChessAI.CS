/*
 * MIT License
 *
 * Copyright(c) 2020 Marco Tröster
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Chess.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Chess.DataTools.PGN
{
    // source: https://de.wikipedia.org/wiki/Schachnotation

    /// <summary>
    /// A parser class to extract chess games from PGN (portable game notation) log files.
    /// </summary>
    public class CachedPgnParser : IPgnParser
    {
        #region Constants

        private const string LITTLE_ROCHADE = "O-O";
        //private const string BIG_ROCHADE = "O-O-O";

        #endregion Constants

        #region Methods

        /// <summary>
        /// Retrieve the chess games from the given PGN file.
        /// </summary>
        /// <param name="filePath">The PGN file to be parsed.</param>
        /// <returns>a list of chess games</returns>
        public IEnumerable<ChessGame> ParsePgnFile(string filePath)
        {
            string content;

            // load text content from pgn file into RAM
            using (var reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
            }

            // extract the draws per game as raw text (game log)
            var logs = extractGameLogs(content);

            // parse raw text into chess game format
            var games = logs.AsParallel().Select(log => parseGameLog(log)).ToList();

            //Console.WriteLine($"Successfully parsed { games.Count() } games from file '{ filePath }'!");
            return games;
        }

        /// <summary>
        /// Divide the raw PGN text content into the content belonging to each game.
        /// </summary>
        /// <param name="content">The raw PGN text content from a PGN file.</param>
        /// <returns>a list of raw PGN game logs, each element's content belongs to another chess game</returns>
        private IEnumerable<string> extractGameLogs(string content)
        {
            // remove \r characters for multi-plattform compatibility
            content = content.Replace("\r", "");

            // extract the lines containing the game log data
            //var logs = content.Split("\n1.").Select(x => "1." + x).Select(x => x.Substring(0, x.IndexOf("\n"))).ToList();
            var logs = content.Split("\n1.").Select(x => "1." + x).Select(x => x.Replace("\n", " ")).Select(x => x.Replace("\r", ""))
                .Select(x => x.Contains('[') ? x.Substring(0, x.IndexOf('[')).Trim() : x)
                .ToList();
            logs.RemoveAt(0);

            return logs;
        }

        /// <summary>
        /// Retrieve the chess game data from a single PGN game log.
        /// </summary>
        /// <param name="log">The PGN game log to be parsed.</param>
        /// <returns>The chess game data to was parsed.</returns>
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

            // determine the winning side
            int gameSeriesScoreIndex = log.LastIndexOf(" ") + 1;
            string gameSeriesScore = log.Substring(gameSeriesScoreIndex, log.Length - gameSeriesScoreIndex);
            game.Winner = gameSeriesScore.Equals("1-0") ? ChessColor.White : (gameSeriesScore.Equals("0-1") ? (ChessColor?)ChessColor.Black : null);
            // TODO: extend parser logic for game scores other than '1-0', '0-1' and '1/2, 1/2'

            return game;
        }

        /// <summary>
        /// Retrieve a round (white draw and black answer).
        /// </summary>
        /// <param name="game">The chess game with all previous draws.</param>
        /// <param name="content">The PGN game log content to be parsed (content equals two draws).</param>
        /// <returns>The draws parsed.</returns>
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

        /// <summary>
        /// Retrieve a single chess draw from the given PGN content.
        /// </summary>
        /// <param name="game">The chess game with all previous draws.</param>
        /// <param name="content">The PGN content to be parsed.</param>
        /// <returns>The parsed chess draw or null (if there was a data format error).</returns>
        private ChessDraw? parseDraw(ChessGame game, string content)
        {
            // differ between rochade (little + big) and usual metadata draws.
            return content.Contains(LITTLE_ROCHADE) ? parseRochade(game, content) as ChessDraw? : parseMetadataDraw(game, content);
        }

        /// <summary>
        /// Parse a rochade draw from the given PGN content.
        /// </summary>
        /// <param name="game">The chess game with all previous draws.</param>
        /// <param name="content">The PGN content to be parsed.</param>
        /// <returns>The parsed rochade draw</returns>
        private ChessDraw? parseRochade(ChessGame game, string content)
        {
            int row = (game.SideToDraw == ChessColor.White) ? 0 : 7;
            int column = content.Equals(LITTLE_ROCHADE) ? 6 : 2;

            var oldPos = new ChessPosition(row, 4);
            var newPos = new ChessPosition(row, column);

            ChessDraw? draw = new ChessDraw(game.Board, oldPos, newPos);
            return draw;
        }

        /// <summary>
        /// Parse a metadata draw from the given PGN content.
        /// </summary>
        /// <param name="game">The chess game with all previous draws.</param>
        /// <param name="content">The PGN content to be parsed.</param>
        /// <returns>The parsed metadata draw</returns>
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

        /// <summary>
        /// Convert the given character into a chess piece type (K=King, Q=Queen, R=Rook, B=Bishop, N=Knight, P=Peasant).
        /// </summary>
        /// <param name="c">The character to convert.</param>
        /// <returns>The parsed chess piece type</returns>
        /// <exception cref="ArgumentException">Throws an argument exception if the given character does not belong to a chess piece type.</exception>
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
                default: throw new ArgumentException($"unknown chess piece type detected! { c }");
            }

            return type;
        }

        #endregion Methods
    }
}
