using System;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration of all chess piece types.
    /// </summary>
    public enum ChessPieceType
    {
        /// <summary>
        /// Representing a king chess piece.
        /// </summary>
        King = 0,

        /// <summary>
        /// Representing a queen chess piece.
        /// </summary>
        Queen = 1,

        /// <summary>
        /// Representing a rook chess piece.
        /// </summary>
        Rook = 2,

        /// <summary>
        /// Representing a bishop chess piece.
        /// </summary>
        Bishop = 3,

        /// <summary>
        /// Representing a knigh chess piece.
        /// </summary>
        Knight = 4,

        /// <summary>
        /// Representing a peasant chess piece.
        /// </summary>
        Peasant = 5
    }
    
    /// <summary>
    /// An enumeration of all chess piece colors.
    /// </summary>
    public enum ChessColor
    {
        /// <summary>
        /// Representing white chess pieces, white chess fields, etc.
        /// </summary>
        White = 0,

        /// <summary>
        /// Representing black chess pieces, black chess fields, etc.
        /// </summary>
        Black = 1,
    }
    
    /// <summary>
    /// Represents a chess piece by its color, chess piece type and whether it was already moved.
    /// </summary>
    public struct ChessPiece : ICloneable
    {
        #region Constants

        // define the trailing bits after the data bits
        private const byte COLOR_TRAILING_BITS     = 4;
        private const byte WAS_MOVED_TRAILING_BITS = 3;
        
        // define which bits of the hash code store the data
        private const byte BITS_OF_COLOR          = 0b_10000;   // bits: 10000
        private const byte BITS_OF_WAS_MOVED_FLAG = 0b_01000;   // bits: 01000
        private const byte BITS_OF_TYPE           = 0b_00111;   // bits: 00111

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Creates a chess piece instance with the given parameters.
        /// </summary>
        /// <param name="type">The type of the chess piece</param>
        /// <param name="color">The color of the chess piece</param>
        /// <param name="wasMoved">Indicates whether the chess piece was already moved</param>
        public ChessPiece(ChessPieceType type, ChessColor color, bool wasMoved)
        {
            byte colorBits = (byte)(((int)color) << COLOR_TRAILING_BITS);
            byte wasMovedBits = (byte)((wasMoved ? 1 : 0) << WAS_MOVED_TRAILING_BITS);
            byte typeBits = (byte)type;

            // fuse the bit patterns to the hash code (with bitwise OR)
            _hashCode = (byte)(colorBits | wasMovedBits | typeBits);
        }

        /// <summary>
        /// Creates a chess piece instance from hash code.
        /// </summary>
        /// <param name="hashCode">The hash code containing the chess piece data</param>
        public ChessPiece(byte hashCode)
        {
            // make sure the hash code is within the expected value range
            if (hashCode < 0 || hashCode >= 0b_100000) { throw new ArgumentException("invalid hash code detected (expected a number of set { 0, 1, ..., 31 })"); }

            _hashCode = hashCode;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The binary representation containing the chess piece data.
        /// 
        /// The code consists of 5 bits: 
        /// 3 bits for piece type and another 1 bit for color / was moved flag
        /// 
        /// | unused | color | was moved | type |
        /// |    xxx |     x |         x |  xxx |
        /// </summary>
        private byte _hashCode;
        
        /// <summary>
        /// The color of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessColor Color
        {
            get { return (ChessColor)((_hashCode & BITS_OF_COLOR) >> COLOR_TRAILING_BITS); }
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_COLOR) | (((byte)value) << COLOR_TRAILING_BITS)); }
        }

        /// <summary>
        /// Indicates whether the chess piece was already drawn. (calculated from hash code)
        /// </summary>
        public bool WasMoved
        {
            get { return ((_hashCode & BITS_OF_WAS_MOVED_FLAG) >> WAS_MOVED_TRAILING_BITS) == 1; }
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_WAS_MOVED_FLAG) | (((byte)(value ? 1 : 0)) << WAS_MOVED_TRAILING_BITS)); }
        }

        /// <summary>
        /// The type of the chess piece. (calculated from hash code)
        /// </summary>
        public ChessPieceType Type
        {
            get { return (ChessPieceType)(_hashCode & BITS_OF_TYPE); }
            set { _hashCode = (byte)((_hashCode & ~BITS_OF_TYPE) | ((byte)value)); }
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

            return $"{ color } { type }";
        }

        /// <summary>
        /// Overrides Equals() method by evaluating the overloaded object type and comparing the properties.
        /// </summary>
        /// <param name="obj">The object instance to be compared to 'this'</param>
        /// <returns>a boolean indicating whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(ChessPiece)) && (((ChessPiece)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore the hash code of the instance is returned.
        /// </summary>
        /// <returns>a hash code that is unique for each chess piece</returns>
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

    /// <summary>
    /// This is an extension class providing the conversion from a chess piece type enumeration to a character value.
    /// </summary>
    public static class ChessPieceTypeAsChar
    {
        /// <summary>
        /// Retrieve the character representing the given chess piece type.
        /// </summary>
        /// <param name="type">The chess piece type to be represented</param>
        /// <returns>a character representing the given chess piece type</returns>
        public static char ToChar(this ChessPieceType type)
        {
            switch (type)
            {
                case ChessPieceType.King:    return 'K';
                case ChessPieceType.Queen:   return 'Q';
                case ChessPieceType.Rook:    return 'R';
                case ChessPieceType.Bishop:  return 'B';
                case ChessPieceType.Knight:  return 'N';
                case ChessPieceType.Peasant: return 'P';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    /// <summary>
    /// This is an extension class providing the conversion from a chess color enumeration to a character value.
    /// </summary>
    public static class ChessPieceColorAsChar
    {
        /// <summary>
        /// Retrieve the character representing the given chess color.
        /// </summary>
        /// <param name="color">The chess color to be represented</param>
        /// <returns>a character representing the given chess color</returns>
        public static char ToChar(this ChessColor color)
        {
            switch (color)
            {
                case ChessColor.White: return 'W';
                case ChessColor.Black: return 'B';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    /// <summary>
    /// This is an extension class providing the conversion from a chess color enumeration to its' complementary color (white vs. black).
    /// </summary>
    public static class ChessPieceColorOpponent
    {
        /// <summary>
        /// Retrieve the opponent's chess color according to the given allied chess color. (complementary)
        /// </summary>
        /// <param name="color">The allied chess color</param>
        /// <returns>the opponent's chess color</returns>
        public static ChessColor Opponent(this ChessColor color)
        {
            return (color == ChessColor.White) ? ChessColor.Black : ChessColor.White;
        }
    }
}
