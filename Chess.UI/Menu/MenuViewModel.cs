/*
 * MIT License
 * 
 * Copyright(c) 2020 Marco Tröster
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
