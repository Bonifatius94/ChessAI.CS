
-- ==================================================
--         BEST DRAWS CACHE DATABASE SCHEMA
-- ==================================================

-- --------------------------------------------------
--                 DATABASE TABLES
-- --------------------------------------------------

CREATE TABLE WinRateInfo (

    -- columns
    InfoId INTEGER PRIMARY KEY AUTOINCREMENT,
    DrawHash CHAR(6) NOT NULL,
    DrawHashNumeric INTEGER NOT NULL,
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

-- enforce the win rate infos collection to be unique for each (draw, board before) tuple
CREATE UNIQUE INDEX WinRateInfo_DrawXBoardBefore ON WinRateInfo (DrawHash, BoardBeforeHash);
CREATE INDEX WinRateInfo_DrawHash ON WinRateInfo (DrawHash);
CREATE INDEX WinRateInfo_BoardBeforeHash ON WinRateInfo (BoardBeforeHash);

-- --------------------------------------------------
--                  DATABASE VIEWS
-- --------------------------------------------------

-- visualize data from draw hash
CREATE VIEW DrawHashMetadata AS
SELECT 
    DrawHash,
    CASE WHEN (DrawHashNumeric % 16777216 / 8388608) = 0 THEN 'w' ELSE 'b' END AS DrawingSide,
    CASE 
         WHEN (DrawHashNumeric %  8388608 / 2097152) = 0 THEN 'Standard'
         WHEN (DrawHashNumeric %  8388608 / 2097152) = 1 THEN 'Rochade'
         WHEN (DrawHashNumeric %  8388608 / 2097152) = 2 THEN 'En Passant'
         WHEN (DrawHashNumeric %  8388608 / 2097152) = 3 THEN 'Peasant Promotion'
    END AS DrawType,
    CASE 
         WHEN (DrawHashNumeric %  2097152 /  262144) = 0 THEN 'Null'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 1 THEN 'King'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 2 THEN 'Queen'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 3 THEN 'Rook'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 4 THEN 'Bishop'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 5 THEN 'Knight'
         WHEN (DrawHashNumeric %  2097152 /  262144) = 6 THEN 'Peasant'
    END AS DrawingPieceType,
    CASE 
         WHEN (DrawHashNumeric %   262144 /   32768) = 0 THEN 'Null'
         WHEN (DrawHashNumeric %   262144 /   32768) = 1 THEN 'King'
         WHEN (DrawHashNumeric %   262144 /   32768) = 2 THEN 'Queen'
         WHEN (DrawHashNumeric %   262144 /   32768) = 3 THEN 'Rook'
         WHEN (DrawHashNumeric %   262144 /   32768) = 4 THEN 'Bishop'
         WHEN (DrawHashNumeric %   262144 /   32768) = 5 THEN 'Knight'
         WHEN (DrawHashNumeric %   262144 /   32768) = 6 THEN 'Peasant'
    END AS TakenPieceType,
    CASE 
         WHEN (DrawHashNumeric %    32768 /    4096) = 0 THEN 'Null'
         WHEN (DrawHashNumeric %    32768 /    4096) = 1 THEN 'King'
         WHEN (DrawHashNumeric %    32768 /    4096) = 2 THEN 'Queen'
         WHEN (DrawHashNumeric %    32768 /    4096) = 3 THEN 'Rook'
         WHEN (DrawHashNumeric %    32768 /    4096) = 4 THEN 'Bishop'
         WHEN (DrawHashNumeric %    32768 /    4096) = 5 THEN 'Knight'
         WHEN (DrawHashNumeric %    32768 /    4096) = 6 THEN 'Peasant'
    END AS PeasantPromotionPieceType,
    char((DrawHashNumeric % 512 / 64) + unicode('A')) || char((DrawHashNumeric % 4096 / 512) + unicode('1')) AS OldPosition,
    char((DrawHashNumeric %   8     ) + unicode('A')) || char((DrawHashNumeric %   64 /   8) + unicode('1')) AS NewPosition
FROM (SELECT DISTINCT DrawHash, DrawHashNumeric FROM WinRateInfo);

-- --------------------------------------------------
--             AFTER SCRIPT INSTRUCTIONS
-- --------------------------------------------------

-- set schema version (this simplifies the maintenance lateron)
PRAGMA schema_version = 1;

-- ==================================================
--            Marco Tröster, 2020-02-20
-- ==================================================