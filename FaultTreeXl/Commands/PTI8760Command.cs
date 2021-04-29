using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class PTI8760Command : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Node node)
            {
                node.PTI = 8760M;
            }
        }
    }
}
