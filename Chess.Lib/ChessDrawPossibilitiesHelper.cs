using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    // TODO: use multiple threads to calculate the possible draws

    public interface IChessDrawPossibilitiesHelper
    {
        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="piece">The chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="avoidDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool avoidDrawIntoCheck);
    }

    public class ChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            IChessDrawPossibilitiesHelper helper;

            switch (piece.Type)
            {
                case ChessPieceType.King:    helper = new KingChessDrawPossibilitiesHelper();    break;
                case ChessPieceType.Queen:   helper = new QueenChessDrawPossibilitiesHelper();   break;
                case ChessPieceType.Rook:    helper = new RockChessDrawPossibilitiesHelper();    break;
                case ChessPieceType.Bishop:  helper = new BishopChessDrawPossibilitiesHelper();  break;
                case ChessPieceType.Knight:  helper = new KnightChessDrawPossibilitiesHelper();  break;
                case ChessPieceType.Peasant: helper = new PeasantChessDrawPossibilitiesHelper(); break;
                default: throw new ArgumentException("unknown chess piece type detected!");
            }

            var draws = helper.GetPossibleDraws(board, piece, precedingEnemyDraw, analyzeDrawIntoCheck).ToList();
            
            return draws;
        }

        #endregion Methods
    }

    public class KingChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // get the possible draw positions
            var positions = getKingDrawPositions(piece);

            // only retrieve positions that are not captured by an allied chess piece
            var alliedPieces = (piece.Color == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            var positionsCapturedByEnemy = alliedPieces.Select(x => x.Position);
            positions = positions.Except(positionsCapturedByEnemy);

            // only retrieve positions that cannot be captured by the enemy king (-> avoid draw into check)
            var enemyKing = piece.Color == ChessColor.White ? board.BlackKing : board.WhiteKing;
            var enemyKingDrawPositons = getKingDrawPositions(enemyKing);
            positions = positions.Except(enemyKingDrawPositons);

            // only retrieve positions that cannot be captured by other enemy chess pieces (-> no draw into check)
            var enemyCapturablePositions =
                board.Pieces.Where(x => x.Color != piece.Color && x.Type != ChessPieceType.King)                            // select only enemy pieces that are not the king
                .SelectMany(x => new ChessDrawPossibilitiesHelper().GetPossibleDraws(board, x, precedingEnemyDraw, false))  // compute draws of those enemy pieces
                .Select(x => x.NewPosition).ToList();

            positions = positions.Except(enemyCapturablePositions);

            // transform positions to draws
            var draws = positions.Select(newPos => new ChessDraw(board, piece.Position, newPos));

            // add rochade draws
            draws = draws.Concat(getPossibleRochadeDraws(board, piece.Color, enemyCapturablePositions));
            
            return draws;
        }

        private IEnumerable<ChessPosition> getKingDrawPositions(ChessPiece piece)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // cache old position (avoid multiple re-calculations in getter)
            var oldPos = piece.Position;

            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var coords = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(oldPos.Row - 1, oldPos.Column - 1),
                new Tuple<int, int>(oldPos.Row - 1, oldPos.Column    ),
                new Tuple<int, int>(oldPos.Row - 1, oldPos.Column + 1),
                new Tuple<int, int>(oldPos.Row    , oldPos.Column - 1),
                new Tuple<int, int>(oldPos.Row    , oldPos.Column + 1),
                new Tuple<int, int>(oldPos.Row + 1, oldPos.Column - 1),
                new Tuple<int, int>(oldPos.Row + 1, oldPos.Column    ),
                new Tuple<int, int>(oldPos.Row + 1, oldPos.Column + 1),
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            var positions = coords.Where(x => ChessPosition.AreCoordsValid(x)).Select(x => new ChessPosition(x.Item1, x.Item2));

            return positions;
        }

        private IEnumerable<ChessDraw> getPossibleRochadeDraws(ChessBoard board, ChessColor drawingSide, List<ChessPosition> enemyCapturablePositions)
        {
            var draws = new List<ChessDraw>();

            // get the allied king and towers
            int row = drawingSide == ChessColor.White ? 0 : 7;
            var alliedKing = drawingSide == ChessColor.White ? board.WhiteKing : board.BlackKing;
            var farAlliedTower = board.GetPieceAt(new ChessPosition(row, 0));
            var nearAlliedTower = board.GetPieceAt(new ChessPosition(row, 7));

            // define the fields that must not be capturable by the opponent
            var bigRochadeKingPassagePositions = new List<ChessPosition>() { new ChessPosition(row, 2), new ChessPosition(row, 3), new ChessPosition(row, 4) };
            var smallRochadeKingPassagePositions = new List<ChessPosition>() { new ChessPosition(row, 4), new ChessPosition(row, 5) };

            // check for preconditions of big rochade
            if (farAlliedTower != null && !alliedKing.WasMoved && !farAlliedTower.Value.WasMoved && bigRochadeKingPassagePositions.All(x => board.GetPieceAt(x) == null))
            {
                // make sure that no rochade field can be captured by the opponent
                bool canBigRochade = !enemyCapturablePositions.Any(pos => bigRochadeKingPassagePositions.Contains(pos));

                // add the draw to the list
                if (canBigRochade) { draws.Add(new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 2))); }
            }

            // check for preconditions of small rochade
            if (nearAlliedTower != null && !alliedKing.WasMoved && !nearAlliedTower.Value.WasMoved && smallRochadeKingPassagePositions.All(x => board.GetPieceAt(x) == null))
            {
                // make sure that no rochade field can be captured by the opponent
                bool canBigRochade = !enemyCapturablePositions.Any(pos => smallRochadeKingPassagePositions.Contains(pos));

                // add the draw to the list
                if (canBigRochade) { draws.Add(new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 2))); }
            }

            return draws;
        }

        #endregion Methods
    }

    public class QueenChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is a queen
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // combine the positions that a rock or a bishop could capture
            var draws =
                new RockChessDrawPossibilitiesHelper().GetPossibleDraws(board, piece, precedingEnemyDraw, analyzeDrawIntoCheck)
                .Union(new BishopChessDrawPossibilitiesHelper().GetPossibleDraws(board, piece, precedingEnemyDraw, analyzeDrawIntoCheck));

            return draws;
        }

        #endregion Methods
    }

    public class RockChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is rock-like
            if (piece.Type != ChessPieceType.Rook && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rock."); }

            var draws = new List<ChessDraw>();
            
            // get upper-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row + i, piece.Position.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row - i, piece.Position.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row, piece.Position.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row, piece.Position.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }
            
            return draws;
        }
        
        #endregion Methods
    }

    public class BishopChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            var draws = new List<ChessDraw>();

            // get upper left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row + i, piece.Position.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row - i, piece.Position.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get upper right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row + i, piece.Position.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(piece.Position.Row - i, piece.Position.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, piece.Position, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }
            
            return draws;
        }
        
        #endregion Methods
    }

    public class KnightChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is a knight
            if (piece.Type != ChessPieceType.Knight) { throw new InvalidOperationException("The chess piece is not a knight."); }

            IEnumerable<ChessDraw> draws = new List<ChessDraw>();

            if (analyzeDrawIntoCheck)
            {
                // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
                var coords = new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(piece.Position.Row - 2, piece.Position.Column - 1),
                    new Tuple<int, int>(piece.Position.Row - 2, piece.Position.Column + 1),
                    new Tuple<int, int>(piece.Position.Row - 1, piece.Position.Column - 2),
                    new Tuple<int, int>(piece.Position.Row - 1, piece.Position.Column + 2),
                    new Tuple<int, int>(piece.Position.Row + 1, piece.Position.Column - 2),
                    new Tuple<int, int>(piece.Position.Row + 1, piece.Position.Column + 2),
                    new Tuple<int, int>(piece.Position.Row + 2, piece.Position.Column - 1),
                    new Tuple<int, int>(piece.Position.Row + 2, piece.Position.Column + 1),
                };

                // only retrieve positions that are actually onto the chess board (and not off scale)
                var positions = coords.Where(x => ChessPosition.AreCoordsValid(x)).Select(x => new ChessPosition(x));

                // do not draw into positions captured by allied chess pieces
                positions = positions.Where(x => !board.IsCapturedAt(x) || board.GetPieceAt(x).Value.Color != piece.Color);

                // transform positions to chess draws
                draws = positions.Select(newPos => new ChessDraw(board, piece.Position, newPos));

                // remove draws that would draw into a check situation (all moves are invalid if one draws into check)
                draws = !(draws?.Count() > 0 && new ChessDrawSimulator().IsDrawIntoCheck(board, draws.First())) ? draws : new List<ChessDraw>();
            }
            
            return draws;
        }

        #endregion Methods
    }

    public class PeasantChessDrawPossibilitiesHelper : IChessDrawPossibilitiesHelper
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
        public IEnumerable<ChessDraw> GetPossibleDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Peasant) { throw new InvalidOperationException("The chess piece is not a peasant."); }

            // get all possible draws
            var draws = getForewardDraws(board, piece).Union(getCatchDraws(board, piece, precedingEnemyDraw));

            if (analyzeDrawIntoCheck)
            {
                // remove draws that would draw into a check situation
                draws = draws.Where(x => !new ChessDrawSimulator().IsDrawIntoCheck(board, x));
            }

            return draws;
        }

        private IEnumerable<ChessDraw> getForewardDraws(ChessBoard board, ChessPiece piece)
        {
            var draws = new List<ChessDraw>();

            var coordsPosOneForeward = new Tuple<int, int>(piece.Position.Row + (piece.Color == ChessColor.White ? 1 : -1), piece.Position.Column);
            var coordsPosTwoForeward = new Tuple<int, int>(piece.Position.Row + (piece.Color == ChessColor.White ? 2 : -2), piece.Position.Column);
            
            if (ChessPosition.AreCoordsValid(coordsPosOneForeward))
            {
                var posOneForeward = new ChessPosition(coordsPosOneForeward);
                bool oneForeward = !board.IsCapturedAt(posOneForeward);

                if (oneForeward)
                {
                    // handle peasant promotion
                    if ((posOneForeward.Row == 7 && piece.Color == ChessColor.White) || (posOneForeward.Row == 0 && piece.Color == ChessColor.Black))
                    {
                        // add options for peasant promotion (all piece types except king and peasant)
                        for (int i = 1; i < 5; i++)
                        {
                            var pieceType = (ChessPieceType)i;
                            draws.Add(new ChessDraw(board, piece.Position, posOneForeward, pieceType));
                        }
                    }
                    // handle normal foreward draw
                    else { draws.Add(new ChessDraw(board, piece.Position, posOneForeward)); }

                    if (!piece.WasMoved && ChessPosition.AreCoordsValid(coordsPosTwoForeward))
                    {
                        var posTwoForeward = new ChessPosition(coordsPosTwoForeward);

                        if (!board.IsCapturedAt(posTwoForeward))
                        {
                            draws.Add(new ChessDraw(board, piece.Position, posTwoForeward));
                        }
                    }
                }
            }
            
            return draws;
        }

        private IEnumerable<ChessDraw> getCatchDraws(ChessBoard board, ChessPiece piece, ChessDraw precedingEnemyDraw)
        {
            var draws = new List<ChessDraw>();

            // get the possible chess field positions (info: right / left from the point of view of the white side player)
            var coordsPosCatchLeft  = new Tuple<int, int>(piece.Position.Row + (piece.Color == ChessColor.White ? 1 : -1), piece.Position.Column + 1);
            var coordsPosCatchRight = new Tuple<int, int>(piece.Position.Row + (piece.Color == ChessColor.White ? 1 : -1), piece.Position.Column - 1);

            // check for en-passant precondition
            bool wasLastDrawPeasantDoubleForeward = precedingEnemyDraw.DrawingPieceType == ChessPieceType.Peasant && Math.Abs(precedingEnemyDraw.OldPosition.Row - precedingEnemyDraw.NewPosition.Row) == 2;

            // check if left catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchLeft))
            {
                var posCatchLeft = new ChessPosition(coordsPosCatchLeft);

                bool catchLeft = board.IsCapturedAt(posCatchLeft) && board.GetPieceAt(posCatchLeft).Value.Color != piece.Color;
                if (catchLeft) { draws.Add(new ChessDraw(board, piece.Position, posCatchLeft)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyLeft = new ChessPosition(piece.Position.Row, piece.Position.Column + 1);
                    bool isLeftFieldCapturedByEnemy = board.IsCapturedAt(posEnPassantEnemyLeft) && board.GetPieceAt(posEnPassantEnemyLeft).Value.Color != piece.Color;
                    bool enPassantLeft = isLeftFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyLeft.Column - piece.Position.Column) == 1;
                    if (enPassantLeft) { draws.Add(new ChessDraw(board, piece.Position, posCatchLeft)); }
                }
            }

            // check if right catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchRight))
            {
                var posCatchRight = new ChessPosition(coordsPosCatchRight);

                bool catchRight = board.IsCapturedAt(posCatchRight) && board.GetPieceAt(posCatchRight).Value.Color != piece.Color;
                if (catchRight) { draws.Add(new ChessDraw(board, piece.Position, posCatchRight)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyRight = new ChessPosition(piece.Position.Row, piece.Position.Column - 1);
                    bool isRightFieldCapturedByEnemy = board.IsCapturedAt(posEnPassantEnemyRight) && board.GetPieceAt(posEnPassantEnemyRight).Value.Color != piece.Color;
                    bool enPassantRight = isRightFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyRight.Column - piece.Position.Column) == 1;
                    if (enPassantRight) { draws.Add(new ChessDraw(board, piece.Position, posCatchRight)); }
                }
            }
            
            return draws;
        }

        #endregion Methods
    }
}
