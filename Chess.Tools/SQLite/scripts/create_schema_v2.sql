
---- ==================================================
----         BEST DRAWS CACHE DATABASE SCHEMA
---- ==================================================

---- --------------------------------------------------
----                 DATABASE TABLES
---- --------------------------------------------------

---- CREATE TABLE ChessDraw (
---- 
----     --DrawId INTEGER PRIMARY KEY AUTOINCREMENT,
----     GameId INTEGER NOT NULL,
----     DrawHash CHAR(6) NOT NULL,
----     BoardBeforeHash CHAR(80) NOT NULL,
----     BoardAfterHash CHAR(80) NOT NULL,
----     DrawingSide CHAR(1) NOT NULL CHECK (DrawingSide = 'w' OR DrawingSide = 'b'),
----     GameResult CHAR(1) NOT NULL CHECK (DrawingSide = 'w' OR DrawingSide = 'b' OR GameResult = 't')
---- );

--CREATE TABLE WinRateInfo (

--    DrawHash CHAR(6) NOT NULL,
--    BoardBeforeHash CHAR(80) NOT NULL,
--    DrawingSide CHAR(1) NOT NULL CHECK (DrawingSide = 'w' OR DrawingSide = 'b'),
--    WinRate REAL NOT NULL,
--    AnalyzedGames INTEGER NOT NULL
--);

---- --------------------------------------------------
----                 DATABASE INDIZES
---- --------------------------------------------------

---- speed up joins by indexing search columns
---- CREATE INDEX ChessDraw_BoardBeforeHash ON ChessDraw (BoardBeforeHash);
---- CREATE INDEX ChessDraw_BoardAfterHash ON ChessDraw (BoardAfterHash);
---- CREATE INDEX ChessDraw_DrawHash ON ChessDraw (DrawHash);
--CREATE INDEX WinRateInfo_DrawHash ON WinRateInfo (DrawHash);
--CREATE INDEX WinRateInfo_BoardBeforeHash ON WinRateInfo (BoardBeforeHash);

---- --------------------------------------------------
----                  DATABASE VIEWS
---- --------------------------------------------------

---- database view for computing WinRateInfo on chess game data => table WinRateInfo is just a snapshot of the view results
---- CREATE VIEW WinRateInfo_Live AS
---- 
---- WITH WinningSidesCountForDraws (
----     SELECT 
----         DrawHash AS DrawHash,
----         BoardBeforeHash AS BoardBeforeHash,
----         DrawingSide AS DrawingSide,
----         WinningSide AS WinningSide,
----         COUNT(*) AS Occurances
----     FROM ChessDraw
----     GROUP BY BoardBeforeHash, DrawHash, DrawingSide, WinningSide
---- )
---- 
---- SELECT 
----     DrawHash AS DrawHash,
----     BoardBeforeHash AS BoardBeforeHash,
----     DrawingSide AS BoardBeforeHash,
----     1.0 * (SELECT Occurances FROM WinningSidesCountForDraws WHERE DrawHash = DrawHash AND BoardBeforeHash = BoardBeforeHash AND DrawingSide = WinningSide) 
----         / (SELECT SUM(Occurances) FROM WinningSidesCountForDraws WHERE DrawHash = DrawHash AND BoardBeforeHash = BoardBeforeHash) AS WinRate,
----     (SELECT SUM(Occurances) FROM WinningSidesCountForDraws WHERE DrawHash = DrawHash AND BoardBeforeHash = BoardBeforeHash) AS AnalyzedGames
---- FROM (
----     SELECT DISTINCT DrawHash, BoardBeforeHash, DrawingSide FROM ChessDraw
---- );

---- --------------------------------------------------
----             AFTER SCRIPT INSTRUCTIONS
---- --------------------------------------------------

---- set schema version (this simplifies the maintenance lateron)
--PRAGMA schema_version = 1;

---- ==================================================
----            Marco Tröster, 2020-02-20
---- ==================================================