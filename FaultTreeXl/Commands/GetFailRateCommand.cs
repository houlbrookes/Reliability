using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class GetFailRateCommand : ICommand
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
                if (parameter is GraphicItem node)
                {
                    var form = new StandardFailures();
                    form.DataContext = new StandardFailiuresController() { ItemToUpdate = node, CalledFromWindow=form };
                    form.ShowDialog();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }

}
