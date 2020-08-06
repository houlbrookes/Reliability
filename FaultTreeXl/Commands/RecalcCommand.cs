﻿using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    class RecalcCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel ftm)
            {
                ftm.ReDrawRootNode();
                ftm.Status = "Recalculated";
            }
        }
    }


}
