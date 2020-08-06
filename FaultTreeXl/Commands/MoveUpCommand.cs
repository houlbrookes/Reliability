using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class MoveUpCommand : ICommand
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
                    if (!(theGraphicItem.Parent.Parent is null))
                    {
                        theGraphicItem.Parent.Nodes.Remove(theGraphicItem);
                        theGraphicItem.Parent.Parent.Nodes.Add(theGraphicItem);
                    }
                    (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
                }
            }
        }
    }
}
