using System;
using System.Collections.Generic;
using System.Text;

namespace ChessAI.Lib
{
    public class ChessFieldPosition
    {
        #region Members

        /// <summary>
        /// The row index of the chess field position, starting with 0.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// The column index of the chess field position, starting with 0.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Generates the chess field name out of Row and Column property (e.g. 'A1', 'H8').
        /// </summary>
        public string FieldName { get { return $"{ (char)('A' + Column) }{ (char)('1' + Row) }"; } }

        #endregion Members
    }
}
