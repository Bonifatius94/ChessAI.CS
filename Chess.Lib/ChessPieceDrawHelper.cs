using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    public class ChessPieceDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the king (chess piece type obviously needs to be king).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <returns>a list of field positions</returns>
        public List<ChessFieldPosition> GetKingDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var positions = new List<ChessFieldPosition>()
            {
                new ChessFieldPosition() { Row = piece.Position.Row - 1, Column = piece.Position.Column - 1 },
                new ChessFieldPosition() { Row = piece.Position.Row - 1, Column = piece.Position.Column     },
                new ChessFieldPosition() { Row = piece.Position.Row - 1, Column = piece.Position.Column + 1 },
                new ChessFieldPosition() { Row = piece.Position.Row    , Column = piece.Position.Column - 1 },
                new ChessFieldPosition() { Row = piece.Position.Row    , Column = piece.Position.Column + 1 },
                new ChessFieldPosition() { Row = piece.Position.Row + 1, Column = piece.Position.Column - 1 },
                new ChessFieldPosition() { Row = piece.Position.Row + 1, Column = piece.Position.Column     },
                new ChessFieldPosition() { Row = piece.Position.Row + 1, Column = piece.Position.Column + 1 },
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            positions = positions.Where(x => x.Row >= 0 && x.Row < ChessBoard.CHESS_BOARD_DIMENSION && x.Column >= 0 && x.Column < ChessBoard.CHESS_BOARD_DIMENSION).ToList();

            return positions;
        }

        /// <summary>
        /// Compute the field positions that can be captured by the queen (chess piece type obviously needs to be queen).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <returns>a list of field positions</returns>
        public List<ChessFieldPosition> GetQueenDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // get positions that a rock or a bishop could capture
            return GetRockDrawPositions(piece).Union(GetBishopDrawPositions(piece)).ToList();
        }

        /// <summary>
        /// Compute the field positions that can be captured by the rock (chess piece type obviously needs to be rock-like).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessFieldPosition> GetRockDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is rock-like
            if (piece.Type != ChessPieceType.Rock || piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rock."); }

            // TODO: implement logic
            return null;
        }

        /// <summary>
        /// Compute the field positions that can be captured by the bishop (chess piece type obviously needs to be bishop-like).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessFieldPosition> GetBishopDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop || piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            // TODO: implement logic
            return null;
        }

        /// <summary>
        /// Compute the field positions that can be captured by the knight (chess piece type obviously needs to be knight).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessFieldPosition> GetKnightDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var positions = new List<ChessFieldPosition>()
            {
                new ChessFieldPosition() { Row = piece.Position.Row - 2, Column = piece.Position.Column - 1 },
                new ChessFieldPosition() { Row = piece.Position.Row - 2, Column = piece.Position.Column + 1 },
                new ChessFieldPosition() { Row = piece.Position.Row - 1, Column = piece.Position.Column - 2 },
                new ChessFieldPosition() { Row = piece.Position.Row - 1, Column = piece.Position.Column + 2 },
                new ChessFieldPosition() { Row = piece.Position.Row + 1, Column = piece.Position.Column - 2 },
                new ChessFieldPosition() { Row = piece.Position.Row + 1, Column = piece.Position.Column + 2 },
                new ChessFieldPosition() { Row = piece.Position.Row + 2, Column = piece.Position.Column - 1 },
                new ChessFieldPosition() { Row = piece.Position.Row + 2, Column = piece.Position.Column + 1 },
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            positions = positions.Where(x => x.Row >= 0 && x.Row < ChessBoard.CHESS_BOARD_DIMENSION && x.Column >= 0 && x.Column < ChessBoard.CHESS_BOARD_DIMENSION).ToList();

            return positions;
        }

        /// <summary>
        /// Compute the field positions that can be captured by the peasant (chess piece type obviously needs to be peasant).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessFieldPosition> GetPeasantDrawPositions(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            var positions = new List<ChessFieldPosition>();
            
            // check if single move is possible
            if (piece.Board.Fields[])

            // check if double move is possible
            if (piece.WasAlreadyDrawn)
            {
                positions.Add();
            }

            // TODO: implement logic

            return positions;
        }

        #endregion Methods
    }
}
