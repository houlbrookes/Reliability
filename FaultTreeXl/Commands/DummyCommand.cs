using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class DummyCommand : ICommand
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

}
