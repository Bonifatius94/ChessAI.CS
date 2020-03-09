using Chess.GameLib;
using Chess.Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Chess.UI.NewGame
{
    public enum DrawingSideMode
    {
        White,
        Black,
        Random
    }

    public class NewGameViewModel : PropertyChangedBase
    {
        #region Constructor

        // TODO: add documentation

        public NewGameViewModel(Window dialog)
        {
            _dialog = dialog;

            // init commands
            ConfirmDataCommand = new DelegateCommand(onDataConfirmed);
            CancelCommand = new DelegateCommand(onCancel);
        }

        #endregion Constructor

        #region Members

        private Window _dialog = null;

        public ICommand ConfirmDataCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #region DrawingSide

        private static readonly Random _random = new Random();

        public ChessColor DrawingSide { get; private set; }

        public ObservableCollection<DrawingSideMode> DrawingSideModes { get; private set; } 
            = new ObservableCollection<DrawingSideMode>(
                new List<DrawingSideMode>() { 
                    DrawingSideMode.White, 
                    DrawingSideMode.Black, 
                    DrawingSideMode.Random 
                }
            );

        private DrawingSideMode _selectedDrawingSideMode = DrawingSideMode.Random;
        public DrawingSideMode SelectedDrawingSideMode
        {
            get { return _selectedDrawingSideMode; }
            set
            {
                _selectedDrawingSideMode = value;
                NotifyPropertyChanged(nameof(SelectedDrawingSideMode));
            }
        }

        #endregion DrawingSide

        #region Difficulty

        public ObservableCollection<ChessDifficultyLevel> DifficultyModes { get; private set; }
            = new ObservableCollection<ChessDifficultyLevel>(
                new List<ChessDifficultyLevel>() {
                    ChessDifficultyLevel.Random,
                    ChessDifficultyLevel.VeryStupid,
                    ChessDifficultyLevel.Stupid,
                    ChessDifficultyLevel.VeryEasy,
                    ChessDifficultyLevel.Easy,
                    ChessDifficultyLevel.Medium,
                    ChessDifficultyLevel.Hard,
                    ChessDifficultyLevel.VeryHard,
                    ChessDifficultyLevel.Extreme,
                    ChessDifficultyLevel.Godlike
                }
            );

        private ChessDifficultyLevel _difficulty = ChessDifficultyLevel.Easy;
        public ChessDifficultyLevel Difficulty
        {
            get { return _difficulty; }
            set
            {
                _difficulty = value;
                NotifyPropertyChanged(nameof(Difficulty));
            }
        }

        #endregion Difficulty

        #endregion Members

        #region Methods

        private void onDataConfirmed()
        {
            DrawingSide = evaluateDrawingSide();
            _dialog.DialogResult = true;
            _dialog.Close();
        }

        private void onCancel()
        {
            _dialog.DialogResult = false;
            _dialog.Close();
        }

        private ChessColor evaluateDrawingSide()
        {
            switch (_selectedDrawingSideMode)
            {
                case DrawingSideMode.Random: return _random.Next(0, 2) == 0 ? ChessColor.White : ChessColor.Black;
                case DrawingSideMode.White: return ChessColor.White;
                case DrawingSideMode.Black: return ChessColor.Black;
                default: throw new NotImplementedException($"drawing side mode { _selectedDrawingSideMode } is currently unsupported!");
            }
        }

        #endregion Methods
    }
}
