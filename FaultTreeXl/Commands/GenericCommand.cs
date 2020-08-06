using System;
using System.Windows.Input;

namespace FaultTreeXl
{
    /// <summary>
    /// A generic command template that can be used in situ
    /// without creating a new class for each command
    /// </summary>
    /// <typeparam name="T">Type of the Parameter passed to the command</typeparam>
    public class GenericCommand<T> : ICommand
    {
        public Func<T, bool> CanExecuteProxy;

        public Action<T> ExecuteProxy;

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            bool? result = true;
            if (parameter is T typedParam)
            {
                result = CanExecuteProxy?.Invoke(typedParam);
            }

            return result == true;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
            {
                if (CanExecuteProxy != null)
                {
                    ExecuteProxy?.Invoke(typedParam);
                }
            }
        }

    }
}
