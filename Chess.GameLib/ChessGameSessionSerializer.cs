using Chess.GameLib.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.GameLib
{
    public class ChessGameSessionSerializer
    {
        #region Singleton

        private ChessGameSessionSerializer() { }

        public static readonly ChessGameSessionSerializer Instance = new ChessGameSessionSerializer();

        #endregion Singleton

        #region Methods

        public void Serialize(string filePath, ChessGameSession session)
        {
            // TODO: implement logic
        }

        public ChessGameSession Deserialize(string filePath)
        {
            // TODO: implement logic
            return null;
        }

        #endregion Methods
    }
}
