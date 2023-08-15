using FaultTreeXl.Global;
using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class NodeAddCommand : ICommand
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
            if (parameter is GraphicItem theGraphic)
            {
                var newNode = new Node() {
                    Name = NodeUtils.NextNodeName,
                    Beta = 0,
                };
                theGraphic.Nodes.Add(newNode);
                NodeUtils.ReDrawRootNode();
                // Open up an edit window for this node
                OREdit editingWindow = new OREdit
                {
                    DataContext = newNode,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                editingWindow.ShowDialog();
                newNode.UpdateParent();
            }
        }
    }

}
