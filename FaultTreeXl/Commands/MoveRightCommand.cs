using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class MoveRightCommand : ICommand
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
            if (parameter is GraphicItem theGraphicItem)
            {
                if (!(theGraphicItem.Parent is null))
                {
                    var nodeList = theGraphicItem.Parent.Nodes;
                    var pos = nodeList.IndexOf(theGraphicItem);
                    if ((pos+1)<nodeList.Count())
                    {
                        nodeList.Remove(theGraphicItem);
                        nodeList.Add(theGraphicItem); //Add to the end
                    }

                    (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
                }
            }
        }
    }
}
