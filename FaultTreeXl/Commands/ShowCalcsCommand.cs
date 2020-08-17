using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Visio;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace FaultTreeXl
{
    internal class ShowCalcsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        private void ShowCutSetCalcs(GraphicItem theItem)
        {
            var theApp = new Application();
            theApp.Visible = true;
            var theWorkBook = theApp.Workbooks.Add(System.Reflection.Missing.Value);
            _Worksheet theWorkSheet = theWorkBook.ActiveSheet;
            var cutSets = theItem.CutSets;
            var row = 3;

            Func<int, int, string> addr = (r, c) =>
                theWorkSheet.Cells[r, c].Address(false, false);

            var cutset_qty = cutSets.Count;
            var max_cutset_length = cutSets.Max(cs => cs.Count);
            var col_offset = max_cutset_length + 1;
            Range r1 = theWorkSheet.UsedRange;

            Func<int, int, int, int, Range> GetRange = (row1, col1, row2, col2) =>
            {
                var fromAddress = $"{addr(row1, col1)}";
                var toAddress = $"{addr(row2, col2)}";
                return theWorkSheet.Range[fromAddress, toAddress];
            };

            GetRange(1, 1, 1, max_cutset_length * 2 + 3).MergeCells = true;
            theWorkSheet.Cells[1, 1] = theItem.Name;
            theWorkSheet.Cells[1, 1].Font.Bold = true;
            theWorkSheet.Cells[1, 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            //theWorkSheet.Cells[2, 1] = "Cut Set";
            //theWorkSheet.Cells[2, 1].Font.Bold = true;
            //theWorkSheet.Cells[2, 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Put the heading in
            for (var i = 0; i < max_cutset_length; i++)
            {
                var node_col = i * 2 + 1;
                theWorkSheet.Cells[2, node_col] = $"Node {i+1}";
                theWorkSheet.Cells[2, node_col].Font.Bold = true;
                theWorkSheet.Cells[2, node_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                GetRange(2, node_col, 2, node_col + 1).MergeCells = true;

                theWorkSheet.Cells[3, node_col + 0] = $"ID";
                theWorkSheet.Cells[3, node_col + 0].Font.Bold = true;
                theWorkSheet.Cells[3, node_col + 1] = $"λDU{i+1}";
                theWorkSheet.Cells[3, node_col + 1].Characters[2, 3].Font.Subscript = true;
                theWorkSheet.Cells[3, node_col + 1].Font.Bold = true;
                theWorkSheet.Cells[3, node_col + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }

            var pti_col = max_cutset_length * 2 + 1;
            var pfd_col = pti_col + 1;
            var frate_col = pfd_col + 1;

            theWorkSheet.Cells[2, pti_col] = "All Nodes";
            theWorkSheet.Cells[2, pti_col].Font.Bold = true;
            theWorkSheet.Cells[2, pti_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[3, pti_col] = "PTI";
            theWorkSheet.Cells[3, pti_col].Font.Bold = true;
            theWorkSheet.Cells[3, pti_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[2, pfd_col] = "Cut Set";
            theWorkSheet.Cells[2, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[2, pfd_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            GetRange(2, pfd_col, 2, frate_col).MergeCells = true;

            theWorkSheet.Cells[3, pfd_col] = "PFD";
            theWorkSheet.Cells[3, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[3, pfd_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[3, frate_col] = "Fail Rate";
            theWorkSheet.Cells[3, frate_col].Font.Bold = true;
            theWorkSheet.Cells[3, frate_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;


            foreach (var cs in cutSets.OrderByDescending(c => c.PFD))
            {
                row += 1;
                var node_index = 0;
                var node_qty = cs.Nodes.Count;
                // Put the highest PTI in the first column
                var max_pti = cs.Max(n => n.PTI);
                var prodString = "";
                foreach (var n in cs)
                {
                    theWorkSheet.Cells[row, node_index * 2 + 1] = n.Name;
                    theWorkSheet.Cells[row, node_index * 2 + 2] = n.Lambda;
                    theWorkSheet.Cells[row, node_index * 2 + 2].NumberFormat = "0.00E+00";
                    if (prodString.Length != 0)
                        prodString += ",";
                    prodString += addr(row, node_index * 2 + 2);
                    node_index += 1;
                }

                theWorkSheet.Cells[row, pti_col] = max_pti;
                theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";
                theWorkSheet.Cells[row, pfd_col] = $"={addr(row, pti_col)}^{node_qty}*Product({prodString})/{node_qty + 1}";
                theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";
                theWorkSheet.Cells[row, frate_col] = $"={addr(row, pti_col)}^{node_qty-1}*Product({prodString})";
                theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";
            }

            theWorkSheet.Cells[row + 1, pfd_col-1] = "Total=";
            theWorkSheet.Cells[row + 1, pfd_col-1].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col-1].HorizontalAlignment = XlHAlign.xlHAlignRight;

            theWorkSheet.Cells[row + 1, pfd_col] = $"=Sum({addr(4, pfd_col)}:{addr(row, pfd_col)})";
            theWorkSheet.Cells[row + 1, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col].NumberFormat = "0.00E+00";

            theWorkSheet.Cells[row + 1, pfd_col + 1] = $"=Sum({addr(4, frate_col)}:{addr(row, frate_col)})";
            theWorkSheet.Cells[row + 1, pfd_col + 1].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col + 1].NumberFormat = "0.00E+00";

            row += 2;
            GetRange(row, 1, row, 4).MergeCells = true;
            theWorkSheet.Cells[row, 1] = "List of Nodes";
            theWorkSheet.Cells[row, 1].Font.Bold = true;

            row += 1;
            theWorkSheet.Cells[row, 1] = "Ref";
            theWorkSheet.Cells[row, 1].Font.Bold = true;
            theWorkSheet.Cells[row, 2] = "Make/Model";
            theWorkSheet.Cells[row, 2].Font.Bold = true;
            theWorkSheet.Cells[row, 3] = "λDU";
            theWorkSheet.Cells[row, 3].Characters[2, 2].Font.Subscript = true;
            theWorkSheet.Cells[row, 3].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[row, 3].Font.Bold = true;
            theWorkSheet.Cells[row, 4] = "PTI";
            theWorkSheet.Cells[row, 4].HorizontalAlignment = XlHAlign.xlHAlignRight;
            theWorkSheet.Cells[row, 4].Font.Bold = true;

            foreach (var node in allNodes(theItem).OrderBy(xx => xx.Name))
            {
                if (node is Node theNode)
                {
                    row += 1;
                    theWorkSheet.Cells[row, 1] = node.Name;
                    theWorkSheet.Cells[row, 2] = node.MakeModel;
                    theWorkSheet.Cells[row, 3] = node.Lambda;
                    theWorkSheet.Cells[row, 3].NumberFormat = "0.00E+00";
                    theWorkSheet.Cells[row, 4] = node.PTI;
                }
            }
        }

        void ShowNodeCalcs(GraphicItem theItem)
        {
            var theApp = new Application();
            theApp.Visible = true;
            var theWorkBook = theApp.Workbooks.Add(System.Reflection.Missing.Value);
            _Worksheet theWorkSheet = theWorkBook.ActiveSheet;
            Func<int, int, string> addr = (r, c) =>
                theWorkSheet.Cells[r, c].Address(false, false);
            theWorkSheet.Cells[1, 1] = theItem.Name;
            var row = 1;
            var nodes = theItem.Nodes.Count;
            if (theItem is OR theOR)
            {
                foreach (var item in theOR.Nodes)
                {
                    row += 1;
                    theWorkSheet.Cells[row, 1] = item.Name;
                    theWorkSheet.Cells[row, 2] = item.Lambda;
                    theWorkSheet.Cells[row, 3] = item.MDT;
                    theWorkSheet.Cells[row, 4] = $"={addr(row, 2)}*{addr(row, 3)}";
                }
                theWorkSheet.Cells[1, 4] = $"=Sum({addr(2, 4)}:{addr(row, 4)})";
            }
            else if (theItem is AND theAND)
            {
                foreach (var item in theAND.Nodes)
                {
                    row += 1;
                    theWorkSheet.Cells[row, 1] = item.Name;
                    theWorkSheet.Cells[row, 2] = item.Lambda;
                    theWorkSheet.Cells[row, 3] = item.MDT;
                    theWorkSheet.Cells[row, 4] = $"={addr(row, 2)}*{addr(row, 3)}";
                }
                theWorkSheet.Cells[row, 5] = $"=Product({addr(2, 4)}:{addr(row, 4)})/{nodes + 1}";
            }

        }

        IEnumerable<GraphicItem> allNodes(GraphicItem item)
        {
            var result = new List<GraphicItem>();
            foreach (var x in item.Nodes)
            { 
                result.Add(x);
                foreach (var y in allNodes(x))
                    result.Add(y); 

            }
            return result;
        }
            



        public void Execute(object parameter)
        {
            if (parameter is GraphicItem theItem)
            {
                ShowCutSetCalcs(theItem);
            }
        }

    }
}
