using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Lib
{
    /// <summary>
    /// Represents a position on a chess board.
    /// </summary>
    public readonly struct ChessFieldPosition : ICloneable
    {
        #region Constants

        /// <summary>
        /// The regex instance for validating a chess field name.
        /// </summary>
        private static readonly Regex _regexFieldName = new Regex("^[A-H]{1}[1-8]{1}$");

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Create a new field position instance from the given row and column.
        /// </summary>
        public ChessFieldPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Create a new field position instance from the given field name.
        /// </summary>
        /// <param name="fieldName">the chess field name (e.g. E5)</param>
        public ChessFieldPosition(string fieldName)
        {
            // check if the field name format is correct (otherwise throw argument exception)
            if (!_regexFieldName.IsMatch(fieldName)) { throw new ArgumentException($"invalid field name { fieldName }!"); }

            // parse row and column
            Row = fieldName[1] - '1';
            Column = fieldName[0] - 'A';
        }

        public ChessFieldPosition(int hashCode)
        {
            Column = (hashCode >> 3);
            Row = (hashCode & 7);
        }

        #endregion Constructor

        #region Members
        
        /// <summary>
        /// The row index of the chess field position, starting with 0.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// The column index of the chess field position, starting with 0.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Generates the chess field name out of Row and Column property (e.g. 'A1', 'H8').
        /// </summary>
        public string FieldName { get { return $"{ (char)('A' + Column) }{ (char)('1' + Row) }"; } }

        /// <summary>
        /// Indicates whether the row and column are in bounds of the chess board (0 &lt;= x &lt; 8)
        /// </summary>
        public bool IsValid { get { return Row >= 0 && Row < ChessBoard.CHESS_BOARD_DIMENSION && Column >= 0 && Column < ChessBoard.CHESS_BOARD_DIMENSION; } }

        #endregion Members

        #region Methods

        /// <summary>
        /// Retrieve a string representing this chess field position.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FieldName;
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the row and column propoerty.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof(ChessFieldPosition)) && (((ChessFieldPosition)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore column * 8 + row is returned, which is unique for each field position on the chess board.
        /// </summary>
        /// <returns>a hash code that is unique for each (row, column) tuple</returns>
        public override int GetHashCode()
        {
            // TODO: check if this calculation can be optimized by binary operations
            return Column * ChessBoard.CHESS_BOARD_DIMENSION + Row;
        }

        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        public object Clone()
        {
            return new ChessFieldPosition(Row, Column);
        }

        /// <summary>
        /// Implements the '==' operator for comparing positions.
        /// </summary>
        /// <param name="c1">The first position to compare</param>
        /// <param name="c2">The second position to compare</param>
        /// <returns>a boolean that indicates whether the positions are equal</returns>
        public static bool operator ==(ChessFieldPosition c1, ChessFieldPosition c2)
        {
            return c1.GetHashCode() == c2.GetHashCode();
        }

        /// <summary>
        /// Implements the '!=' operator for comparing positions.
        /// </summary>
        /// <param name="c1">The first position to compare</param>
        /// <param name="c2">The second position to compare</param>
        /// <returns>a boolean that indicates whether the positions are not equal</returns>
        public static bool operator !=(ChessFieldPosition c1, ChessFieldPosition c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }

        #endregion Methods
    }
}
