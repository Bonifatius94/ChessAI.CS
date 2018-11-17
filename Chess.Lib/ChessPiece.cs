using System;

namespace Chess.Lib
{
    /// <summary>
    /// An enumeration of all chess piece types. The enumerations are represented by a character value:
    /// 
    ///  - King (K)
    ///  - Queen (Q)
    ///  - Rock (R)
    ///  - Bishop (B)
    ///  - Knight (H)
    ///  - Peasant (P)
    /// </summary>
    public enum ChessPieceType
    {
        King     = 0,
        Queen    = 1,
        Rock     = 2,
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
    public enum ChessPieceColor
    {
        Black = 0,
        White = 1
    }
    
    public struct ChessPiece : ICloneable
    {
        #region Constants

        private const int POSITION_BITS = 6;
        private const int TYPE_BITS = 3;

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Creates a chess piece instance from hash code.
        /// </summary>
        /// <param name="hashCode">The hash code containing the chess piece data</param>
        public ChessPiece(int hashCode)
        {
            // decode color flag (1st leading bit)
            Color = (ChessPieceColor)((hashCode & 1024) >> 10);

            // decode was moved flag (2nd leading bit)
            WasAlreadyDrawn = ((hashCode & 512) >> 9) == 1;

            // decode chess piece type (3rd - 5th leading bit)
            Type = (ChessPieceType)((hashCode & 448) >> 6);

            // decode position (last 6 bits of the code)
            Position = new ChessFieldPosition(hashCode & 63);
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// The type of the chess piece.
        /// </summary>
        public ChessPieceType Type { get; set; }

        /// <summary>
        /// The color of the chess piece.
        /// </summary>
        public ChessPieceColor Color { get; set; }

        /// <summary>
        /// The position of the chess piece on the chess board.
        /// </summary>
        public ChessFieldPosition Position { get; set; }
        
        /// <summary>
        /// Indicates whether the chess piece was already drawn.
        /// </summary>
        public bool WasAlreadyDrawn { get; set; }
        
        #endregion Members

        #region Methods
        
        /// <summary>
        /// Create a deep copy of the current instance.
        /// </summary>
        /// <returns>a deep copy of the current instance</returns>
        public object Clone()
        {
            var piece = new ChessPiece() {
                Type = Type,
                Color = Color,
                Position = (ChessFieldPosition)Position.Clone(),
                WasAlreadyDrawn = WasAlreadyDrawn
            };

            return piece;
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
            return (obj.GetType() == typeof(ChessFieldPosition)) && (((ChessFieldPosition)obj).GetHashCode() == GetHashCode());
        }

        /// <summary>
        /// Override of GetHashCode() is required for Equals() method. Therefore a unique number is returned.
        /// 
        /// The code consists of 11 bits: 
        /// 6 bits for position, 3 bits for piece type and another 1 bit for color / was moved flag
        /// 
        /// | color | was moved | type | position |
        /// |     x |         x |  xxx |   xxxxxx |
        /// </summary>
        /// <returns>a hash code that is unique for each (row, column) tuple</returns>
        public override int GetHashCode()
        {
            int code;
            
            // encode color / was moved flag (leading 2 bits of the code)
            code = (((int)Color) << 1) + ((WasAlreadyDrawn) ? 1 : 0);

            // encode type (next 3 bits of the code)
            code = code << TYPE_BITS;
            code += (int)Type;
            
            // encode position (last 6 bits of the code)
            code = code << POSITION_BITS;
            code += Position.GetHashCode();
            
            return code;
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
                case ChessPieceType.King: return 'K';
                case ChessPieceType.Queen: return 'Q';
                case ChessPieceType.Rock: return 'R';
                case ChessPieceType.Bishop: return 'B';
                case ChessPieceType.Knight: return 'H'; // 'H' like hourse (because 'K' is already taken by king)
                case ChessPieceType.Peasant: return 'P';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }

    public static class ChessPieceColorAsChar
    {
        public static char ToChar(this ChessPieceColor type)
        {
            switch (type)
            {
                case ChessPieceColor.White: return 'W';
                case ChessPieceColor.Black: return 'B';
                default: throw new ArgumentException("unknown chess piece type detected!");
            }
        }
    }
}
