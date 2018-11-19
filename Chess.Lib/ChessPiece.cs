using System;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration of all chess piece types. The enumerations are represented by a character value:
    /// 
    ///  - King (K)
    ///  - Queen (Q)
    ///  - Rook (R)
    ///  - Bishop (B)
    ///  - Knight (H)
    ///  - Peasant (P)
    /// </summary>
    public enum ChessPieceType
    {
        King     = 0,
        Queen    = 1,
        Rook     = 2,
        Bishop   = 3,
        Knight   = 4,
        Peasant  = 5
    }
    
    /// <summary>
    /// An enumeration of all chess piece colors. The enumerations are represented by a character value:
    /// 
    ///  - Black (B)
    ///  - White (W)
    /// </summary>
    public enum ChessColor
    {
        Black = 0,
        White = 1
    }
    
    public struct ChessPiece : ICloneable
    {
        #region Constants
        
        // define the trailing bits after the data bits
        private const short TYPE_TRAILING_BITS      = 6;
        private const short WAS_MOVED_TRAILING_BITS = 9;
        private const short COLOR_TRAILING_BITS     = 10;
        
        // define which bits of the hash code store the data
        private const short BITS_OF_COLOR          = 1024;   // bits: 100 00000000
        private const short BITS_OF_WAS_MOVED_FLAG = 512;    // bits: 010 00000000
        private const short BITS_OF_TYPE           = 448;    // bits: 001 11000000
        private const short BITS_OF_POSITION       = 63;     // bits: 000 00111111

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Creates a chess piece instance from hash code.
        /// </summary>
        /// <param name="hashCode">The hash code containing the chess piece data</param>
        public ChessPiece(short hashCode)
        {
            _hashCode = hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The binary representation containing the chess piece data.
        /// 
        /// The code consists of 11 bits: 
        /// 6 bits for position, 3 bits for piece type and another 1 bit for color / was moved flag
        /// 
        /// | unused | color | was moved | type | position |
        /// |  xxxxx |     x |         x |  xxx |   xxxxxx |
        /// </summary>
        private short _hashCode;
        
        /// <summary>
        /// The color of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessColor Color
        {
            get { return (ChessColor)((_hashCode & BITS_OF_COLOR) >> COLOR_TRAILING_BITS); }
            set { _hashCode = (short)((_hashCode & ~BITS_OF_COLOR) | (((short)value) << COLOR_TRAILING_BITS)); }
        }

        /// <summary>
        /// Indicates whether the chess piece was already drawn. (calculated from hash code)
        /// </summary>
        public bool WasMoved
        {
            get { return ((_hashCode & BITS_OF_WAS_MOVED_FLAG) >> WAS_MOVED_TRAILING_BITS) == 1; }
            set { _hashCode = (short)((_hashCode & ~BITS_OF_WAS_MOVED_FLAG) | (((short)(value ? 1 : 0)) << WAS_MOVED_TRAILING_BITS)); }
        }

        /// <summary>
        /// The type of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessPieceType Type
        {
            get { return (ChessPieceType)((_hashCode & BITS_OF_TYPE) >> TYPE_TRAILING_BITS); }
            set { _hashCode = (short)((_hashCode & ~BITS_OF_TYPE) | (((short)value) << TYPE_TRAILING_BITS)); }
        }

        /// <summary>
        /// The position of the chess piece on the chess board. (calculated from hash code)
        /// </summary>
        public ChessPosition Position
        {
            get { return new ChessPosition((byte)(_hashCode & BITS_OF_POSITION)); }
            set { _hashCode = (short)((_hashCode & ~BITS_OF_POSITION) | value.GetHashCode()); }
        }
        
        #endregion Members

        #region Methods
        
        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        public object Clone()
        {
            return new ChessPiece(_hashCode);
        }

        /// <summary>
        /// Retrieve a string representing this chess piece.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string color = Color.ToString().ToLower();
            string type = Type.ToString().ToLower();

            return $"{ color } { type } ({ Position.ToString() })";
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the properties.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof(ChessPosition)) && (((ChessPosition)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore the hash code of the instance is returned.
        /// </summary>
        /// <returns>a hash code that is unique for each (row, column) tuple</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Implements the '==' operator for comparing chess pieces.
        /// </summary>
        /// <param name="c1">The first chess piece to compare</param>
        /// <param name="c2">The second chess piece to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are equal</returns>
        public static bool operator ==(ChessPiece c1, ChessPiece c2)
        {
            return c1.GetHashCode() == c2.GetHashCode();
        }

        /// <summary>
        /// Implements the '!=' operator for comparing chess pieces.
        /// </summary>
        /// <param name="c1">The first chess piece to compare</param>
        /// <param name="c2">The second chess piece to compare</param>
        /// <returns>a boolean that indicates whether the chess pieces are not equal</returns>
        public static bool operator !=(ChessPiece c1, ChessPiece c2)
        {
            return c1.GetHashCode() != c2.GetHashCode();
        }
        
        #endregion Methods
    }

    public static class ChessPieceTypeAsChar
    {
        public static char ToChar(this ChessPieceType type)
        {
            switch (type)
            {
                case ChessPieceType.King:    return 'K';
                case ChessPieceType.Queen:   return 'Q';
                case ChessPieceType.Rook:    return 'R';
                case ChessPieceType.Bishop:  return 'B';
                case ChessPieceType.Knight:  return 'H'; // 'H' like horse (because 'K' is already taken by king)
                case ChessPieceType.Peasant: return 'P';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    public static class ChessPieceColorAsChar
    {
        public static char ToChar(this ChessColor type)
        {
            switch (type)
            {
                case ChessColor.White: return 'W';
                case ChessColor.Black: return 'B';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }
}
