﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Chess.Lib
{
    public interface IChessDrawHelper
    {
        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="avoidDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool avoidDrawIntoCheck);
    }

    public class ChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
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

            var draws = helper.GetPossibleDraws(board, piece, precedingEnemyDraw, analyzeDrawIntoCheck);
            
            return draws;
        }

        #endregion Methods
    }

    public class KingChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // get the possible draw positions
            var positions = getKingDrawPositions(piece);

            // only retrieve positions that are not captured by an allied chess piece
            var alliedPieces = (piece.Color == ChessPieceColor.White) ? board.WhitePieces : board.BlackPieces;
            var positionsCapturedByEnemy = alliedPieces.Select(x => x.Position).ToList();
            positions = positions.Except(positionsCapturedByEnemy).ToList();

            // only retrieve positions that cannot be captured by the enemy king (-> no draw into check)
            var enemyKing = piece.Color == ChessPieceColor.White ? board.BlackKing : board.WhiteKing;
            var enemyKingDrawPositons = getKingDrawPositions(enemyKing);
            positions = positions.Except(enemyKingDrawPositons).ToList();

            // only retrieve positions that cannot be captured by other enemy chess pieces (-> no draw into check)
            var enemyCapturablePositions =
                board.Pieces.Where(x => x.Color != piece.Color && x != enemyKing)                              // select only enemy pieces that are not the king
                .SelectMany(x => new ChessDrawHelper().GetPossibleDraws(board, x, precedingEnemyDraw, false))  // compute draws of those enemy pieces
                .Select(x => x.NewPosition).ToList();

            positions = positions.Except(enemyCapturablePositions).ToList();

            // transform positions to draws
            var draws = positions.Select(newPos => new ChessDraw(piece.Color, ChessPieceType.King, piece.Position, newPos, board.PiecesByPosition.GetValueOrDefault(newPos)?.Type)).ToList();

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
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is a queen
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // combine the positions that a rock or a bishop could capture
            var draws =
                new RockChessDrawHelper().GetPossibleDraws(board, piece, precedingEnemyDraw, true)
                .Union(new BishopChessDrawHelper().GetPossibleDraws(board, piece, precedingEnemyDraw, true)).ToList();

            return draws;
        }

        #endregion Methods
    }

    public class RockChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            
            // make sure the chess piece is rock-like
            if (piece.Type != ChessPieceType.Rock && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rock."); }

            var draws = new List<ChessDraw>();
            
            // get upper-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row + i, piece.Position.Column);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get lower-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row - i, piece.Position.Column);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row, piece.Position.Column + i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row, piece.Position.Column - i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // TODO: remove draws that would draw into a check situation

            return draws;
        }
        
        public ChessDraw? GetRochadeDraw(ChessBoard board, ChessPiece piece)
        {
            // get allied king
            var alliedKing = (piece.Color == ChessPieceColor.White) ? board.WhiteKing : board.BlackKing;

            // TODO: implement logic

            return null;
        }

        #endregion Methods
    }

    public class BishopChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            var draws = new List<ChessDraw>();

            // get upper left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row + i, piece.Position.Column - i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get lower left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row - i, piece.Position.Column - i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get upper right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row + i, piece.Position.Column + i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // get lower right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var newPos = new ChessFieldPosition(piece.Position.Row - i, piece.Position.Column + i);
                if (!newPos.IsValid) { break; }

                var pieceAtPos = board.PiecesByPosition.GetValueOrDefault(newPos);
                bool addPosAsDraw = (pieceAtPos == null || pieceAtPos.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (addPosAsDraw) { draws.Add(new ChessDraw(piece.Color, piece.Type, piece.Position, newPos, pieceAtPos?.Type)); }
                if (abort) { break; }
            }

            // TODO: remove draws that would draw into a check situation

            return draws;
        }
        
        #endregion Methods
    }

    public class KnightChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
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

            // transform positions to chess draws
            var draws = positions.Select(newPos => new ChessDraw(piece.Color, ChessPieceType.Knight, piece.Position, newPos, board.PiecesByPosition.GetValueOrDefault(newPos)?.Type)).ToList();

            // TODO: remove draws that would draw into a check situation

            return draws;
        }

        #endregion Methods
    }

    public class PeasantChessDrawHelper : IChessDrawHelper
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public List<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Peasant) { throw new InvalidOperationException("The chess piece is not a peasant."); }

            // init draws list
            var draws = getForewardDraws(board, piece).Union(getCatchDraws(board, piece, precedingEnemyDraw)).ToList();

            // TODO: remove draws that would draw into a check situation

            return draws;
        }

        private List<ChessDraw> getForewardDraws(ChessBoard board, ChessPiece piece)
        {
            var posOneForeward = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column);
            var posTwoForeward = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 2 : -2), piece.Position.Column);

            bool oneForeward = posOneForeward.IsValid && !board.IsFieldCaptured(posOneForeward);
            bool twoForeward = oneForeward && !piece.WasAlreadyDrawn && posTwoForeward.IsValid && !board.IsFieldCaptured(posTwoForeward);

            var draws = new List<ChessDraw>();

            if (oneForeward) { draws.Add(new ChessDraw(piece.Color, ChessPieceType.Peasant, piece.Position, posOneForeward)); }
            if (twoForeward) { draws.Add(new ChessDraw(piece.Color, ChessPieceType.Peasant, piece.Position, posTwoForeward)); }

            return draws;
        }

        private List<ChessDraw> getCatchDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            var draws = new List<ChessDraw>();

            // get the possible chess field positions (info: right / left from the point of view of the white side player)
            var posCatchLeft = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column + 1);
            var posCatchRight = new ChessFieldPosition(piece.Position.Row + (piece.Color == ChessPieceColor.White ? 1 : -1), piece.Position.Column - 1);
            
            // check if right / left catch is possible
            bool catchLeft = posCatchLeft.IsValid && board.IsFieldCaptured(posCatchLeft) && board.PiecesByPosition[posCatchLeft].Color != piece.Color;
            bool catchRight = posCatchRight.IsValid && board.IsFieldCaptured(posCatchRight) && board.PiecesByPosition[posCatchRight].Color != piece.Color;

            if (catchLeft) { draws.Add(new ChessDraw(piece.Color, ChessPieceType.Peasant, piece.Position, posCatchLeft, board.PiecesByPosition[posCatchLeft].Type)); }
            if (catchRight) { draws.Add(new ChessDraw(piece.Color, ChessPieceType.Peasant, piece.Position, posCatchRight, board.PiecesByPosition[posCatchRight].Type)); }

            // check if en-passant is possible
            bool wasLastDrawPeasantDoubleForeward = precedingEnemyDraw.DrawingPieceType == ChessPieceType.Peasant && Math.Abs(precedingEnemyDraw.OldPosition.Row - precedingEnemyDraw.NewPosition.Row) == 2;

            if (wasLastDrawPeasantDoubleForeward)
            {
                // get the positions of an enemy chess piece taken by a possible en-passant
                var posEnPassantEnemyLeft = new ChessFieldPosition(piece.Position.Row, piece.Position.Column + 1);
                var posEnPassantEnemyRight = new ChessFieldPosition(piece.Position.Row, piece.Position.Column - 1);

                bool isLeftFieldCapturedByEnemy = board.IsFieldCaptured(posEnPassantEnemyLeft) && board.PiecesByPosition[posEnPassantEnemyLeft].Color != piece.Color;
                bool isRightFieldCapturedByEnemy = board.IsFieldCaptured(posEnPassantEnemyRight) && board.PiecesByPosition[posEnPassantEnemyRight].Color != piece.Color;

                bool enPassantLeft = isLeftFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyLeft.Column - piece.Position.Column) == 1;
                bool enPassantRight = isRightFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyRight.Column - piece.Position.Column) == 1;

                if (enPassantLeft) { draws.Add(new ChessDraw(ChessDrawType.EnPassant, piece.Color, piece.Position, posCatchLeft)); }
                if (enPassantRight) { draws.Add(new ChessDraw(ChessDrawType.EnPassant, piece.Color, piece.Position, posCatchRight)); }
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
            // clone chess board and simulate the draw
            var simulatedBoard = board.Clone() as ChessBoard;
            simulatedBoard.ApplyDraw(draw);

            // get all enemy chess pieces and their possible answers
            var enemyPieces = (draw.DrawingSide == ChessPieceColor.White) ? simulatedBoard.BlackPieces : simulatedBoard.WhitePieces;
            var possibleEnemyAnswers = enemyPieces.SelectMany(x => new ChessDrawHelper().GetPossibleDraws(board, x, draw, false)).ToList();

            // find out if the allied king could be taken by at least one enemy answer
            bool ret = possibleEnemyAnswers.Any(x => x.TakenEnemyPiece == ChessPieceType.King);

            return ret;
        }

        public bool CanOpponentCaptureField(ChessBoard board, ChessFieldPosition position, ChessPieceColor opponent)
        {
            // TODO: implement logic
            return false;
        }
        
        #endregion Methods
    }
}
