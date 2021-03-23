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
    internal class EditCommand : ICommand
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
            if (parameter is GraphicItem theNode)
            {
                OREdit editingWindow = new OREdit();
                editingWindow.DataContext = theNode;
                editingWindow.Owner = Application.Current.MainWindow;
                editingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editingWindow.ShowDialog();
                theNode.UpdateParent();
            }
        }
    }

}
