using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;

namespace FaultTreeXl
{
    class RunSimulationCommand : ICommand
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
                model.StartSimulation();
            }
        }

    }

}
