using Chess.Lib;
using Chess.UI.Board;
using Chess.UI.Field;
using Chess.UI.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.UI.Main
{
    public class MainViewModel
    {
        #region Members

        private ChessGameSession _session;

        public ChessBoardViewModel Board { get; } = new ChessBoardViewModel();

        #endregion Members

        #region Methods

        // TODO: add a menu bar
        // TODO: implement start game logic etc.

        #endregion Methods
    }
}
