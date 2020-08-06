using System;
using System.Linq;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class PT100Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal PROOF_TEST_EFFECTIVENESS = 1;

            if (parameter is FaultTreeModel faultTreeModel)
            {
                faultTreeModel.ProofTestEffectiveness = PROOF_TEST_EFFECTIVENESS;
                foreach (Node node in faultTreeModel.FaultTree.OfType<Node>())
                {
                    node.ProofTestEffectiveness = PROOF_TEST_EFFECTIVENESS;
                }
                foreach (var node in faultTreeModel.FaultTree) node.LifeTime = faultTreeModel.MissionTime;
                faultTreeModel.ReDrawRootNode();
                faultTreeModel.Status = $"Global Proof Test Effectiveness Updated to: {PROOF_TEST_EFFECTIVENESS:P0}";
            }
        }
    }

    internal class PT90Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal PROOF_TEST_EFFECTIVENESS = (decimal)0.9;
            if (parameter is FaultTreeModel faultTreeModel)
            {
                faultTreeModel.ProofTestEffectiveness = PROOF_TEST_EFFECTIVENESS;
                foreach (Node node in faultTreeModel.FaultTree.OfType<Node>())
                {
                    node.ProofTestEffectiveness = PROOF_TEST_EFFECTIVENESS;
                }
                foreach (var node in faultTreeModel.FaultTree) node.LifeTime = faultTreeModel.MissionTime;
                faultTreeModel.ReDrawRootNode();
                faultTreeModel.Status = $"Global Proof Test Effectiveness Updated to: {PROOF_TEST_EFFECTIVENESS:P0}";
            }
        }
    }

    internal class Life5YearsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal LIFETIME = 8760 * 5;
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

    internal class Life10YearsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal LIFETIME = 8760 * 10;
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

    internal class Life15YearsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

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

    internal class Life20YearsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            const decimal LIFETIME = 8760 * 20;
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
