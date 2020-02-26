using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    // TODO: use multiple threads to calculate the possible draws (=> already tested, but was actually slower than the single-thread approach)

    /// <summary>
    /// An interface specifying operations for generating chess draws.
    /// </summary>
    public interface IChessDrawGenerator
    {
        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of all possible chess draws</returns>
        IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false);
    }

    /// <summary>
    /// A generic chess draw generator that works for all chess piece types.
    /// </summary>
    public class ChessDrawGenerator : IChessDrawGenerator
    {
        #region Singleton

        // flag constructor private to avoid objects being generated other than the singleton instance
        private ChessDrawGenerator() { }

        /// <summary>
        /// Get of singleton object reference.
        /// </summary>
        public static readonly IChessDrawGenerator Instance = new ChessDrawGenerator();

        #endregion Singleton

        #region Members

        // chess draw generators (one per chess piece type)
        private static readonly Dictionary<ChessPieceType, IChessDrawGenerator> _explicitGenerators = new Dictionary<ChessPieceType, IChessDrawGenerator>()
        {
            { ChessPieceType.King,    new KingChessDrawGenerator()    },
            { ChessPieceType.Queen,   new QueenChessDrawGenerator()   },
            { ChessPieceType.Rook,    new RookChessDrawGenerator()    },
            { ChessPieceType.Bishop,  new BishopChessDrawGenerator()  },
            { ChessPieceType.Knight,  new KnightChessDrawGenerator()  },
            { ChessPieceType.Peasant, new PeasantChessDrawGenerator() }
        };

        #endregion Members

        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            // determine the drawing piece and the required draw generator
            var piece = board.GetPieceAt(drawingPiecePosition);
            var generator = _explicitGenerators[piece.Type];

            // compute all possible chess draws for the given chess piece
            var draws = generator.GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck).ToArray();
            
            return draws;
        }
        
        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for king chess pieces.
    /// </summary>
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
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.King) { throw new InvalidOperationException("The chess piece is not a king."); }

            // get the possible draw positions
            var positions = getStandardDrawPositions(drawingPiecePosition);

            // only retrieve positions that are not captured by an allied chess piece
            var alliedPieces = board.GetPiecesOfColor(piece.Color);
            positions = positions.Except(alliedPieces.Select(x => x.Position));

            // only retrieve positions that cannot be captured by the enemy king (-> avoid draw into check)
            var enemyKing = piece.Color == ChessColor.White ? board.BlackKing : board.WhiteKing;
            var enemyKingDrawPositons = getStandardDrawPositions(enemyKing.Position);
            positions = positions.Except(enemyKingDrawPositons);
            
            // transform positions to draws
            IEnumerable<ChessDraw> draws = positions.Select(newPos => new ChessDraw(board, drawingPiecePosition, newPos)).ToArray();

            // analyze draw into a check situation
            if (analyzeDrawIntoCheck && draws?.Count() > 0) { draws = draws.Where(draw => !ChessDrawSimulator.Instance.IsDrawIntoCheck(board, draw)).ToArray(); }

            // add rochade draws
            bool canRochade = (drawingPiecePosition == new ChessPosition("E1") || drawingPiecePosition == new ChessPosition("E8"));
            if (canRochade) { draws = draws.Concat(getRochadeDraws(board, piece.Color)); }
            
            return draws;
        }

        private IEnumerable<ChessPosition> getStandardDrawPositions(ChessPosition position)
        {
            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var coords = new Tuple<int, int>[]
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

        // the rochade passage positions of the king
        private static readonly ChessPosition[] whiteKingBigRochadePassagePositions   = new ChessPosition[] { new ChessPosition(0, 2), new ChessPosition(0, 3), new ChessPosition(0, 4) };
        private static readonly ChessPosition[] whiteKingSmallRochadePassagePositions = new ChessPosition[] { new ChessPosition(0, 4), new ChessPosition(0, 5), new ChessPosition(0, 6) };
        private static readonly ChessPosition[] blackKingBigRochadePassagePositions   = new ChessPosition[] { new ChessPosition(7, 2), new ChessPosition(7, 3), new ChessPosition(7, 4) };
        private static readonly ChessPosition[] blackKingSmallRochadePassagePositions = new ChessPosition[] { new ChessPosition(7, 4), new ChessPosition(7, 5), new ChessPosition(7, 6) };

        private IEnumerable<ChessDraw> getRochadeDraws(ChessBoard board, ChessColor drawingSide)
        {
            // get enemy capturable positions
            var enemyKing = (drawingSide == ChessColor.White) ? board.BlackKing : board.WhiteKing;
            var enemyCapturablePositions =
                board.GetPiecesOfColor(drawingSide.Opponent()).Where(x => x.Piece.Type != ChessPieceType.King)
                .SelectMany(x => ChessDrawGenerator.Instance.GetDraws(board, x.Position, null, false)).Select(x => x.NewPosition)
                .Concat(getStandardDrawPositions(enemyKing.Position));

            // get the allied king and towers
            int row = (drawingSide == ChessColor.White) ? 0 : 7;
            var alliedKing = (drawingSide == ChessColor.White) ? board.WhiteKing : board.BlackKing;
            var farAlliedTowerPos = new ChessPosition(row, 0);
            var farAlliedTower = board.GetPieceAt(farAlliedTowerPos);
            var nearAlliedTowerPos = new ChessPosition(row, 7);
            var nearAlliedTower = board.GetPieceAt(nearAlliedTowerPos);

            // define the fields that must not be capturable by the opponent
            var bigRochadeKingPassagePositions   = (drawingSide == ChessColor.White) ? whiteKingBigRochadePassagePositions   : blackKingBigRochadePassagePositions;
            var smallRochadeKingPassagePositions = (drawingSide == ChessColor.White) ? whiteKingSmallRochadePassagePositions : blackKingSmallRochadePassagePositions;

            bool canBigRochade = 
                // check for preconditions of big rochade
                (board.IsCapturedAt(farAlliedTowerPos) && !alliedKing.Piece.WasMoved && !farAlliedTower.WasMoved 
                && bigRochadeKingPassagePositions.All(x => !board.IsCapturedAt(x) || (board.GetPieceAt(x).Color == drawingSide && board.GetPieceAt(x).Type == ChessPieceType.King))
                && !board.IsCapturedAt(new ChessPosition(row, 1)))
                // make sure that no rochade field can be captured by the opponent
                && !enemyCapturablePositions.Any(pos => bigRochadeKingPassagePositions.Contains(pos));

            bool canSmallRochade = 
                // check for preconditions of small rochade
                (board.IsCapturedAt(nearAlliedTowerPos) && !alliedKing.Piece.WasMoved && !nearAlliedTower.WasMoved 
                && smallRochadeKingPassagePositions.All(x => !board.IsCapturedAt(x) || (board.GetPieceAt(x).Color == drawingSide && board.GetPieceAt(x).Type == ChessPieceType.King)))
                // make sure that no rochade field can be captured by the opponent and the rochade field on the B-line is not captured
                && !enemyCapturablePositions.Any(pos => smallRochadeKingPassagePositions.Contains(pos));

            // write result draws
            int index = 0;
            int drawsCount = (canSmallRochade ? 1 : 0) + (canBigRochade ? 1 : 0);
            ChessDraw[] draws = new ChessDraw[drawsCount];
            if (canSmallRochade) { draws[index++] = new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 6)); }
            if (canBigRochade) { draws[index++] = new ChessDraw(board, alliedKing.Position, new ChessPosition(row, 2)); }

            return draws;
        }

        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for queen chess pieces.
    /// </summary>
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
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is a queen
            if (piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a queen."); }

            // combine the positions that a rock or a bishop could capture
            var draws =
                new RookChessDrawGenerator().GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck)
                .Union(new BishopChessDrawGenerator().GetDraws(board, drawingPiecePosition, precedingEnemyDraw, analyzeDrawIntoCheck));

            return draws;
        }

        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for rook chess pieces.
    /// </summary>
    public class RookChessDrawGenerator : IChessDrawGenerator
    {
        #region Methods

        /// <summary>
        /// Compute the field positions that can be captured by the given chess piece.
        /// </summary>
        /// <param name="board">The chess board representing the game situation</param>
        /// <param name="drawingPiecePosition">The chess position of the chess piece to be drawn</param>
        /// <param name="precedingEnemyDraw">The last draw made by the opponent</param>
        /// <param name="analyzeDrawIntoCheck">Indicates whether drawing into a check situation should be analyzed</param>
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is rook-like
            if (piece.Type != ChessPieceType.Rook && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a rook."); }

            var draws = new List<ChessDraw>();
            
            // get upper-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get lower-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get right-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get left-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // analyze draw into a check situation
            if (analyzeDrawIntoCheck && draws?.Count() > 0)
            {
                draws = draws.Where(draw => !ChessDrawSimulator.Instance.IsDrawIntoCheck(board, draw)).ToList();
            }

            return draws;
        }
        
        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for bishop chess pieces.
    /// </summary>
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
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is bishop-like
            if (piece.Type != ChessPieceType.Bishop && piece.Type != ChessPieceType.Queen) { throw new InvalidOperationException("The chess piece is not a bishop."); }

            var draws = new List<ChessDraw>();

            // get upper left-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get lower left-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column - i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get upper right-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row + i, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // get lower right-side draws
            for (int i = 1; i < 8; i++)
            {
                // get position and make sure it is valid (otherwise exit loop)
                var coords = new Tuple<int, int>(drawingPiecePosition.Row - i, drawingPiecePosition.Column + i);
                if (!ChessPosition.AreCoordsValid(coords)) { break; }

                var newPos = new ChessPosition(coords);

                if (!board.IsCapturedAt(newPos) || board.GetPieceAt(newPos).Color != piece.Color) { draws.Add(new ChessDraw(board, drawingPiecePosition, newPos)); }
                if (board.IsCapturedAt(newPos)) { break; }
            }

            // analyze draw into a check situation
            if (analyzeDrawIntoCheck && draws?.Count() > 0)
            {
                draws = draws.Where(draw => !ChessDrawSimulator.Instance.IsDrawIntoCheck(board, draw)).ToList();
            }
            
            return draws;
        }
        
        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for knight chess pieces.
    /// </summary>
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
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is a knight
            if (piece.Type != ChessPieceType.Knight) { throw new InvalidOperationException("The chess piece is not a knight."); }
            
            // get positions next to the current position of the king (all permutations of { -1, 0, +1 }^2 except (0, 0))
            var coords = new Tuple<int, int>[]
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
            positions = positions.Where(x => !board.IsCapturedAt(x) || board.GetPieceAt(x).Color != piece.Color);

            // transform positions to chess draws
            var draws = positions.Select(newPos => new ChessDraw(board, drawingPiecePosition, newPos));

            if (analyzeDrawIntoCheck && draws?.Count() > 0)
            {
                // remove draws that would draw into a check situation
                draws = draws.Where(x => !ChessDrawSimulator.Instance.IsDrawIntoCheck(board, x));
            }
            
            return draws;
        }

        #endregion Methods
    }

    /// <summary>
    /// A chess draw generator for peasant chess pieces.
    /// </summary>
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
        /// <returns>a list of all possible chess draws</returns>
        public IEnumerable<ChessDraw> GetDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null, bool analyzeDrawIntoCheck = false)
        {
            var piece = board.GetPieceAt(drawingPiecePosition);

            // make sure the chess piece is a king
            if (piece.Type != ChessPieceType.Peasant) { throw new InvalidOperationException("The chess piece is not a peasant."); }

            // get all possible draws
            var draws = getForewardDraws(board, drawingPiecePosition).Union(getCatchDraws(board, drawingPiecePosition, precedingEnemyDraw));

            // handle peasant promotion
            draws = draws.SelectMany(x => {
                
                // check if the chess draw is a peasant promotion
                if ((x.NewPosition.Row == 7 && piece.Color == ChessColor.White) || (x.NewPosition.Row == 0 && piece.Color == ChessColor.Black))
                {
                    // return all 4 promotion options for the input draw
                    return new ChessDraw[]
                    {
                        new ChessDraw(board, x.OldPosition, x.NewPosition, ChessPieceType.Queen),
                        new ChessDraw(board, x.OldPosition, x.NewPosition, ChessPieceType.Rook),
                        new ChessDraw(board, x.OldPosition, x.NewPosition, ChessPieceType.Bishop),
                        new ChessDraw(board, x.OldPosition, x.NewPosition, ChessPieceType.Knight),
                    };
                }

                // return the input draw unchanged
                return new ChessDraw[] { x };
            });
            
            if (analyzeDrawIntoCheck && draws?.Count() > 0)
            {
                // remove draws that would draw into a check situation
                draws = draws.Where(x => !ChessDrawSimulator.Instance.IsDrawIntoCheck(board, x));
            }

            return draws;
        }

        private IEnumerable<ChessDraw> getForewardDraws(ChessBoard board, ChessPosition drawingPiecePosition)
        {
            var draws = new List<ChessDraw>();
            var piece = board.GetPieceAt(drawingPiecePosition);

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

        private IEnumerable<ChessDraw> getCatchDraws(ChessBoard board, ChessPosition drawingPiecePosition, ChessDraw? precedingEnemyDraw = null)
        {
            var draws = new List<ChessDraw>();
            var piece = board.GetPieceAt(drawingPiecePosition);

            // get the possible chess field positions (info: right / left from the point of view of the white side player)
            var coordsPosCatchLeft  = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 1 : -1), drawingPiecePosition.Column + 1);
            var coordsPosCatchRight = new Tuple<int, int>(drawingPiecePosition.Row + (piece.Color == ChessColor.White ? 1 : -1), drawingPiecePosition.Column - 1);

            // check for en-passant precondition
            bool wasLastDrawPeasantDoubleForeward =
                precedingEnemyDraw != null && precedingEnemyDraw.Value.DrawingPieceType == ChessPieceType.Peasant 
                && Math.Abs(precedingEnemyDraw.Value.OldPosition.Row - precedingEnemyDraw.Value.NewPosition.Row) == 2;

            // check if left catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchLeft))
            {
                var posCatchLeft = new ChessPosition(coordsPosCatchLeft);

                bool catchLeft = board.IsCapturedAt(posCatchLeft) && board.GetPieceAt(posCatchLeft).Color != piece.Color;
                if (catchLeft) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchLeft)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyLeft = new ChessPosition(drawingPiecePosition.Row, drawingPiecePosition.Column + 1);

                    bool isLeftFieldCapturedByEnemy = 
                        precedingEnemyDraw.Value.NewPosition == posEnPassantEnemyLeft && board.IsCapturedAt(posEnPassantEnemyLeft) 
                        && board.GetPieceAt(posEnPassantEnemyLeft).Color != piece.Color;

                    bool enPassantLeft = isLeftFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyLeft.Column - drawingPiecePosition.Column) == 1;
                    if (enPassantLeft) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchLeft)); }
                }
            }

            // check if right catch / en-passant is possible
            if (ChessPosition.AreCoordsValid(coordsPosCatchRight))
            {
                var posCatchRight = new ChessPosition(coordsPosCatchRight);

                bool catchRight = board.IsCapturedAt(posCatchRight) && board.GetPieceAt(posCatchRight).Color != piece.Color;
                if (catchRight) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchRight)); }

                if (wasLastDrawPeasantDoubleForeward)
                {
                    // get the positions of an enemy chess piece taken by a possible en-passant
                    var posEnPassantEnemyRight = new ChessPosition(drawingPiecePosition.Row, drawingPiecePosition.Column - 1);
                    
                    bool isRightFieldCapturedByEnemy =
                        precedingEnemyDraw.Value.NewPosition == posEnPassantEnemyRight
                        && board.IsCapturedAt(posEnPassantEnemyRight) && board.GetPieceAt(posEnPassantEnemyRight).Color != piece.Color;
                    
                    bool enPassantRight = isRightFieldCapturedByEnemy && Math.Abs(posEnPassantEnemyRight.Column - drawingPiecePosition.Column) == 1;
                    if (enPassantRight) { draws.Add(new ChessDraw(board, drawingPiecePosition, posCatchRight)); }
                }
            }
            
            return draws;
        }

        #endregion Methods
    }
}
