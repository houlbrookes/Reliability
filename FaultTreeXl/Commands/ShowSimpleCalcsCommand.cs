using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace FaultTreeXl
{
    internal class ShowSimpleCalcsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public int DrawNode(_Workbook theWorkbook, _Worksheet theWorkSheet, GraphicItem theItem, int row=3)
        {

            Func<int, int, string> addr = (r, c) =>
                theWorkSheet.Cells[r, c].Address(false, false);

            var cutset_qty = theItem.Nodes.Count; // cutSets.Count;

            var max_cutset_length = 1;
            var andNodes = theItem.Nodes.OfType<AND>();
            if (andNodes.Count() > 0)
            {
                max_cutset_length = theItem.Nodes.OfType<AND>().Max(and => and.Nodes.Count); // Largest number of Nodes in an AND
            }
            var col_offset = max_cutset_length + 1;
            Range r1 = theWorkSheet.UsedRange;

            Func<int, int, int, int, Range> GetRange = (row1, col1, row2, col2) =>
            {
                var fromAddress = $"{addr(row1, col1)}";
                var toAddress = $"{addr(row2, col2)}";
                return theWorkSheet.Range[fromAddress, toAddress];
            };

            GetRange(row - 2, 1, row - 2, max_cutset_length * 2 + 3).MergeCells = true;
            theWorkSheet.Cells[row - 2, 1] = theItem.Name;
            theWorkSheet.Cells[row - 2, 1].Font.Bold = true;
            theWorkSheet.Cells[row - 2, 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Put the heading in
            for (var i = 0; i < max_cutset_length; i++)
            {
                var node_col = i * 2 + 1;
                theWorkSheet.Cells[row - 1, node_col] = $"Node {i + 1}";
                theWorkSheet.Cells[row - 1, node_col].Font.Bold = true;
                theWorkSheet.Cells[row - 1, node_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                GetRange(row - 1, node_col, row - 1, node_col + 1).MergeCells = true;

                theWorkSheet.Cells[row, node_col + 0] = $"ID";
                theWorkSheet.Cells[row, node_col + 0].Font.Bold = true;
                theWorkSheet.Cells[row, node_col + 1] = $"λDU{i + 1}";
                theWorkSheet.Cells[row, node_col + 1].Characters[2, 3].Font.Subscript = true;
                theWorkSheet.Cells[row, node_col + 1].Font.Bold = true;
                theWorkSheet.Cells[row, node_col + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }

            var pti_col = max_cutset_length * 2 + 1;
            var pfd_col = pti_col + 1;
            var frate_col = pfd_col + 1;

            theWorkSheet.Cells[row-1, pti_col] = "All Nodes";
            theWorkSheet.Cells[row - 1, pti_col].Font.Bold = true;
            theWorkSheet.Cells[row - 1, pti_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[row, pti_col] = "MDT";
            theWorkSheet.Cells[row, pti_col].Font.Bold = true;
            theWorkSheet.Cells[row, pti_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[row - 1, pfd_col] = ""; //"Cut Set";
            theWorkSheet.Cells[row - 1, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[row - 1, pfd_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            GetRange(row - 1, pfd_col, row - 1, frate_col).MergeCells = true;

            theWorkSheet.Cells[row, pfd_col] = "PFD";
            theWorkSheet.Cells[row, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[row, pfd_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            theWorkSheet.Cells[row, frate_col] = "Fail Rate";
            theWorkSheet.Cells[row, frate_col].Font.Bold = true;
            theWorkSheet.Cells[row, frate_col].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            var saveHeadingRow = row;

            foreach (var cs in theItem.Nodes.OrderByDescending(c => c.PFD))
            {
                row += 1;
                // Put the highest PTI in the first column
                var max_mdt = cs.MDT;
                var min_mdt = cs.MDT;

                var prodString = "";
                if (cs is Node)
                {
                    if (cs.ProofTestEffectiveness == 1M)
                    {
                        theWorkSheet.Cells[row, 1] = cs.Name;
                        theWorkSheet.Cells[row, 2] = cs.Lambda;
                        theWorkSheet.Cells[row, 2].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, pti_col] = max_mdt;
                        theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";

                        theWorkSheet.Cells[row, pfd_col] = $"={addr(row, pti_col)}*{addr(row, 2)}";
                        theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, frate_col] = $"={addr(row, 2)}";
                        theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";

                    }
                    else
                    {
                        theWorkSheet.Cells[row, 1] = cs.Name + " (Proof)";
                        theWorkSheet.Cells[row, 2] = cs.Lambda * cs.ProofTestEffectiveness;
                        theWorkSheet.Cells[row, 2].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, pti_col] = cs.PTI/2;
                        theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";

                        theWorkSheet.Cells[row, pfd_col] = $"={addr(row, pti_col)}*{addr(row, 2)}";
                        theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, frate_col] = $"={addr(row, 2)}";
                        theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";

                        row += 1;

                        theWorkSheet.Cells[row, 1] = cs.Name + "(Life)";
                        theWorkSheet.Cells[row, 2] = cs.Lambda * (1 - cs.ProofTestEffectiveness);
                        theWorkSheet.Cells[row, 2].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, pti_col] = cs.LifeTime/2;
                        theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";

                        theWorkSheet.Cells[row, pfd_col] = $"={addr(row, pti_col)}*{addr(row, 2)}";
                        theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";

                        theWorkSheet.Cells[row, frate_col] = $"={addr(row, 2)}";
                        theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";

                    }
                }
                else if (cs is OR)
                {
                    theWorkSheet.Cells[row, 1] = cs.Name;
                    theWorkSheet.Cells[row, 2] = cs.Lambda;
                    theWorkSheet.Cells[row, 2].NumberFormat = "0.00E+00";

                    theWorkSheet.Cells[row, pti_col] = max_mdt;
                    theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";

                    theWorkSheet.Cells[row, pfd_col] = $"={addr(row, pti_col)}*{addr(row, 2)}";
                    theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";
                    
                    theWorkSheet.Cells[row, frate_col] = $"={addr(row, 2)}";
                    theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";
                }
                else if (cs is AND)
                {
                    if (cs.Nodes.Count() > 0)
                    {
                        max_mdt = cs.Nodes.Max(n => n.MDT);
                        min_mdt = cs.Nodes.Min(n => n.MDT);
                    }
                    var node_index = 0;
                    var node_qty = cs.Nodes.Count;
                    foreach (var n in cs.Nodes)
                    {
                        theWorkSheet.Cells[row, node_index * 2 + 1] = n.Name;
                        theWorkSheet.Cells[row, node_index * 2 + 2] = n.Lambda;
                        theWorkSheet.Cells[row, node_index * 2 + 2].NumberFormat = "0.00E+00";
                        if (prodString.Length != 0)
                            prodString += ",";
                        prodString += addr(row, node_index * 2 + 2);
                        node_index += 1;
                    }
                    theWorkSheet.Cells[row, pti_col] = max_mdt;
                    theWorkSheet.Cells[row, pti_col].NumberFormat = @"#,###,##0";
                    theWorkSheet.Cells[row, pfd_col] = $"=({addr(row, pti_col)}*2)^{node_qty}*Product({prodString})/{node_qty + 1}";
                    theWorkSheet.Cells[row, pfd_col].NumberFormat = "0.00E+00";
                    theWorkSheet.Cells[row, frate_col] = $"=({addr(row, pti_col)}*2)^{node_qty - 1}*Product({prodString})";
                    theWorkSheet.Cells[row, frate_col].NumberFormat = "0.00E+00";
                    
                    if (min_mdt != max_mdt)
                    {
                        theWorkSheet.Cells[row, frate_col + 1] = $"** Largest MDT used as a conservative estimate";
                    }

                }
            }

            theWorkSheet.Cells[row + 1, pfd_col - 1] = "Total=";
            theWorkSheet.Cells[row + 1, pfd_col - 1].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col - 1].HorizontalAlignment = XlHAlign.xlHAlignRight;

            theWorkSheet.Cells[row + 1, pfd_col] = $"=Sum({addr(saveHeadingRow + 1, pfd_col)}:{addr(row, pfd_col)})";
            theWorkSheet.Cells[row + 1, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col].NumberFormat = "0.00E+00";

            theWorkSheet.Cells[row + 1, pfd_col + 1] = $"=Sum({addr(saveHeadingRow + 1, frate_col)}:{addr(row, frate_col)})";
            theWorkSheet.Cells[row + 1, pfd_col + 1].Font.Bold = true;
            theWorkSheet.Cells[row + 1, pfd_col + 1].NumberFormat = "0.00E+00";


            theWorkSheet.Cells[row + 2, pfd_col - 1] = "MDT=";
            theWorkSheet.Cells[row + 2, pfd_col - 1].Font.Bold = true;
            theWorkSheet.Cells[row + 2, pfd_col - 1].HorizontalAlignment = XlHAlign.xlHAlignRight;

            theWorkSheet.Cells[row + 2, pfd_col] = $"={addr(row+1, pfd_col)}/{addr(row + 1, frate_col)}";
            theWorkSheet.Cells[row + 2, pfd_col].Font.Bold = true;
            theWorkSheet.Cells[row + 2, pfd_col].NumberFormat = @"#,###,##0";

            theWorkSheet.Cells[row + 2, pfd_col + 1] = $"hrs";
            theWorkSheet.Cells[row + 2, pfd_col + 1].Font.Bold = true;
            theWorkSheet.Cells[row + 2, pfd_col + 1].HorizontalAlignment = XlHAlign.xlHAlignLeft;

            //_Worksheet lastWorksheet = theWorkSheet;            
            foreach (var subNode in theItem.Nodes.OfType<OR>())
            {
                //var nextWorksheet = theWorkbook.Worksheets.Add(After:lastWorksheet);
                var nextWorksheet = theWorkSheet;
                row = DrawNode(theWorkbook, nextWorksheet, subNode, row + 6);
                //lastWorksheet = nextWorksheet;
            }
            foreach (var subANDNode in theItem.Nodes.OfType<AND>())
            {
                foreach (var subNode in subANDNode.Nodes.OfType<OR>())
                {
                    //var nextWorksheet = theWorkbook.Worksheets.Add(After: lastWorksheet);
                    var nextWorksheet = theWorkSheet;
                    row = DrawNode(theWorkbook, nextWorksheet, subNode, row + 6);
                    //lastWorksheet = nextWorksheet;
                }
            }

            return row;
        }

        private void ShowCutSetCalcs(GraphicItem theItem)
        {
            var theApp = new Application();
            theApp.Visible = true;
            var theWorkBook = theApp.Workbooks.Add(System.Reflection.Missing.Value);
            _Worksheet theWorkSheet = theWorkBook.ActiveSheet;


            var row = DrawNode(theWorkBook, theWorkSheet, theItem);

            Func<int, int, string> addr = (r, c) =>
                theWorkSheet.Cells[r, c].Address(false, false);

            Func<int, int, int, int, Range> GetRange = (row1, col1, row2, col2) =>
            {
                var fromAddress = $"{addr(row1, col1)}";
                var toAddress = $"{addr(row2, col2)}";
                return theWorkSheet.Range[fromAddress, toAddress];
            };

            row += 3;
            // if this table includes PTC values then extend the cell to include 10 columns
            // otherwise only 6 columns are required

            var listOfAlllNodes = allNodes(theItem);
            var anyPTE = listOfAlllNodes.Any(thisNode => thisNode.ProofTestEffectiveness != 1.0M);

            GetRange(row, 1, row, anyPTE?10:6).MergeCells = true;
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
            theWorkSheet.Cells[row, 5] = "β %";
            theWorkSheet.Cells[row, 5].HorizontalAlignment = XlHAlign.xlHAlignRight;
            theWorkSheet.Cells[row, 5].Font.Bold = true;
            theWorkSheet.Cells[row, 6] = "λDU*(1-β)";
            theWorkSheet.Cells[row, 6].Characters[2, 2].Font.Subscript = true;
            theWorkSheet.Cells[row, 6].HorizontalAlignment = XlHAlign.xlHAlignRight;
            theWorkSheet.Cells[row, 6].Font.Bold = true;

            if (anyPTE)
            {
                theWorkSheet.Cells[row, 7] = "C";
                theWorkSheet.Cells[row, 7].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                theWorkSheet.Cells[row, 7].Font.Bold = true;
                theWorkSheet.Cells[row, 8] = "λDU*(1-β)*C";
                theWorkSheet.Cells[row, 8].Characters[2, 2].Font.Subscript = true;
                theWorkSheet.Cells[row, 8].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                theWorkSheet.Cells[row, 8].Font.Bold = true;
                theWorkSheet.Cells[row - 1, 8] = "_P";
                theWorkSheet.Cells[row - 1, 8].Font.Bold = true;
                theWorkSheet.Cells[row - 1, 8].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                theWorkSheet.Cells[row, 9] = "λDU*(1-β)*(1-C)";
                theWorkSheet.Cells[row, 9].Characters[2, 2].Font.Subscript = true;
                theWorkSheet.Cells[row, 9].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                theWorkSheet.Cells[row, 9].Font.Bold = true;
                theWorkSheet.Cells[row - 1, 9] = "_L";
                theWorkSheet.Cells[row - 1, 9].Font.Bold = true;
                theWorkSheet.Cells[row - 1, 9].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                theWorkSheet.Cells[row, 10] = "Lifetime";
                theWorkSheet.Cells[row, 10].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                theWorkSheet.Cells[row, 10].Font.Bold = true;
            }

            foreach (var node in listOfAlllNodes.OrderBy(theNode => theNode.Name).OrderByDescending(theNode => theNode.Lambda))
            {
                if (node is Node theNode)
                {
                    row += 1;
                    theWorkSheet.Cells[row, 1] = node.Name;
                    theWorkSheet.Cells[row, 2] = node.MakeModel;
                    theWorkSheet.Cells[row, 3] = node.BetaFreeLambda;
                    theWorkSheet.Cells[row, 3].NumberFormat = "0.00E+00";
                    theWorkSheet.Cells[row, 4] = node.PTI;
                    if (node.Beta!=0)
                    {
                        theWorkSheet.Cells[row, 5] = node.Beta;
                        theWorkSheet.Cells[row, 6] = node.Lambda;
                        theWorkSheet.Cells[row, 6].NumberFormat = "0.00E+00";
                    }
                    if (node.ProofTestEffectiveness != 1)
                    {
                        theWorkSheet.Cells[row, 7] = node.ProofTestEffectiveness;
                        theWorkSheet.Cells[row, 7].NumberFormat = "0%";
                        theWorkSheet.Cells[row, 8] = node.Lambda * node.ProofTestEffectiveness;
                        theWorkSheet.Cells[row, 8].NumberFormat = "0.00E+00";
                        theWorkSheet.Cells[row, 9] = node.Lambda * (1 - node.ProofTestEffectiveness);
                        theWorkSheet.Cells[row, 9].NumberFormat = "0.00E+00";
                        theWorkSheet.Cells[row, 10] = node.LifeTime;
                        theWorkSheet.Cells[row, 10].NumberFormat = "#,###,##0";
                    }

                }
            }

            try
            {
                theWorkSheet.Name = theItem.Name;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error Renaming Sheet");
            }

            // Tell the user that the process has completed
            MessageBox.Show("All done!", "Simple Calculations Complete", MessageBoxButtons.OK);

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

        /// <summary>
        /// Find all nodes including this one
        /// Also sorts the nodes on name and lambda
        /// </summary>
        /// <param name="item">the item to search</param>
        /// <returns>a list of GraphicItems below this one including this one</returns>
        IEnumerable<GraphicItem> allNodes(GraphicItem item)
        {
            return from eachNode in item.Nodes
                   from eachSubNode in new List<GraphicItem> { eachNode }.Concat(allNodes(eachNode))
                   orderby eachSubNode.Name ascending, eachSubNode.Lambda descending
                   select eachSubNode;
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
