using Chess.UI.Main;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Chess.UI.Menu
{
    public class MenuViewModel
    {
        #region Constructor

        public MenuViewModel(MainViewModel main)
        {
            // init commands
            NewGameCommand = new DelegateCommand(() => main.NewGame());
            ExitCommand = new DelegateCommand(() => System.Windows.Application.Current.Shutdown());
            ShowRulesCommand = new DelegateCommand(() => showRules());
            AboutCommand = new DelegateCommand(() => showAbout());
        }

        #endregion Constructor

        #region Members

        public ICommand NewGameCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand ShowRulesCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        #endregion Members

        #region Methods

        private void showRules()
        {
            // TODO: implement logic
        }

        private void showAbout()
        {

        }

        #endregion Methods
    }
}
