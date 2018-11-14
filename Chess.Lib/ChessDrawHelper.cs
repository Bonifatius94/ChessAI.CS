using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Lib
{
    public interface IChessDrawHelper
    {
        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="considerEnemyCheckDraws">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw, bool considerEnemyCheckDraws);
    }

    public class ChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="considerEnemyCheckDraws">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw, bool considerEnemyCheckDraws)
        {
            IChessDrawHelper helper;

            switch (piece.Type)
            {
                case ChessPieceType.King:    helper = new KingChessDrawHelper();    break;
                case ChessPieceType.Queen:   helper = new QueenChessDrawHelper();   break;
                case ChessPieceType.Rock:    helper = new RockChessDrawHelper();    break;
                case ChessPieceType.Bishop:  helper = new BishopChessDrawHelper();  break;
                case ChessPieceType.Knight:  helper = new KnightChessDrawHelper();  break;
                case ChessPieceType.Peasant: helper = new PeasantChessDrawHelper(); break;
                default: throw new ArgumentException("unknown chess piece type detected!");
            }

            var draws = helper.GetPossibleDraws(piece, precedingEnemyDraw, considerEnemyCheckDraws);

            return draws;
        }

        #endregion Methods
    }

    public class KingChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the king (chess piece type obviously needs to be king).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // get the possible draw positions
            var positions = getKingDrawPositions(piece);

            // only retrieve positions that are not captured by an allied chess piece
            positions = positions.Where(x => piece.Board.Fields[x].Piece?.Color != piece.Color).ToList();

            // only retrieve positions that cannot be captured by the enemy king (-> no check)
            var enemyKing = piece.Color == ChessPieceColor.White ? piece.Board.BlackKing : piece.Board.WhiteKing;
            var enemyKingDrawPositons = getKingDrawPositions(enemyKing);
            positions = positions.Except(enemyKingDrawPositons).ToList();

            // only retrieve positions that cannot be captured by other enemy chess pieces (-> no check)
            var enemyCapturablePositions =
                piece.Board.Pieces.Where(x => x.Color != piece.Color && x != enemyKing)          // select only enemy pieces that are not the king
                .SelectMany(x => new ChessDrawHelper().GetPossibleDraws(x, precedingEnemyDraw))  // compute draws of those enemy pieces
                .Select(x => x.NewPosition).ToList();

            positions = positions.Except(enemyCapturablePositions).ToList();

            // transform positions to draws
            var draws = positions.Select(newPos => new ChessDraw()
            {
                Type = ChessDrawType.Standard,
                DrawingSide = piece.Color,
                DrawingPieceType = ChessPieceType.King,
                OldPosition = piece.Position,
                NewPosition = newPos,
                TakenEnemyPiece = piece.Board.Fields[newPos]?.Piece.Type
            }).ToList();

            return draws;
        }

        private List<ChessFieldPosition> getKingDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var positions = new List<ChessFieldPosition>()
            {
                new ChessFieldPosition(piece.Position.Row - 1, piece.Position.Column - 1),
                new ChessFieldPosition(piece.Position.Row - 1, piece.Position.Column    ),
                new ChessFieldPosition(piece.Position.Row - 1, piece.Position.Column + 1),
                new ChessFieldPosition(piece.Position.Row    , piece.Position.Column - 1),
                new ChessFieldPosition(piece.Position.Row    , piece.Position.Column + 1),
                new ChessFieldPosition(piece.Position.Row + 1, piece.Position.Column - 1),
                new ChessFieldPosition(piece.Position.Row + 1, piece.Position.Column    ),
                new ChessFieldPosition(piece.Position.Row + 1, piece.Position.Column + 1),
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            positions = positions.Where(x => x.IsValid).ToList();

            return positions;
        }

        #endregion Methods
    }

    public class QueenChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the queen (chess piece type obviously needs to be queen).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // get positions that a rock or a bishop could capture
            return new RockChessDrawHelper().GetPossibleDraws(piece, precedingEnemyDraw).Union(new BishopChessDrawHelper().GetPossibleDraws(piece, precedingEnemyDraw)).ToList();
        }

        #endregion Methods
    }

    public class RockChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the rock (chess piece type obviously needs to be rock-like).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is rock-like
            if (piece.Type != ChessPieceType.Rock || piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rock."); }

            // TODO: implement logic
            return null;
        }

        #endregion Methods
    }

    public class BishopChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the bishop (chess piece type obviously needs to be bishop-like).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop || piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            // TODO: implement logic
            return null;
        }

        #endregion Methods
    }

    public class KnightChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the knight (chess piece type obviously needs to be knight).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is a knight
            if (piece.Type != ChessPieceType.Knight) { throw new InvalidOperationException("The chess piece is not a knight."); }

            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var positions = new List<ChessFieldPosition>()
            {
                new ChessFieldPosition(piece.Position.Row - 2, piece.Position.Column - 1),
                new ChessFieldPosition(piece.Position.Row - 2, piece.Position.Column + 1),
                new ChessFieldPosition(piece.Position.Row - 1, piece.Position.Column - 2),
                new ChessFieldPosition(piece.Position.Row - 1, piece.Position.Column + 2),
                new ChessFieldPosition(piece.Position.Row + 1, piece.Position.Column - 2),
                new ChessFieldPosition(piece.Position.Row + 1, piece.Position.Column + 2),
                new ChessFieldPosition(piece.Position.Row + 2, piece.Position.Column - 1),
                new ChessFieldPosition(piece.Position.Row + 2, piece.Position.Column + 1),
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            positions = positions.Where(x => x.IsValid).ToList();

            return positions;
        }

        #endregion Methods
    }

    public class PeasantChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the peasant (chess piece type obviously needs to be peasant).
        /// </summary>
        /// <param name="piece">The piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <returns>a list of field positiosn</returns>
        public List<ChessDraw> GetPossibleDraws(ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Peasant) { throw new InvalidOperationException("The chess piece is not a peasant."); }

            // get the possible chess field positions (info: right / left from the point of view of the white side player)
            var posOneForeward = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column);
            var posTwoForeward = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 2 : -2), piece.Position.Column);
            var posCatchLeft = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column + 1);
            var posCatchRight = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column - 1);

            // get the positions of an enemy chess piece taken by the en-passant rule
            var posEnPassantEnemyLeft = new ChessFieldPosition(piece.Position.Row, piece.Position.Column + 1);
            var posEnPassantEnemyRight = new ChessFieldPosition(piece.Position.Row, piece.Position.Column - 1);

            // check if single / double forward move is possible
            bool oneForeward = posOneForeward.IsValid && !piece.Board.Fields[posOneForeward].IsCapturedByPiece;
            bool twoForeward = oneForeward && !piece.WasAlreadyDrawn && posTwoForeward.IsValid && !piece.Board.Fields[posTwoForeward].IsCapturedByPiece;

            // check if right / left catch is possible
            bool catchLeft = posCatchLeft.IsValid && piece.Board.Fields[posCatchLeft].IsCapturedByPiece && piece.Board.Fields[posCatchLeft].Piece.Color != piece.Color;
            bool catchRight = posCatchRight.IsValid && piece.Board.Fields[posCatchRight].IsCapturedByPiece && piece.Board.Fields[posCatchRight].Piece.Color != piece.Color;

            // check if en-passant is possible
            bool wasLastDrawPeasantDoubleForeward = precedingEnemyDraw.DrawingPieceType == ChessPieceType.Peasant && Math.Abs(precedingEnemyDraw.OldPosition.Row - precedingEnemyDraw.NewPosition.Row) == 2;
            bool enPassantLeft = wasLastDrawPeasantDoubleForeward && posEnPassantEnemyLeft.Row == piece.Position.Row && Math.Abs(posEnPassantEnemyLeft.Column - piece.Position.Column) == 1;
            bool enPassantRight = wasLastDrawPeasantDoubleForeward && posEnPassantEnemyRight.Row == piece.Position.Row && Math.Abs(posEnPassantEnemyRight.Column - piece.Position.Column) == 1;

            // collect possible chess draws
            var draws = new List<ChessDraw>();

            if (oneForeward) {
                draws.Add(new ChessDraw() {
                    Type = ChessDrawType.Standard, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posOneForeward
                });
            }

            if (twoForeward)
            {
                draws.Add(new ChessDraw()
                {
                    Type = ChessDrawType.Standard, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posTwoForeward
                });
            }

            if (catchLeft)
            {
                draws.Add(new ChessDraw()
                {
                    Type = ChessDrawType.Standard, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posCatchLeft, TakenEnemyPiece = piece.Board.Fields[posCatchLeft].Piece.Type
                });
            }

            if (catchRight)
            {
                draws.Add(new ChessDraw()
                {
                    Type = ChessDrawType.Standard, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posCatchRight, TakenEnemyPiece = piece.Board.Fields[posCatchRight].Piece.Type
                });
            }

            if (enPassantLeft)
            {
                draws.Add(new ChessDraw()
                {
                    Type = ChessDrawType.EnPassant, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posCatchLeft, TakenEnemyPiece = piece.Board.Fields[posEnPassantEnemyLeft].Piece.Type
                });
            }

            if (enPassantRight)
            {
                draws.Add(new ChessDraw()
                {
                    Type = ChessDrawType.EnPassant, DrawingSide = piece.Color, DrawingPieceType = ChessPieceType.Peasant,
                    OldPosition = piece.Position, NewPosition = posCatchRight, TakenEnemyPiece = piece.Board.Fields[posEnPassantEnemyRight].Piece.Type
                });
            }

            return draws;
        }

        #endregion Methods
    }

    public class ChessDrawSimulator
    {
        #region Methods
        
        public bool IsDrawIntoCheck(ChessBoard board, ChessDraw draw)
        {
            // clone chess board
            var simulatedBoard = board.Clone() as ChessBoard;

            // simulate the draw
            simulatedBoard.Fields[draw.OldPosition].Piece.Draw(draw.NewPosition);

            // get all enemy chess pieces
            var enemyPieces = (draw.DrawingSide == ChessPieceColor.White) ? simulatedBoard.BlackPieces : simulatedBoard.WhitePieces;

            // get all enemy draws of the enemy chess pieces
            var possibleEnemyAnswers = enemyPieces.SelectMany(x => new ChessDrawHelper().GetPossibleDraws(x, draw)).ToList();

            // find out if the allied king would be checked
            possibleEnemyAnswers.Any(x => x.)

            return false;
        }

        #endregion Methods
    }
}
