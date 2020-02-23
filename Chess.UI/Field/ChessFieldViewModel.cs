using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Chess.UI.Field
{
    public class ChessFieldViewModel : PropertyChangedBase
    {
        #region Constructor

        public ChessFieldViewModel(ChessPosition position) { _position = position; }

        #endregion Constructor

        #region Members

        private ChessPosition _position;
        private ChessPiece? _piece = null;

        public Brush FieldBackground { get { return _position.ColorOfField == ChessColor.White ? Brushes.White : Brushes.Gray; } }

        public string PieceText { get { return prepareChessPieceChar(_piece); } }

        #endregion Members

        #region Methods

        private string prepareChessPieceChar(ChessPiece? piece)
        {
            // return empty string if there is not chess piece onto the field
            if (piece == null) { return string.Empty; }

            // return the unicode char of the chess piece (color + type)
            switch (piece.Value.Color)
            {
                case ChessColor.White:
                    switch (piece.Value.Type)
                    {
                        case ChessPieceType.King:    return "\u2654";
                        case ChessPieceType.Queen:   return "\u2655";
                        case ChessPieceType.Rook:    return "\u2656";
                        case ChessPieceType.Bishop:  return "\u2657";
                        case ChessPieceType.Knight:  return "\u2658";
                        case ChessPieceType.Peasant: return "\u2659";
                        default: throw new ArgumentException();
                    }
                default:
                    switch (piece.Value.Type)
                    {
                        case ChessPieceType.King:    return "\u265A";
                        case ChessPieceType.Queen:   return "\u265B";
                        case ChessPieceType.Rook:    return "\u265C";
                        case ChessPieceType.Bishop:  return "\u265D";
                        case ChessPieceType.Knight:  return "\u265E";
                        case ChessPieceType.Peasant: return "\u265F";
                        default: throw new ArgumentException();
                    }
            }
        }

        public void UpdatePiece(ChessPiece? piece)
        {
            _piece = piece;
            NotifyPropertyChanged(nameof(PieceText));
        }

        #endregion Methods
    }
}
