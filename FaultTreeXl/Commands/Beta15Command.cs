using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class Beta15Command : ICommand
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
                node.Beta = 15D;
            }
        }
    }

}
