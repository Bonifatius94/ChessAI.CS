using Chess.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    /// <summary>
    /// This struct represents a chess board and all fields / pieces on it. It is designed in a human-readable / understandable manner.
    /// </summary>
    public readonly struct ChessBoard : IChessBoard, ICloneable
    {
        #region Constants
        
        private static readonly ChessPieceAtPos[] START_FORMATION = new ChessPieceAtPos[]
        {
            new ChessPieceAtPos(new ChessPosition("A1"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("B1"), new ChessPiece(ChessPieceType.Knight,  ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("C1"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("D1"), new ChessPiece(ChessPieceType.Queen,   ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("E1"), new ChessPiece(ChessPieceType.King,    ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("F1"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("G1"), new ChessPiece(ChessPieceType.Knight,  ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("H1"), new ChessPiece(ChessPieceType.Rook,    ChessColor.White, false)),

            new ChessPieceAtPos(new ChessPosition("A2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("B2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("C2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("D2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("E2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("F2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("G2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),
            new ChessPieceAtPos(new ChessPosition("H2"), new ChessPiece(ChessPieceType.Peasant, ChessColor.White, false)),

            new ChessPieceAtPos(new ChessPosition("A7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("B7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("C7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("D7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("E7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("F7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("G7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("H7"), new ChessPiece(ChessPieceType.Peasant, ChessColor.Black, false)),

            new ChessPieceAtPos(new ChessPosition("A8"), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("B8"), new ChessPiece(ChessPieceType.Knight,  ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("C8"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("D8"), new ChessPiece(ChessPieceType.Queen,   ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("E8"), new ChessPiece(ChessPieceType.King,    ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("F8"), new ChessPiece(ChessPieceType.Bishop,  ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("G8"), new ChessPiece(ChessPieceType.Knight,  ChessColor.Black, false)),
            new ChessPieceAtPos(new ChessPosition("H8"), new ChessPiece(ChessPieceType.Rook,    ChessColor.Black, false)),
        };

        #endregion Constants

        #region Constructor

        /// <summary>
        /// Create a new instance of a chess board with the given chess pieces.
        /// </summary>
        /// <param name="piecesAtPos">The chess pieces to be applied to the chess board</param>
        public ChessBoard(IEnumerable<ChessPieceAtPos> piecesAtPos) : this(convertPieces(piecesAtPos)) { }

        /// <summary>
        /// Create a new instance of a chess board with the given chess pieces.
        /// </summary>
        /// <param name="pieces">The chess pieces to be applied to the chess board</param>
        public ChessBoard(ChessPiece[] pieces)
        {
            // apply pieces array
            _pieces = pieces;
        }

        #region Preparation

        private static ChessPiece[] convertPieces(IEnumerable<ChessPieceAtPos> piecesAtPos)
        {
            var pieces = new ChessPiece[64];
            foreach (var pieceAtPos in piecesAtPos) { pieces[pieceAtPos.Position.GetHashCode()] = pieceAtPos.Piece; }
            return pieces;
        }

        #endregion Preparation

        #endregion Constructor

        #region Members

        /// <summary>
        /// <para>An array of all chess pieces at the index of their position's hash code. 
        /// The piece value is zero - or more precisely 0x00 - if there is no chess piece at the position.</para>
        /// <para>The array consists of the ChessPiece struct which represents all possible chess pieces with a single byte. 
        /// Therefore this array is just a specialized byte[] array of size 64.</para>
        /// <para>As the ChessPiece struct does not store the piece's position on the board, 
        /// accessing the piece is implemented by the ChessPosition struct's 6-bit position index (0b_rrrccc).</para>
        /// </summary>
        private readonly ChessPiece[] _pieces;

        /// <summary>
        /// Retrieve a new chess board instance with start formation.
        /// </summary>
        public static ChessBoard StartFormation { get { return new ChessBoard(START_FORMATION); } }

        /// <summary>
        /// A list of all chess pieces (and their position) that are currently on the chess board.
        /// </summary>
        public IEnumerable<ChessPieceAtPos> AllPieces
        {
            get
            {
                // determine the pieces count
                byte piecesCount = 0;
                for (byte pos = 0; pos < 64; pos++) { if (_pieces[pos].HasValue) { piecesCount++; } }

                // fill the pieces array
                byte i = 0;
                var pieces = new ChessPieceAtPos[piecesCount];
                for (byte pos = 0; pos < 64; pos++) { if (_pieces[pos].HasValue) { pieces[i++] = new ChessPieceAtPos(new ChessPosition(pos), _pieces[pos]); } }

                return pieces;
            }
        }

        /// <summary>
        /// Selects all white chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> WhitePieces { get { return GetPiecesOfColor(ChessColor.White); } }

        /// <summary>
        /// Selects all black chess pieces from the chess pieces list. (computed operation)
        /// </summary>
        public IEnumerable<ChessPieceAtPos> BlackPieces { get { return GetPiecesOfColor(ChessColor.Black); } }

        /// <summary>
        /// Selects the white king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos WhiteKing { get { return WhitePieces.First(x => x.Piece.Type == ChessPieceType.King); } }

        /// <summary>
        /// Selects the black king from the chess pieces list. (computed operation)
        /// </summary>
        public ChessPieceAtPos BlackKing { get { return BlackPieces.First(x => x.Piece.Type == ChessPieceType.King); } }

        #endregion Members

        #region Methods

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="position">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsCapturedAt(ChessPosition position)
        {
            return _pieces[position.GetHashCode()].HasValue;
        }

        /// <summary>
        /// Indicates whether the chess field at the given positon is captured by a chess piece.
        /// </summary>
        /// <param name="positionHash">The chess field to check</param>
        /// <returns>A boolean that indicates whether the given chess field is captured</returns>
        public bool IsCapturedAt(byte positionHash)
        {
            return _pieces[positionHash].HasValue;
        }

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="positionHash">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece GetPieceAt(byte positionHash)
        {
            return _pieces[positionHash];
        }

        /// <summary>
        /// Retrieves the chess piece or null according to the given position on the chess board.
        /// </summary>
        /// <param name="position">The chess field</param>
        /// <returns>the chess piece at the given position or null (if the chess field is not captured)</returns>
        public ChessPiece GetPieceAt(ChessPosition position)
        {
            return _pieces[position.GetHashCode()];
        }

        /// <summary>
        /// Retrieve all chess pieces of the given player's side.
        /// </summary>
        /// <param name="side">The player's side</param>
        /// <returns>a list of all chess pieces of the given player's side</returns>
        public IEnumerable<ChessPieceAtPos> GetPiecesOfColor(ChessColor side)
        {
            return AllPieces.Where(x => x.Piece.Color == side).ToArray();
        }

        /// <summary>
        /// Update the chess piece at the given position.
        /// </summary>
        /// <param name="piecesToUpdate">The list of pieces to apply to the new board.</param>
        /// <returns>the new chess board containing the updated pieces</returns>
        private ChessBoard updatePiecesAt(IEnumerable<ChessPieceAtPos> piecesToUpdate)
        {
            var pieces = (ChessPiece[])_pieces.Clone();
            foreach (var pieceAtPos in piecesToUpdate) { pieces[pieceAtPos.Position.GetHashCode()] = pieceAtPos.Piece; }
            return new ChessBoard(pieces);
        }

        /// <summary>
        /// Draw the chess piece to the given position on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draw">The chess draw to be executed</param>
        public ChessBoard ApplyDraw(ChessDraw draw)
        {
            var piecesToUpdate = new List<ChessPieceAtPos>();

            // get the destination chess field instance of the chess board
            var drawingPiece = GetPieceAt(draw.OldPosition);
            //var pieceToTake = GetPieceAt(draw.NewPosition);
            
            // update drawing piece data
            drawingPiece.WasMoved = true;

            // handle peasant promotion
            if (draw.Type == ChessDrawType.PeasantPromotion) { drawingPiece.Type = draw.PeasantPromotionPieceType.Value; }

            // handle rochade
            if (draw.Type == ChessDrawType.Rochade)
            {
                // get the rook involved and its old and new position
                var oldRookPosition = new ChessPosition(draw.NewPosition.Row, (draw.NewPosition.Column == 2) ? 0 : 7);
                var newRookPosition = new ChessPosition(draw.NewPosition.Row, (draw.NewPosition.Column == 2) ? 3 : 5);
                var drawingRook = GetPieceAt(oldRookPosition);

                // move the tower
                piecesToUpdate.Add(new ChessPieceAtPos(oldRookPosition, ChessPiece.NULL));
                piecesToUpdate.Add(new ChessPieceAtPos(newRookPosition, drawingRook));
            }

            // handle en-passant
            if (draw.Type == ChessDrawType.EnPassant)
            {
                // get position of the taken enemy peasant and remove it
                var takenPeasantPosition = new ChessPosition((draw.DrawingSide == ChessColor.White) ? 4 : 3, draw.NewPosition.Column);
                piecesToUpdate.Add(new ChessPieceAtPos(takenPeasantPosition, ChessPiece.NULL));
            }

            // apply data to the chess board
            piecesToUpdate.Add(new ChessPieceAtPos(draw.OldPosition, ChessPiece.NULL));
            piecesToUpdate.Add(new ChessPieceAtPos(draw.NewPosition, drawingPiece));

            // apply changes to the new immutable chess board
            return updatePiecesAt(piecesToUpdate);
        }

        /// <summary>
        /// Draw the chess pieces to the given positions on the chess board. Also handle enemy pieces that get taken and special draws.
        /// </summary>
        /// <param name="draws">The chess draws to be executed</param>
        public ChessBoard ApplyDraws(IList<ChessDraw> draws)
        {
            ChessBoard board = this;
            foreach (var draw in draws) { board = board.ApplyDraw(draw); }
            return board;
        }

        /// <summary>
        /// Transform the current game situation of the chess board into a text format.
        /// 
        /// e.g. start position:
        /// 
        ///   -----------------------------------------
        /// 8 | BR | BN | BB | BQ | BK | BB | BN | BR |
        ///   -----------------------------------------
        /// 7 | BP | BP | BP | BP | BP | BP | BP | BP |
        ///   -----------------------------------------
        /// 6 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 5 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 4 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 3 |    |    |    |    |    |    |    |    |
        ///   -----------------------------------------
        /// 2 | WP | WP | WP | WP | WP | WP | WP | WP |
        ///   -----------------------------------------
        /// 1 | WR | WN | WB | WQ | WK | WB | WN | WR |
        ///   -----------------------------------------
        ///     A    B    C    D    E    F    G    H
        /// </summary>
        /// <returns>a string representing the current game situation that can be printed e.g. to a CLI</returns>
        public override string ToString()
        {
            const string SEPARATING_LINE = "   -----------------------------------------";
            var builder = new StringBuilder(SEPARATING_LINE).AppendLine();

            for (int row = 7; row >= 0; row--)
            {
                builder.Append($" { row + 1 } |");

                for (int column = 0; column < 8; column++)
                {
                    var position = new ChessPosition(row, column);
                    var piece = GetPieceAt(position);

                    // TODO: try to use unicode chess symbols
                    char chessPieceColor = IsCapturedAt(position) ? piece.Color.ToChar() : ' ';
                    char chessPieceType = IsCapturedAt(position) ? piece.Type.ToChar() : ' ';
                    builder.Append($" { chessPieceColor }{ chessPieceType } |");
                }

                builder.AppendLine();
                builder.AppendLine(SEPARATING_LINE);
            }

            builder.Append(" ");

            for (int column = 0; column < 8; column++)
            {
                builder.Append($"    { (char)('A' + column) }");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Create a new chess board instance with the same game situation.
        /// </summary>
        /// <returns>a new chess board instance</returns>
        public object Clone()
        {
            return new ChessBoard(this.AllPieces);
        }

        /// <summary>
        /// Determine whether another object is equal to this object.
        /// </summary>
        /// <param name="obj">The object to be compared to</param>
        /// <returns>a boolean indicating whether the objects are equal</returns>
        public override bool Equals(object obj)
        {
            // make sure that the object types are the same and the pieces on the boards match
            return (obj != null && obj.GetType() == typeof(ChessBoard)) && (this.ToHash().Equals(((ChessBoard)obj).ToHash()));
        }

        /// <summary>
        /// Overwrite hash code function and compute a hash value that is often but not always unique.
        /// </summary>
        /// <returns>hash of pieces string, may not always be unique</returns>
        public override int GetHashCode()
        {
            // unfortunately the most compact chess board representation requires 40 bytes instead of 4 bytes,
            // so returning the leading 4 bytes of the board may already be a good equality indicator.
            return BitConverter.ToInt32(this.ToBitboard().BinaryData.Take(4).ToArray());
        }

        /// <summary>
        /// Implements the '==' operator for comparing chess boards.
        /// </summary>
        /// <param name="c1">The first chess board to compare</param>
        /// <param name="c2">The second chess board to compare</param>
        /// <returns>a boolean that indicates whether the chess boards are equal</returns>
        public static bool operator ==(ChessBoard c1, ChessBoard c2)
        {
            return c1.Equals(c2);
        }

        /// <summary>
        /// Implements the '!=' operator for comparing chess boards.
        /// </summary>
        /// <param name="c1">The first chess board to compare</param>
        /// <param name="c2">The second chess board to compare</param>
        /// <returns>a boolean that indicates whether the chess boards are not equal</returns>
        public static bool operator !=(ChessBoard c1, ChessBoard c2)
        {
            return !c1.Equals(c2);
        }

        #endregion Methods
    }
}
