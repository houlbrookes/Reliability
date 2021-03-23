using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class CancelSimulationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel model)
            {
                model.simulationProcess?.CancelAsync();
            }
        }

    }

}
