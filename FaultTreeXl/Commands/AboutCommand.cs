using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class AboutCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var window = new About();
            window.ShowDialog();
        }
    }
}
