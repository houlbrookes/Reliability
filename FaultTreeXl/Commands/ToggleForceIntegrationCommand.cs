using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ToggleForceIntegrationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel fta)
            {
                var newValue = !fta.RootNode.ForceIntegration;
                foreach(var node in fta.FaultTree)
                {
                    node.ForceIntegration = newValue;
                }
            }
        }
    }

}
