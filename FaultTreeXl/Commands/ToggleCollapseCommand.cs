using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class ToggleCollapseCommand : ICommand
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
            if (parameter is GraphicItem theOR)
            {
                if (theOR.Parent is null)
                {
                    MessageBox.Show("You cannot collapse the root node");
                }
                else
                {
                    theOR.Collapsed = ! theOR.Collapsed;
                    (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
                }
            }
        }
    }

}
