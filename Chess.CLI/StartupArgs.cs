﻿/*
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

using System;
using System.Linq;

namespace Chess.CLI
{
    /// <summary>
    /// The mode of a chess game.
    /// </summary>
    public enum ChessGameMode
    {
        /// <summary>
        /// Game mode player vs. player.
        /// </summary>
        PvP,

        /// <summary>
        /// Game mode player vs. computer.
        /// </summary>
        PvC,

        /// <summary>
        /// Game mode computer vs. computer.
        /// </summary>
        CvC
    }

    /// <summary>
    /// Representing startup arguments of the chess CLI program.
    /// </summary>
    public class StartupArgs
    {
        #region Constants

        #region Help

        private const string ARG_HELP = "--help";
        private const string ARG_HELP_WIN = "/?";

        private const string PROGRAM_STARTUP_MESSAGE =
              "Chess Game Client v1.0" +
            "\nMarco Tröster © 2020" +
            "\n========================" +
            "\n";

        private const string HELP_MESSAGE =
              "USAGE" +
            "\ndotnet Chess.CLI.dll [--mode=<mode>] [--level=<level>]" +
            "\ndotnet Chess.CLI.dll --mode=pvc --level=5" +
            "\n" +
            "\nmode: the game mode (either 'person vs person', 'person vs computer' or 'computer vs computer' (default: pvc)" +
            "\nlevel: the intelligence level of the computer (if a computer is involved), a higher level lets the computer look further in the future and therefore make better decisions (default: 5)";

        #endregion Help

        #region Args

        private const string ARG_GAME_MODE = "--mode=";
        private const string ARG_COMPUTER_LEVEL = "--level=";

        #endregion Args

        #endregion Constants

        #region Members

        /// <summary>
        /// Indicates whether the help message was printed.
        /// </summary>
        public bool IsHelp { get; set; }

        /// <summary>
        /// The game mode of the given startup arguments.
        /// </summary>
        public ChessGameMode GameMode { get; set; }

        /// <summary>
        /// The computer of the given startup arguments.
        /// </summary>
        public int ComputerLevel { get; set; }

        #endregion Members

        #region Methods

        /// <summary>
        /// Initialize the parsing of the startup arguments. Prints a help message if the help option was chosen or wrong arguments were passed.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <returns>this startup arguments instance</returns>
        public StartupArgs Init(string[] args)
        {
            // print startup message
            Console.WriteLine(PROGRAM_STARTUP_MESSAGE);

            // handle help flags
            IsHelp = args.Contains(ARG_HELP) || args.Contains(ARG_HELP_WIN);
            
            // parse args
            GameMode = parseGameMode(args);
            ComputerLevel = parseComputerLevel(args);

            // write help text
            if (IsHelp) { Console.WriteLine(HELP_MESSAGE); }

            return this;
        }

        private ChessGameMode parseGameMode(string[] args)
        {
            ChessGameMode mode = ChessGameMode.PvC;
            string arg = args.FirstOrDefault(x => x.ToLower().StartsWith(ARG_GAME_MODE));

            if (!string.IsNullOrEmpty(arg))
            {
                string argValue = arg.Substring(ARG_GAME_MODE.Length, arg.Length - ARG_GAME_MODE.Length);

                switch (argValue.ToLower())
                {
                    case "pvp": mode = ChessGameMode.PvP; break;
                    case "pvc": mode = ChessGameMode.PvC; break;
                    case "cvc": mode = ChessGameMode.CvC; break;
                    default: 
                        Console.WriteLine($"Invalid args! Unknown game mode '{ argValue }'! Please use the '--help' option for more details!");
                        Console.WriteLine();
                        IsHelp = true;
                        break;
                }
            }

            return mode;
        }

        private int parseComputerLevel(string[] args)
        {
            int level = 5;
            string arg = args.FirstOrDefault(x => x.ToLower().StartsWith(ARG_COMPUTER_LEVEL));

            if (!string.IsNullOrEmpty(arg))
            {
                string argValue = arg.Substring(ARG_COMPUTER_LEVEL.Length, arg.Length - ARG_COMPUTER_LEVEL.Length);

                if (!int.TryParse(argValue, out level))
                {
                    Console.WriteLine($"Invalid args! Unknown computer level '{ argValue }'! Please use the '--help' option for more details!");
                    Console.WriteLine();
                    IsHelp = true;
                }
            }

            return level;
        }

        #endregion Methods
    }
}
