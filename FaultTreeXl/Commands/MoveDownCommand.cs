using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class MoveDownCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem theGraphicItem)
            {
                if (!(theGraphicItem.Parent is null))
                {
                    var pos = theGraphicItem.Parent.Nodes.IndexOf(theGraphicItem);
                    if (pos > 0)
                    {
                        theGraphicItem.Parent.Nodes.Remove(theGraphicItem);
                        theGraphicItem.Parent.Nodes.ElementAt(pos - 1).Nodes.Add(theGraphicItem);
                    }

                    (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
                }
            }
        }
    }
}
