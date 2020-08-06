using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ToggleLifeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel model)
            {
                model.ShowLifeInfo = !model.ShowLifeInfo;
                foreach(GraphicItem node in model.FaultTree)
                {
                    node.ShowLifeInfo = model.ShowLifeInfo;
                }
            }
        }
    }
}
