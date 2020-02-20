//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Chess.AI.Data.DB
//{
//    /// <summary>
//    /// A database context connecting to a local chess database.
//    /// </summary>
//    public class ChessAIContext : DbContext
//    {
//        #region Members

//        /// <summary>
//        /// A set of optimal chess draws and the corresponding game situation.
//        /// </summary>
//        public DbSet<OptimalChessDraw> Draws { get; set; }

//        #endregion Members

//        #region Methods

//        /// <summary>
//        /// An override of the OnConfiguring() function. Initialize the context with a connection to a sqlite database.
//        /// </summary>
//        /// <param name="optionsBuilder"></param>
//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder.UseSqlite("Data Source=optimal-chessdraws.db");
//        }

//        #endregion Methods
//    }
//}
