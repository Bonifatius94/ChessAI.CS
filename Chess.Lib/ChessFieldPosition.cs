using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Lib
{
    /// <summary>
    /// This class represents a position on a chess board.
    /// </summary>
    public class ChessFieldPosition
    {
        #region Constructor

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ChessFieldPosition() { }

        /// <summary>
        /// Create a new field position instance from the given field name.
        /// </summary>
        /// <param name="fieldName">the chess field name (e.g. E5)</param>
        public ChessFieldPosition(string fieldName)
        {
            initWithFieldName(fieldName);
        }

        #endregion Constructor

        #region Members

        private static readonly Regex _regexFieldName = new Regex("^[A-H]{1}[1-8]{1}$");

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

        #region Methods

        private void initWithFieldName(string fieldName)
        {
            // check if the field name format is correct (otherwise throw argument exception)
            if (!_regexFieldName.IsMatch(fieldName)) { throw new ArgumentException($"invalid field name { fieldName }!"); }

            // parse row and column
            Row = fieldName[1] - '1';
            Column = fieldName[0] - 'A';
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the row and column propoerty.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var position = obj as ChessFieldPosition;
            return (position != null) && (position.Row == Row && position.Column == Column);
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore always 0 is returned, so the Equals() function needs to be called when hash codes are the same.
        /// </summary>
        /// <returns>a hash code (always 0 in this case)</returns>
        public override int GetHashCode()
        {
            return 0;
        }

        #endregion Methods
    }
}
