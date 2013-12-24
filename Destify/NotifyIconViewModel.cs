using System;
using System.Windows;
using System.Windows.Input;

namespace DestifySharp
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

        /// <summary>
        /// Open history window.
        /// </summary>
        public ICommand ViewHistoryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        History nw = new History();
                        nw.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Open options window.
        /// </summary>
        public ICommand ViewOptionsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Options nw = new Options();
                        nw.Show();
                    }
                };
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}