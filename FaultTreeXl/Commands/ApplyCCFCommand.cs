using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ApplyCCFCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is object[] parms)
            if (parms[0] is Decimal lambda && parms[1] is Decimal PTI && parms[2] is GraphicItem node2update)
            {
                node2update.Lambda = lambda;
                node2update.PTI = PTI;
            }
        }
    }

}
