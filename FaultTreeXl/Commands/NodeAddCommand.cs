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
                var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
                var newNode = new Node(){
                    Name = $"Node {ftm.NextNodeName("Node") + 1}",
                    Lambda = 1E-6M,
                    PTI = 8760M,
                };
                theGraphic.Nodes.Add(newNode);
                ftm.ReDrawRootNode();
            }
        }
    }

}
