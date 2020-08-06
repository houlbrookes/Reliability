using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FaultTreeXl
{
    internal class OREditCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is object[] theParams)
            {
                if (theParams[0] is GraphicItem theNode && theParams[1] is Window theWindow)
                {
                    OREdit editingWindow = new OREdit()
                    {
                        DataContext = theNode,
                        Owner = theWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    };
                    editingWindow.ShowDialog();
                    theNode.UpdateParent();
                }
            }
        }
    }

}
