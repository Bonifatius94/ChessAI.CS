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
        #region Constructor

        public MainViewModel()
        {

            _session = new ChessGameSession();

            Board = new ChessBoardViewModel(onChessFieldClicked);
        }

        #endregion Constructor

        #region Members

        private ChessGameSession _session;

        public ChessBoardViewModel Board { get; private set; }

        #region Draw

        private bool _canPlayerDraw;
        private ChessPieceAtPos? _drawingPiece = null;

        #endregion Draw

        #endregion Members

        #region Methods

        private void onChessFieldClicked(object parameter)
        {
            // identify the field that was clicked
            string fieldname = parameter as string;
            var position = new ChessPosition(fieldname);


        }

        // TODO: add a menu bar
        // TODO: implement start game logic etc.

        #endregion Methods
    }
}
