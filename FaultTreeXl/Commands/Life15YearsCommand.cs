using System;
using System.Linq;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class Life15YearsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal LIFETIME = 8760 * 15;
            if (parameter is FaultTreeModel faultTreeModel)
            {
                faultTreeModel.MissionTime = LIFETIME;
                foreach (Node node in faultTreeModel.FaultTree.OfType<Node>())
                {
                    node.LifeTime = LIFETIME;
                }
                foreach (var node in faultTreeModel.FaultTree) node.LifeTime = faultTreeModel.MissionTime;
                faultTreeModel.ReDrawRootNode();
                faultTreeModel.Status = $"Global Mission Time Updated to: {LIFETIME:N0}";
            }
        }
    }


}
