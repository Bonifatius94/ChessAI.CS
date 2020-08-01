//using Chess.Lib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace Chess.DataTools.PGN
//{

//    public class StreamPgnParser : IPgnParser
//    {
//        #region Constants

//        private const string LITTLE_ROCHADE = "O-O";
//        private const string BIG_ROCHADE = "O-O-O";

//        private const string GAME_RESULT_WHITE_WIN = "1-0";
//        private const string GAME_RESULT_BLACK_WIN = "0-1";
//        private const string GAME_RESULT_TIE       = "1/2-1/2";

//        #endregion Constants

//        #region Methods

//        // TODO: implement reading from ascii / unicode streams
//        // TODO: check why this logic can parse less games than the former approach and is also not really faster

//        /// <summary>
//        /// Retrieve the chess games from the given PGN file.
//        /// </summary>
//        /// <param name="filePath">The PGN file to be parsed.</param>
//        /// <returns>a list of chess games</returns>
//        public IEnumerable<ChessGame> ParsePgnFile(string filePath)
//        {
//            var games = new List<ChessGame>();

//            // implement a more storage saving buffer algorithm
//            string content = string.Empty;
//            using (var reader = new StreamReader(filePath)) { content = reader.ReadToEnd(); }

//            // cache variables
//            var tempGame = new ChessGame();
//            string buffer = string.Empty;

//            // cache flags
//            bool ignore = false;
//            bool isGameValid = true;
//            bool isWhiteDraw = true;

//            // TODO: fix parser errors, so at least the games of the old parser logic are parsed correctly

//            // read every character once
//            for (int i = 0; i < content.Length; i++)
//            {
//                char c = content[i];

//                switch (c)
//                {
//                    // handle ignore mode for metadata and comment sections
//                    case '[': case '{': ignore = true; break;
//                    case ']': case '}': ignore = false; break;

//                    // cut new round markup (e.g. '1.e4' -> 'e4')
//                    case '.':

//                        // clear buffer and set new round flag (only if ignore mode is off)
//                        if (!ignore) { buffer = string.Empty; }
//                        break;

//                    // read buffer content
//                    case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8':  // field rows
//                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f': case 'g': case 'h':  // field columns
//                    case 'K': case 'Q': case 'R': case 'B': case 'N':                                // piece types
//                    case '0': case 'O': case '-': case '/':                                          // special characters

//                        // append char to buffer (only if ignore mode is off)
//                        if (!ignore) { buffer += c; }
//                        break;

//                    // evaluate buffer content
//                    case ' ': case '\n': case '\t':

//                        // continue if ignore mode is on or buffer is empty
//                        if (ignore || buffer?.Length == 0) { continue; }

//                        // declare temp variables
//                        ChessDraw tempDraw;
//                        ChessColor? winner;

//                        // check for end of game (and determine the winner)
//                        if (tryParseWinner(buffer, out winner))
//                        {
//                            // set winner and apply game to list (if the game is valid)
//                            if (isGameValid && tempGame.AllDraws.Count > 0)
//                            {
//                                tempGame.Winner = winner;
//                                games.Add(tempGame);
//                            }

//                            // start parsing the next game -> reset temp variables
//                            tempGame = new ChessGame();
//                            isWhiteDraw = true;
//                            isGameValid = true;
//                        }
//                        // parse chess draw (if previous draws are valid)
//                        else if (isGameValid && tryParsePgnDraw(buffer, tempGame, (isWhiteDraw ? ChessColor.White : ChessColor.Black), out tempDraw))
//                        {
//                            isGameValid = tempGame.ApplyDraw(tempDraw, true);
//                            isWhiteDraw = !isWhiteDraw;
//                        }
//                        // invalid section found -> flag game invalid
//                        else { isGameValid = false; }

//                        // reset buffer
//                        buffer = string.Empty;

//                        break;
//                }
//            }

//            return games;
//        }

//        private bool tryParsePgnDraw(string content, ChessGame game, ChessColor sideToDraw, out ChessDraw draw)
//        {
//            bool isValid;

//            // handle rochade draw
//            if (content.Equals(LITTLE_ROCHADE) || content.Equals(BIG_ROCHADE))
//            {
//                int row = (sideToDraw == ChessColor.White) ? 0 : 7;
//                int column = content.Equals(LITTLE_ROCHADE) ? 6 : 2;

