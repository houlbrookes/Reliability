using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class ANDAddCommand : ICommand
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
            FaultTreeModel ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            if (parameter is GraphicItem theGraphic)
            {
                AND newNode = new AND()
                {
                    Name = $"AND {ftm.NextNodeName("AND") + 1}",
                    Nodes = new ObservableCollection<GraphicItem> {}
                };
                theGraphic.Nodes.Add(newNode);
                (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
            }
        }
    }
}
