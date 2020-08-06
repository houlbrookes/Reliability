using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ORAddCommand : ICommand
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
                var newNode = new OR()
                {
                    Name = $"OR {ftm.NextNodeName("OR") + 1}",
                    Nodes = new ObservableCollection<GraphicItem> {
                        new Node{ Name = $"Node {ftm.NextNodeName("Node") + 1}", Lambda=1E-6M, PTI=8760},
                        new Node{ Name = $"Node {ftm.NextNodeName("Node") + 2}", Lambda=1E-6M, PTI=8760}
                    }
                };
                theGraphic.Nodes.Add(newNode);
                (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
            }
        }
    }


}
