using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class RecalcCommand : ICommand
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
            if (parameter is FaultTreeModel ftm)
            {
                ftm.ReDrawRootNode();
                ftm.Notify(nameof(ftm.SILLevelPFD));
                ftm.Notify(nameof(ftm.SILLevelPFH));
                ftm.Status = "Recalculated";
            }
        }
    }

}
