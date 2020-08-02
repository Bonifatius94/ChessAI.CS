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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Chess.UI
{
    #pragma warning disable CS0067

    /// <summary>
    /// An implementation of ICommand for binding onto user input events.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        #region Constructor

        /// <summary>
        /// Create a new delegate command using the given action and an optional check whether the command can be executed.
        /// </summary>
        /// <param name="executeAction">The action to be executed when the event is invoked.</param>
        /// <param name="canExecute">The optional ckeck whether the command can be executed.</param>
        public DelegateCommand(Action executeAction, Func<bool> canExecute = null)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;
            _mode = Mode.Parameterless;
        }

        /// <summary>
        /// Create a new delegate command using the given action and an optional check whether the command can be executed. Both delegates are additionally provided with a command parameter.
        /// </summary>
        /// <param name="executeAction">The action to be executed when the event is invoked.</param>
        /// <param name="canExecute">The optional ckeck whether the command can be executed.</param>
        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute = null)
        {
            _executeActionWithParam = executeAction;
            _canExecuteWithParam = canExecute;
            _mode = Mode.WithParameter;
        }

        #endregion Constructor

        #region Members

        /// <summary>
        /// An enumeration representing the delegate command modes.
        /// </summary>
        private enum Mode { WithParameter, Parameterless }

        /// <summary>
        /// The mode for invoking delegate actions / functions.
        /// </summary>
        private Mode _mode;

        /// <summary>
        /// The action to be executed.
        /// </summary>
        private Action _executeAction = null;

        /// <summary>
        /// The action to be executed.
        /// </summary>
        private Action<object> _executeActionWithParam = null;

        /// <summary>
        /// The function indicating whether the command can execute (optional, default: true).
        /// </summary>
        private Func<bool> _canExecute = null;

        /// <summary>
        /// The function indicating whether the command can execute (optional, default: true).
        /// </summary>
        private Func<object, bool> _canExecuteWithParam = null;

        #endregion Members

        #region Events

        /// <summary>
        /// An event of the ICommand interface that is not used in this implementation.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion Events

        /// <summary>
        /// An ICommand interface method that is called to find out whether the command can be executed.
        /// </summary>
        /// <param name="parameter">The parameter of the command.</param>
        /// <returns>A boolean that indicates whether the command can be executed.</returns>
        public bool CanExecute(object parameter)
        {
            // use the overloaded function for checking whether the command can execute (default is true)
            return _mode == Mode.Parameterless
                ? _canExecute?.Invoke() ?? true
                : _canExecuteWithParam?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// An ICommand interface method that is called when the command was executed and CanExecute method returned true.
        /// </summary>
        /// <param name="parameter">The parameter of the command.</param>
        public void Execute(object parameter)
        {
            // try to invoke the overloaded action
            if (_mode == Mode.Parameterless) { _executeAction?.Invoke(); }
            else { _executeActionWithParam?.Invoke(parameter); }
        }
    }
}
