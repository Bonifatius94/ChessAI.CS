using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Chess.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructor

        public App()
        {
            // subscribe to unhandled exceptions event
            DispatcherUnhandledException += handleUncaughtException;

            // initialize component
            InitializeComponent();
        }

        #endregion Constructor

        #region Methods

        private void handleUncaughtException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled exception occurred!\r\n\r\n{ e.Exception.ToString() }");
            e.Handled = true;
        }

        #endregion Methods
    }
}
