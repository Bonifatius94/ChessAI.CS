//using Chess.Lib;
//using Chess.Lib.Extensions;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace Chess.AI.Data
//{
//    /// <summary>
//    /// Representing an entity of an optimal chess draw and the corresponding game situation.
//    /// </summary>
//    public class OptimalChessDraw
//    {
//        #region Constructor

//        /// <summary>
//        /// Create an empty instance (for EF).
//        /// </summary>
//        public OptimalChessDraw() { }

//        /// <summary>
//        /// Create a new optimal chess draw instance with the given chess board and chess draw.
//        /// </summary>
//        /// <param name="board">the chess board with the game situation</param>
//        /// <param name="optimalDraw">the optimal chess draw</param>
//        public OptimalChessDraw(ChessBoard board, ChessDraw optimalDraw)
//        {
//            Board = board;
//            OptimalDraw = optimalDraw;
//        }

//        #endregion Constructor

//        #region Members

//        /// <summary>
//        /// The numberic ID column (primary key).
//        /// </summary>
//        [Key]
//        public ulong ID { get; set; }

//        /// <summary>
//        /// The unique game situation hash containing the drawing side and the chess board with the game situation.
//        /// </summary>
//        [StringLength(64)]
//        public string GameSituationHash { get; set; }

//        /// <summary>
//        /// The hash of the optimal chess draw for the given game situation.
//        /// </summary>
//        [StringLength(3)]
//        public int OptimalDrawHash { get; set; }

//        /// <summary>
//        /// The chess board with the game situation.
//        /// </summary>
//        [NotMapped]
//        public ChessBoard Board
//        {
//            get { return GameSituationHash.HashToBoard(); }
//            set { GameSituationHash = value.ToHash(); }
//        }

//        /// <summary>
//        /// The optimal chess draw for the given game situation.
//        /// </summary>
//        [NotMapped]
//        public ChessDraw OptimalDraw
//        {
//            get { return new ChessDraw(OptimalDrawHash); }
//            set { OptimalDrawHash = value.GetHashCode(); }
//        }

//        #endregion Members
//    }
//}
