using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    class ChoosePartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel ftm)
            {
                var form = new StandardParts();
                form.ParentModel = ftm;
                form.Show();
            }
        }
    }
}
