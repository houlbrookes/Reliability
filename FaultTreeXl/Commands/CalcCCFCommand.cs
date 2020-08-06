using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class CalcCCFCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel ftm)
            {
                var form = new UserInterface.CalcCCF();
                form.FTA = ftm;
                form.ShowDialog();
            }
        }
    }

}
