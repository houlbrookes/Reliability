﻿using System;
using System.Linq;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class PT100Command : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

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


}
