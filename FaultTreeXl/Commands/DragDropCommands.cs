using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class DragOverCommands : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is string prompt)
                MessageBox.Show(prompt);
        }
    }

    class DragEnterCommands : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is string prompt)
                MessageBox.Show(prompt);
        }
    }

    class DragLeaveCommands : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is string prompt)
                MessageBox.Show(prompt);
        }
    }

    class DropCommands : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is string prompt)
                MessageBox.Show(prompt);
        }
    }

    class MoveMoveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
        }
    }

}
