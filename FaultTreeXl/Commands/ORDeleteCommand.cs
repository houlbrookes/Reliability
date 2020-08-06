using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ORDeleteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem theOR)
            {
                if (!(theOR.Parent is null))
                {
                    var res = MessageBox.Show(
                        "Are you sure?", 
                        "Delete Object", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Exclamation, 
                        MessageBoxResult.No);
                    if (res == MessageBoxResult.Yes)
                    {
                        theOR.Parent.Nodes.Remove(theOR);
                        (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
                    }
                }
            }
        }
    }
}
