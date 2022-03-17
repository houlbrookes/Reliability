using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class LaunchSFFWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var theWindow = new ArchConstr();
            theWindow.ShowDialog();
        }
    }

}
