
-- ==================================================
--         BEST DRAWS CACHE DATABASE SCHEMA
-- ==================================================

-- --------------------------------------------------
--                 DATABASE TABLES
-- --------------------------------------------------

-- CREATE TABLE ChessGame (
-- 
--     -- columns
--     GameId INTEGER PRIMARY KEY AUTOINCREMENT,
--     GameResult CHAR(1) NOT NULL CHECK (GameResult = 'w' OR GameResult = 'b' OR GameResult = 't')--,
--     -- w: white wins, b: black wins, t: tie
--     
--     -- key constraints
--     --PRIMARY KEY (GameId)
-- );
-- 
-- CREATE TABLE ChessBoard (
-- 
--     -- columns
--     BoardId INTEGER PRIMARY KEY AUTOINCREMENT,
--     BoardHash CHAR(80) NOT NULL--,
--     
--     -- key constraints
--     --PRIMARY KEY (BoardId)
-- );
-- 
-- CREATE TABLE ChessDraw (
-- 
--     -- columns
--     DrawId INTEGER PRIMARY KEY AUTOINCREMENT,
--     GameId INTEGER NOT NULL,
--     DrawHash CHAR(6) NOT NULL,
--     BoardBeforeId INTEGER NOT NULL,
--     BoardAfterId INTEGER NOT NULL,
--     DrawingSide CHAR(1) NOT NULL CHECK (DrawingSide = 'w' OR DrawingSide = 'b'),
--     
--     -- key constraints
--     --PRIMARY KEY (DrawId),
--     FOREIGN KEY (GameId) REFERENCES ChessGame(GameId),
--     FOREIGN KEY (BoardBeforeId) REFERENCES ChessBoard(BoardId),
--     FOREIGN KEY (BoardAfterId) REFERENCES ChessBoard(BoardId)
-- );

CREATE TABLE WinRateInfo (

    -- columns
    InfoId INTEGER PRIMARY KEY AUTOINCREMENT,
    DrawHash CHAR(6) NOT NULL,
    BoardBeforeHash CHAR(80) NOT NULL,
    DrawingSide CHAR(1) NOT NULL CHECK (DrawingSide = 'w' OR DrawingSide = 'b'),
    WinRate REAL NOT NULL,
    AnalyzedGames INTEGER NOT NULL--,
    
    -- key constraints
    --PRIMARY KEY (InfoId)
);

-- --------------------------------------------------
--                 DATABASE INDIZES
-- --------------------------------------------------

-- enforce the boards collection to be unique (no duplicates) => all draws with the same boards point to the same id
--CREATE UNIQUE INDEX ChessBoard_BoardHash ON ChessBoard (BoardHash);

-- enforce the win rate infos collection to be unique for each (draw, board before) tuple
--CREATE UNIQUE INDEX WinRateInfo_DrawXBoardBefore ON WinRateInfo (DrawHash, BoardBeforeHash);

-- speed up joins by indexing search columns
--CREATE INDEX ChessDraw_GameId ON ChessDraw (GameId);
--CREATE INDEX ChessDraw_BoardBeforeId ON ChessDraw (BoardBeforeId);
--CREATE INDEX ChessDraw_BoardAfterId ON ChessDraw (BoardAfterId);
--CREATE INDEX ChessDraw_DrawHash ON ChessDraw (DrawHash);
CREATE INDEX WinRateInfo_DrawHash ON WinRateInfo (DrawHash);
CREATE INDEX WinRateInfo_BoardBeforeHash ON WinRateInfo (BoardBeforeHash);

-- --------------------------------------------------
--                  DATABASE VIEWS
-- --------------------------------------------------

-- database view for computing WinRateInfo on chess game data => table WinRateInfo is just a snapshot of the view results
--CREATE VIEW WinRateInfo_Live AS
--SELECT 
--    ChessDraw.DrawHash AS DrawHash,
--    BoardBefore.BoardHash AS BoardBeforeHash,
--    ChessDraw.DrawingSide AS DrawingSide,
--    COUNT(ChessGame.GameResult) * 1.0 / COUNT(*) AS WinRate,
--    COUNT(*) AS AnalyzedGames
--FROM ChessDraw
--INNER JOIN ChessBoard AS BoardBefore ON BoardBefore.BoardId = ChessDraw.BoardBeforeId
--LEFT OUTER JOIN ChessGame ON ChessGame.GameId = ChessDraw.GameId AND ChessGame.GameResult = ChessDraw.DrawingSide
--GROUP BY BoardBefore.BoardHash, ChessDraw.DrawHash, ChessDraw.DrawingSide
--HAVING COUNT(*) >= 5;

-- --------------------------------------------------
--             AFTER SCRIPT INSTRUCTIONS
-- --------------------------------------------------

-- set schema version (this simplifies the maintenance lateron)
PRAGMA schema_version = 1;

-- ==================================================
--            Marco Tröster, 2020-02-20
-- ==================================================