using Chess.UI.Main;
using System.Diagnostics;
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
            // open wikipedia article on chess rules
            openWebsite(@"https://en.wikipedia.org/wiki/Rules_of_chess");
        }

        private void showAbout()
        {
            // open GitHub project home
            openWebsite(@"https://github.com/Bonifatius94/ChessAI.CS#readme");
        }

        private void openWebsite(string url)
        {
            // set Verb property (required for .NET Core)
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" });
        }

        #endregion Methods
    }
}
