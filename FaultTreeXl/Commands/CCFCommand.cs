using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class CCFCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is Node node)
            {
                var theWindow = new CCFWindow();
                theWindow.Model = (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel);
                theWindow.Node = node;
                theWindow.ShowDialog();
            }
        }
    }
}
