using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.DataTools.PGN
{
    /// <summary>
    /// An interface implementing a parser for the PGN chess game notation.
    /// </summary>
    public interface IPgnParser
    {
        /// <summary>
        /// Retrieve the chess games from the given PGN file.
        /// </summary>
        /// <param name="filePath">The PGN file to be parsed.</param>
        /// <returns>a list of chess games</returns>
        IEnumerable<ChessGame> ParsePgnFile(string filePath);
    }
}
