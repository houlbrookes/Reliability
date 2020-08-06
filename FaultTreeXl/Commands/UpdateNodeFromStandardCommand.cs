using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class UpdateNodeFromStandardCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is object[] theParams)
            {
                if (theParams[0] is GraphicItem standard && theParams[1] is FaultTreeModel model2Update)
                {
                    var reply = MessageBox.Show($"Update Node: {model2Update.HighlightedNode.Name} with {standard.Name}", "Are you sue (y/n)", MessageBoxButton.YesNo);
                    if (reply == MessageBoxResult.Yes)
                    {
                        model2Update.HighlightedNode.Lambda = standard.Lambda;
                        model2Update.HighlightedNode.PTI = standard.PTI;
                    }
                }
            }
        }
    }

}
