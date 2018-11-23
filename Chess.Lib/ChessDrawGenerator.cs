using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    // TODO: use multiple threads to calculate the possible draws (=> already tested, but was actually slower than the single-thread approach)

    public interface IChessDrawGenerator
    {
        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck);
    }

    public class ChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            IChessDrawGenerator helper;
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            switch (piece.Type)
            {
                case ChessPieceType.King:    helper = new KingChessDrawGenerator();    break;
                case ChessPieceType.Queen:   helper = new QueenChessDrawGenerator();   break;
                case ChessPieceType.Rook:    helper = new RockChessDrawGenerator();    break;
                case ChessPieceType.Bishop:  helper = new BishopChessDrawGenerator();  break;
                case ChessPieceType.Knight:  helper = new KnightChessDrawGenerator();  break;
                case ChessPieceType.Peasant: helper = new PeasantChessDrawGenerator(); break;
                default: throw new ArgumentException("unknown chess piece type detected!");
            }

            var draws = helper.GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck).ToList();
            
            return draws;
        }

        #endregion Methods
    }

    public class KingChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // get the possible draw positions
            var positions = getStandardDrawPositions(drawingPiecePosition);

            // only retrieve positions that are not captured by an allied chess piece
            var alliedPieces = (piece.Color == ChessColor.White) ? board.WhitePieces : board.BlackPieces;
            var positionsCapturedByAlly = alliedPieces.Select(x => x.Position);
            positions = positions.Except(positionsCapturedByAlly);

            // only retrieve positions that cannot be captured by the enemy king (-> avoid draw into check)
            var enemyKing = piece.Color == ChessColor.White ? board.BlackKing : board.WhiteKing;
            var enemyKingDrawPositons = getStandardDrawPositions(enemyKing.Position);
            positions = positions.Except(enemyKingDrawPositons);

            // only retrieve positions that cannot be captured by other enemy chess pieces (-> no draw into check)
            var enemyCapturablePositions =
                board.Pieces.Where(x => x.Piece.Color != piece.Color && x.Piece.Type != ChessPieceType.King)      // select only enemy pieces that are not the king
                .SelectMany(x => new ChessDrawGenerator().GetDraws(board, x.Position, precedingEnemyDraw, false)) // compute draws of those enemy pieces
                .Select(x => x.NewPosition).ToList();

            positions = positions.Except(enemyCapturablePositions);

            // transform positions to draws
            var draws = positions.Select(newPos => new ChessDraw(board, drawingPiecePosition, newPos));

            // add rochade draws
            draws = draws.Concat(getRochadeDraws(board, piece.Color, enemyCapturablePositions));
            
            return draws;
        }

        private IEnumerable<ChessPosition> getStandardDrawPositions(ChessPosition position)
        {
            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var coords = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(position.Row - 1, position.Column - 1),
                new Tuple<int, int>(position.Row - 1, position.Column    ),
                new Tuple<int, int>(position.Row - 1, position.Column + 1),
                new Tuple<int, int>(position.Row    , position.Column - 1),
                new Tuple<int, int>(position.Row    , position.Column + 1),
                new Tuple<int, int>(position.Row + 1, position.Column - 1),
                new Tuple<int, int>(position.Row + 1, position.Column    ),
                new Tuple<int, int>(position.Row + 1, position.Column + 1),
            };

            // only retrieve positions that are actually onto the chess board (and not off scale)
            var positions = coords.Where(x => ChessPosition.AreCoordsValid(x)).Select(x => new ChessPosition(x.Item1, x.Item2));

            return positions;
        }

        private IEnumerable<ChessDraw> getRochadeDraws(ChessBoard board, ChessColor drawingSide, List<ChessPosition> enemyCapturablePositions)
        {
            var draws = new List<ChessDraw>();

            // get the allied king and towers
            int row = drawingSide == ChessColor.White ? 0 : 7;
            var alliedKing = drawingSide == ChessColor.White ? board.WhiteKing : board.BlackKing;
            var farAlliedTower = board.GetPieceAt(new ChessPosition(row, 0));
            var nearAlliedTower = board.GetPieceAt(new ChessPosition(row, 7));

            // define the fields that must not be capturable by the opponent
            var bigRochadeKingPassagePositions = new List<ChessPosition>() { new ChessPosition(row, 2), new ChessPosition(row, 3), new ChessPosition(row, 4) };
            var smallRochadeKingPassagePositions = new List<ChessPosition>() { new ChessPosition(row, 4), new ChessPosition(row, 5), new ChessPosition(row, 6) };

            // check for preconditions of big rochade
            if (farAlliedTower != null && !alliedKing.Piece.WasMoved && !farAlliedTower.Value.WasMoved 
                && bigRochadeKingPassagePositions.Select(x => board.GetPieceAt(x)).All(x => x == null || x.Value.Type == ChessPieceType.King))
            {
                // make sure that no rochade field can be captured by the opponent
                bool canBigRochade = !enemyCapturablePositions.Any(pos => bigRochadeKingPassagePositions.Contains(pos));

                // add the draw to the list
                if (canBigRochade) { draws.Add(new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 2))); }
            }

            // check for preconditions of small rochade
            if (nearAlliedTower != null && !alliedKing.Piece.WasMoved && !nearAlliedTower.Value.WasMoved 
                && smallRochadeKingPassagePositions.Select(x => board.GetPieceAt(x)).All(x => x == null || x.Value.Type == ChessPieceType.King))
            {
                // make sure that no rochade field can be captured by the opponent
                bool canBigRochade = !enemyCapturablePositions.Any(pos => smallRochadeKingPassagePositions.Contains(pos));

                // add the draw to the list
                if (canBigRochade) { draws.Add(new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 6))); }
            }

            return draws;
        }

        #endregion Methods
    }

    public class QueenChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is a queen
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // combine the positions that a rock or a bishop could capture
            var draws =
                new RockChessDrawGenerator().GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck)
                .Union(new BishopChessDrawGenerator().GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck));

            return draws;
        }

        #endregion Methods
    }

    public class RockChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is rock-like
            if (piece.Type != ChessPieceType.Rook && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rock."); }

            var draws = new List<ChessDraw>();
            
            // get upper-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }
            
            return draws;
        }
        
        #endregion Methods
    }

    public class BishopChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            var draws = new List<ChessDraw>();

            // get upper left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower left-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get upper right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }

            // get lower right-side draws
            for (int i = 1; i < ChessBoard.CHESS_BOARD_DIMENSION; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);
                var pieceAtPos = board.GetPieceAt(newPos);
                bool canDrawToPos = (pieceAtPos == null || pieceAtPos.Value.Color != piece.Color);
                bool abort = pieceAtPos != null;

                if (canDrawToPos)
                {
                    var draw = new ChessDraw(board, drawingPiecePosition, newPos);
                    if (i == 1 && analyzeDrawIntoCheck && new ChessDrawSimulator().IsDrawIntoCheck(board, draw)) { break; }
                    draws.Add(draw);
                }

                if (abort) { break; }
            }
            
            return draws;
        }
        
        #endregion Methods
    }

    public class KnightChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is a knight
            if (piece.Type != ChessPieceType.Knight) { throw new InvalidOperationException("The chess piece is not a knight."); }

            IEnumerable<ChessDraw> draws = new List<ChessDraw>();

            if (analyzeDrawIntoCheck)
            {
                // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
                var coords = new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(drawingPiecePosition.Row - 2, drawingPiecePosition.Column - 1),
                    new Tuple<int, int>(drawingPiecePosition.Row - 2, drawingPiecePosition.Column + 1),
                    new Tuple<int, int>(drawingPiecePosition.Row - 1, drawingPiecePosition.Column - 2),
                    new Tuple<int, int>(drawingPiecePosition.Row - 1, drawingPiecePosition.Column + 2),
                    new Tuple<int, int>(drawingPiecePosition.Row + 1, drawingPiecePosition.Column - 2),
                    new Tuple<int, int>(drawingPiecePosition.Row + 1, drawingPiecePosition.Column + 2),
                    new Tuple<int, int>(drawingPiecePosition.Row + 2, drawingPiecePosition.Column - 1),
                    new Tuple<int, int>(drawingPiecePosition.Row + 2, drawingPiecePosition.Column + 1),
                };

                // only retrieve positions that are actually onto the chess board (and not off scale)
                var positions = coords.Where(x => ChessPosition.AreCoordsValid(x)).Select(x => new ChessPosition(x));

                // do not draw into positions captured by allied chess pieces
                positions = positions.Where(x => !board.IsCapturedAt(x) || board.GetPieceAt(x).Value.Color != piece.Color);

                // transform positions to chess draws
                draws = positions.Select(newPos => new ChessDraw(board, drawingPiecePosition, newPos));

                // remove draws that would draw into a check situation (all moves are invalid if one draws into check)
                draws = !(draws?.Count() > 0 && new ChessDrawSimulator().IsDrawIntoCheck(board, draws.First())) ? draws : new List<ChessDraw>();
            }
            
            return draws;
        }

        #endregion Methods
    }

    public class PeasantChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of field positions</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw, bool analyzeDrawIntoCheck)
        {
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Peasant) { throw new InvalidOperationException("The chess piece is not a peasant."); }

            // get all possible draws
            var draws = getForewardDraws(board, drawingPiecePosition).Union(getCatchDraws(board, drawingPiecePosition, precedingEnemyDraw));

            // handle peasant promotion
            draws = draws.SelectMany(x => {
                
                if ((x.NewPosition.Row == 7 && piece.Color == ChessColor.White) || (x.NewPosition.Row == 0 && piece.Color == ChessColor.Black))
                {
                    var promotionDraws = new List<ChessDraw>();

                    // add options for peasant promotion (all piece types except king and peasant)
                    for (int i = 1; i < 5; i++)
                    {
                        var pieceType = (ChessPieceType)i;
                        promotionDraws.Add(new ChessDraw(board, x.OldPosition, x.NewPosition, pieceType));
                    }

                    return promotionDraws;
                }

                return new List<ChessDraw>() { x };
            });
            
            if (analyzeDrawIntoCheck)
            {
                // remove draws that would draw into a check situation
                draws = draws.Where(x => !new ChessDrawSimulator().IsDrawIntoCheck(board, x));
            }

            return draws;
        }

        private IEnumerable<ChessDraw> getForewardDraws(ChessBoard board, ChessPosition drawingPiecePosition)
        {
            var draws = new List<ChessDraw>();
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            var coordsPosOneForeward = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 1 : -1), drawingPiecePosition.Column);
            var coordsPosTwoForeward = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 2 : -2), drawingPiecePosition.Column);
            
            if (ChessPosition.AreCoordsValid(coordsPosOneForeward))
            {
                var posOneForeward = new ChessPosition(coordsPosOneForeward);
                bool oneForeward = !board.IsCapturedAt(posOneForeward);

                if (oneForeward)
                {
                    draws.Add(new ChessDraw(board, drawingPiecePosition, posOneForeward));

                    if (!piece.WasMoved && ChessPosition.AreCoordsValid(coordsPosTwoForeward))
                    {
                        var posTwoForeward = new ChessPosition(coordsPosTwoForeward);

                        if (!board.IsCapturedAt(posTwoForeward))
                        {
                            draws.Add(new ChessDraw(board, drawingPiecePosition, posTwoForeward));
                        }
                    }
                }
            }
            
            return draws;
        }

        private IEnumerable<ChessDraw> getCatchDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw precedingEnemyDraw)
        {
            var draws = new List<ChessDraw>();
            var piece = board.GetPieceAt(drawingPiecePosition).Value;

            // get the possible chess field positions (info: right / left from the point of view of the white side player)
            var coordsPosCatchLeft  = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 1 : -1), drawingPiecePosition.Column + 1);
            var coordsPosCatchRight = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 1 : -1), drawingPiecePosition.Column - 1);

            // check for en-passant precondition
            bool wasLastDrawPeasantDoubleForeward = precedingEnemyDraw.DrawingPieceType == ChessPieceType.Peasant && Math.Abs(precedingEnemyDraw.OldPosition.Row - precedingEnemyDraw.NewPosition.Row) == 2;

            // check if left catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchLeft))
            {
                var posCatchLeft = new ChessPosition(coordsPosCatchLeft);

                bool catchLeft = board.IsCapturedAt(posCatchLeft) && board.GetPieceAt(posCatchLeft).Value.Color != piece.Color;
                if (catchLeft) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchLeft)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyLeft = new ChessPosition(drawingPiecePosition.Row, drawingPiecePosition.Column + 1);
                    bool isLeftFieldCapturedByEnemy = board.IsCapturedAt(posEnPassantEnemyLeft) && board.GetPieceAt(posEnPassantEnemyLeft).Value.Color != piece.Color;
                    bool enPassantLeft = isLeftFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyLeft.Column - drawingPiecePosition.Column) == 1;
                    if (enPassantLeft) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchLeft)); }
                }
            }

            // check if right catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchRight))
            {
                var posCatchRight = new ChessPosition(coordsPosCatchRight);

                bool catchRight = board.IsCapturedAt(posCatchRight) && board.GetPieceAt(posCatchRight).Value.Color != piece.Color;
                if (catchRight) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchRight)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyRight = new ChessPosition(drawingPiecePosition.Row, drawingPiecePosition.Column - 1);
                    bool isRightFieldCapturedByEnemy = board.IsCapturedAt(posEnPassantEnemyRight) && board.GetPieceAt(posEnPassantEnemyRight).Value.Color != piece.Color;
                    bool enPassantRight = isRightFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyRight.Column - drawingPiecePosition.Column) == 1;
                    if (enPassantRight) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchRight)); }
                }
            }
            
            return draws;
        }

        #endregion Methods
    }
}