//                draw = new ChessDraw(game.Board, new ChessPosition(row, 4), new ChessPosition(row, column));
//                isValid = true;
//            }
//            // handle metadata draw
//            else 
//            {
//                isValid = tryParseMetadataDraw(content, game, out draw);
//            }

//            return isValid;
//        }

//        /// <summary>
//        /// Parse a metadata draw from the given PGN content.
//        /// </summary>
//        /// <param name="game">The chess game with all previous draws.</param>
//        /// <param name="content">The PGN content to be parsed.</param>
//        /// <param name="draw">The parsed draw (result).</param>
//        /// <returns>Indicates whether the parsing was successful.</returns>
//        private bool tryParseMetadataDraw(string content, ChessGame game, out ChessDraw draw)
//        {
//            draw = new ChessDraw();
//            string original = content;

//            try
//            {
//                // TODO: implement this in a more comprehensive way

//                // parse drawing piece type
//                var type = (char.IsUpper(content[0])) ? parseType(content[0]) : ChessPieceType.Peasant;
//                content = char.IsUpper(content[0]) ? content.Substring(1, content.Length - 1) : content;

//                // parse promotion piece type
//                var promotionType = (char.IsUpper(content[content.Length - 1])) ? (ChessPieceType?)parseType(content[content.Length - 1]) : null;
//                content = char.IsUpper(content[content.Length - 1]) ? content.Substring(0, content.Length - 1) : content;

//                // parse row / column hints
//                int hintRow;
//                hintRow = (content.Length == 3 && int.TryParse(content.Substring(0, 1), out hintRow)) ? (hintRow - 1) : -1;
//                int hintColumn = (content.Length == 3 && content[0] >= 'a' && content[0] <= 'h') ? (content[0] - 'a') : -1;
//                content = (content.Length == 3) ? content.Substring(1, content.Length - 1) : content;

//                // make sure that the content has only 2 characters left
//                if (content.Length > 2 || !ChessPosition.AreCoordsValid(content)) { return false; }

//                // determine the old and new position of the drawing chess piece
//                var newPos = new ChessPosition(content);

//                // compute all possible allied draws
//                var alliedDraws = game.Board.GetPiecesOfColor(game.SideToDraw).Where(x => x.Piece.Type == type)
//                    .SelectMany(x => ChessDrawGenerator.Instance.GetDraws(game.Board, x.Position, game.LastDrawOrDefault)).ToList();

//                // find the draw instance in the list of all possible draws
//                draw = alliedDraws
//                    .First(draw =>
//                        draw.DrawingPieceType == type && draw.NewPosition == newPos && draw.PeasantPromotionPieceType == promotionType
//                        && (hintRow == -1 || draw.OldPosition.Row == hintRow) && (hintColumn == -1 || draw.OldPosition.Column == hintColumn));

//                // TODO: implement parser logic for en-passant
//            }
//            catch (Exception)
//            {
//                Console.WriteLine($"\nunable to parse \"{ original }\"\n{ game.Board.ToString() }\n");
//                return false;
//            }

//            return true;
//        }

//        private bool tryParseWinner(string content, out ChessColor? winner)
//        {
//            switch (content)
//            {
//                case GAME_RESULT_WHITE_WIN: winner = ChessColor.White; return true;
//                case GAME_RESULT_BLACK_WIN: winner = ChessColor.Black; return true;
//                case GAME_RESULT_TIE:       winner = null;             return true;
//                default:                    winner = null;             return false;
//            }
//        }

//        /// <summary>
//        /// Convert the given character into a chess piece type (K=King, Q=Queen, R=Rook, B=Bishop, N=Knight, P=Peasant).
//        /// </summary>
//        /// <param name="c">The character to convert.</param>
//        /// <returns>The parsed chess piece type</returns>
//        /// <exception cref="ArgumentException">Throws an argument exception if the given character does not belong to a chess piece type.</exception>
//        private ChessPieceType parseType(char c)
//        {
//            ChessPieceType type;

//            switch (c)
//            {
//                case 'K': type = ChessPieceType.King; break;
//                case 'Q': type = ChessPieceType.Queen; break;
//                case 'R': type = ChessPieceType.Rook; break;
//                case 'B': type = ChessPieceType.Bishop; break;
//                case 'N': type = ChessPieceType.Knight; break;
//                case 'P': type = ChessPieceType.Peasant; break;
//                default: throw new ArgumentException($"unknown chess piece type detected! { c }");
//            }

//            return type;
//        }

//        #endregion Methods
//    }
//}
