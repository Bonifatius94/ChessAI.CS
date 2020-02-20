using Chess.Lib;
using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Chess.Tools
{
    //public readonly struct DrawSituation
    //{
    //    #region Constructor

    //    public DrawSituation(ChessBoard board, ChessDraw draw)
    //    {
    //        Board = board;
    //        Draw = draw;
    //    }

    //    #endregion Constructor

    //    #region Members

    //    public readonly ChessBoard Board;
    //    public readonly ChessDraw Draw;

    //    #endregion Members

    //    #region Methods

    //    public override bool Equals(object obj)
    //    {
    //        return 
    //            obj.GetType() == typeof(DrawSituation) 
    //            && ((DrawSituation)obj).Draw.Equals(Draw)
    //            && ((DrawSituation)obj).Board.Equals(Board);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Draw.GetHashCode() + Board.GetHashCode();
    //    }

    //    #endregion Methods
    //}

    /// <summary>
    /// 
    /// </summary>
    public class WinRateInfo
    {
        #region Members

        public Tuple<ChessBoard, ChessDraw> Situation { get; set; }
        public double WinRate { get; set; }
        public int AnalyzedGames { get; set; }

        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class WinRateInfoSerializer
    {
        #region Constants

        private const string NODE_WIN_RATES = "rates";
        private const string NODE_WIN_RATE = "winRate";

        private const string ATTRIBUTE_WIN_RATE_BOARD = "board";
        private const string ATTRIBUTE_WIN_RATE_DRAW = "draw";
        private const string ATTRIBUTE_WIN_RATE_PERCENTAGE = "percentage";
        private const string ATTRIBUTE_WIN_RATE_TOTAL_GAMES = "totalGames";

        private static readonly CultureInfo US_FORMAT = CultureInfo.CreateSpecificCulture("en-US");

        #endregion Constants

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="games"></param>
        public void Serialize(string filePath, IEnumerable<ChessGame> games)
        {
            // calculate the win rate of each draw
            var winRateInfos = getWinPercentages(games);

            // write win percentages to XML data file
            using (var writer = XmlWriter.Create(filePath))
            {
                // TODO: add formatting
                
                writer.WriteStartDocument();
                writer.WriteStartElement(NODE_WIN_RATES);
                //writer.WriteAttributeString("whiteOffset", offsetWhiteWinRate.ToString(usFormat));

                foreach (var winRateInfo in winRateInfos)
                {
                    writer.WriteStartElement(NODE_WIN_RATE);
                    writer.WriteAttributeString(ATTRIBUTE_WIN_RATE_BOARD, winRateInfo.Situation.Item1.ToHash());
                    writer.WriteAttributeString(ATTRIBUTE_WIN_RATE_DRAW, winRateInfo.Situation.Item2.GetHashCode().ToString());
                    writer.WriteAttributeString(ATTRIBUTE_WIN_RATE_PERCENTAGE, winRateInfo.WinRate.ToString(US_FORMAT));
                    writer.WriteAttributeString(ATTRIBUTE_WIN_RATE_TOTAL_GAMES, winRateInfo.AnalyzedGames.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public IEnumerable<WinRateInfo> Deserialize(string filePath)
        {
            var winRateInfos = new List<WinRateInfo>();

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    // init new win rate info
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(NODE_WIN_RATE))
                    {
                        var board = reader.GetAttribute(ATTRIBUTE_WIN_RATE_BOARD).HashToBoard();
                        var draw = new ChessDraw(int.Parse(reader.GetAttribute(ATTRIBUTE_WIN_RATE_DRAW)));
                        double winRate = double.Parse(reader.GetAttribute(ATTRIBUTE_WIN_RATE_PERCENTAGE), US_FORMAT);
                        int analyzedGames = int.Parse(reader.GetAttribute(ATTRIBUTE_WIN_RATE_TOTAL_GAMES));

                        winRateInfos.Add(new WinRateInfo() {
                            Situation = new Tuple<ChessBoard, ChessDraw>(board, draw),
                            WinRate = winRate,
                            AnalyzedGames = analyzedGames
                        });
                    }
                }
            }

            return winRateInfos;
        }

        #region Helpers

        private IEnumerable<WinRateInfo> getWinPercentages(IEnumerable<ChessGame> games)
        {
            return
                games.Where(game => game.Winner != null).AsParallel().SelectMany(game => {

                    var drawsXWinner = new List<Tuple<Tuple<ChessBoard, ChessDraw>, ChessColor>>();

                    var winningSide = game.Winner.Value;
                    var tempGame = new ChessGame();

                    foreach (var draw in game.AllDraws)
                    {
                        var board = (ChessBoard)tempGame.Board.Clone();
                        drawsXWinner.Add(new Tuple<Tuple<ChessBoard, ChessDraw>, ChessColor>(new Tuple<ChessBoard, ChessDraw>(board, draw), winningSide));
                        tempGame.ApplyDraw(draw);
                    }

                    return drawsXWinner;
                })
                .GroupBy(x => x.Item1).Where(x => x.Count() >= 5).AsParallel().Select(group => {

                    int drawingSideWins = group.Count(x => x.Item2 == group.Key.Item2.DrawingSide);
                    int totalGames = group.Count();

                    double winRate = (double)drawingSideWins / totalGames;
                    return new WinRateInfo() { Situation = group.Key, WinRate = winRate, AnalyzedGames = totalGames };
                })
                .ToList();
        }

        #endregion Helpers

        #endregion Methods
    }
}
