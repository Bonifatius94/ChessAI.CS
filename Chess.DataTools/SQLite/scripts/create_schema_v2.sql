
-- MIT License
--
-- Copyright(c) 2020 Marco Tröster
--
-- Permission is hereby granted, free of charge, to any person obtaining a copy
-- of this software and associated documentation files (the "Software"), to deal
-- in the Software without restriction, including without limitation the rights
-- to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
-- copies of the Software, and to permit persons to whom the Software is
-- furnished to do so, subject to the following conditions:
-- 
-- The above copyright notice and this permission notice shall be included in all
-- copies or substantial portions of the Software.
-- 
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
-- IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
-- FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
-- AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
-- LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
-- OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
-- SOFTWARE.

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