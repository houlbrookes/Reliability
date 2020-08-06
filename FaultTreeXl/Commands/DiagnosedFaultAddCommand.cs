using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Text.RegularExpressions;

namespace FaultTreeXl
{
    class DiagnosedFaultAddCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem theGraphic)
            {
                var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;

                var newNode = new DiagnosedFaultNode() {
                    Name = $"Node {ftm.NextNodeName("Node")+1}",
                    Lambda = 1E-6M,
                    PTI = 8M,
                };
                theGraphic.Nodes.Add(newNode);
                ftm.ReDrawRootNode();
            }
        }
    }

}
