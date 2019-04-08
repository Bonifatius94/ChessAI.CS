using Chess.AI.PgnConv.TensorflowExport;
using System;
using System.IO;

namespace Chess.AI.PgnConv
{
    class Program
    {
        #region ExitCodes

        public enum ExitCodes
        {
            Ok = 0,
            NotEnoughArgs = -1,
            InputFileNotExisting = -2,
            FatalError = -3
        }
        
        #endregion ExitCodes

        #region Main

        public static void Main(string[] args)
        {
            // make sure that the amount of args is valid, otherwise abort
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments! You need to put input and output file path!");
                Environment.Exit((int)ExitCodes.NotEnoughArgs);
            }

            // parse args
            string inputFilePath = args[0];
            string outputFilePath = args[1];

            // make sure that the input file exists, otherwise abort
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("Invalid arguments! Input file does not exist!");
                Environment.ExitCode = (int)ExitCodes.InputFileNotExisting;
                return;
            }

            // execute data extraction from file
            new Program().Convert(inputFilePath, outputFilePath);
            Environment.ExitCode = (int)ExitCodes.Ok;
        }

        #endregion Main

        #region Methods

        public void Convert(string inputFilePath, string outputFilePath)
        {
            try
            {
                // load the chess games from the input file
                var games = new PgnParser().ParsePgnFile(inputFilePath);

                // write data as python code to output file
                new PgnNumpyExportHelper().ExportAsPythonCode(outputFilePath, games);
            }
            catch (Exception ex)
            {
                Console.WriteLine("program encountered a fatal error: \r\n" + ex.ToString());
                Environment.ExitCode = (int)ExitCodes.FatalError;
            }
        }

        #endregion Methods
    }
}
