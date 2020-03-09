using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.UI.Field
{
    public class ChessFieldViewModel : PropertyChangedBase
    {
        #region Constructor

        /// <summary>
        /// Create a new instance of a chess field view model of the given position onto the chess board and the overloaded click handler.
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="onClickCallback">The click handler that is bound to a click event of the field.</param>
        public ChessFieldViewModel(ChessPosition position, Action<object> onClickCallback)
        {
            Position = position;
            OnClickCommand = new DelegateCommand(onClickCallback);
        }

        #endregion Constructor

        #region Members

        private ChessPiece? _piece = null;
        private bool _isHighlighted = false;

        public ChessPosition Position { get; private set; }

        public Brush FieldBackground { get { return _isHighlighted ? Brushes.LightYellow : (Position.ColorOfField == ChessColor.White ? Brushes.White : Brushes.Gray); } }

        public string PieceText { get { return prepareChessPieceChar(_piece); } }

        public ICommand OnClickCommand { get; private set; }
        public string OnClickCommandParameter { get { return Position.FieldName; } }

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

        /// <summary>
        /// Update the chess piece of this field (piece=null: the field is uncaptured). The displayed piece text is changed accordingly.
        /// </summary>
        /// <param name="piece">The new chess piece (or null).</param>
        public void UpdatePiece(ChessPiece? piece)
        {
            _piece = piece;
            NotifyPropertyChanged(nameof(PieceText));
        }

        /// <summary>
        /// Update the highlighting status of this field. The displayed field background is changed accordingly.
        /// </summary>
        /// <param name="isHighlighted"></param>
        public void UpdateHighlight(bool isHighlighted)
        {
            _isHighlighted = isHighlighted;
            NotifyPropertyChanged(nameof(FieldBackground));
        }

        #endregion Methods
    }
}
