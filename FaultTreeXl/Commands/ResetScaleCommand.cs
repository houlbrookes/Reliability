using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class ResetScaleCommand : ICommand
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
            try
            {
                if (parameter is FaultTreeModel faultTreeModel)
                {
                    faultTreeModel.Scale = 1.0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

}
