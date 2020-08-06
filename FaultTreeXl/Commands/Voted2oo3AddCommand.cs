using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class Voted2oo3AddCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            FaultTreeModel ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            if (parameter is GraphicItem theGraphic)
            {
                Voted2oo3 newNode = new Voted2oo3(
                    new Node { Name = $"Node {ftm.NextNodeName("Node") + 1}", Lambda = 1E-6M, PTI = 8760 },
                    new Node { Name = $"Node {ftm.NextNodeName("Node") + 2}", Lambda = 1E-6M, PTI = 8760 },
                    new Node { Name = $"Node {ftm.NextNodeName("Node") + 3}", Lambda = 1E-6M, PTI = 8760 })
                {
                    Name = $"2oo3 {ftm.NextNodeName("2oo3") + 1}",
                };
                theGraphic.Nodes.Add(newNode);
                (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
            }
        }
    }
}
