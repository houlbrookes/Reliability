﻿using System;
using System.Windows;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class ShowCutSetsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is GraphicItem node)
                {
                    //var CCF = node.CutSetsAsString;
                    //MessageBox.Show(CCF, "Minimal Cut Sets");
                    var frm = new UserInterface.CutSetGrid();
                    var dc = new UserInterface.CutSetGridContext();
                    dc.CutSets = node.CutSets;
                    frm.DataContext = dc;
                    frm.ShowDialog();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error!");
            }
        }
    }
}
