using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
//using Microsoft.Office.Interop.Excel;
//using Application = Microsoft.Office.Interop.Excel.Application;
using static FaultTreeXl.GlobalExcelApp;
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

        public int DrawNode(/*_Workbook theWorkbook, _Worksheet theWorkSheet, */GraphicItem theItem, int row=3)
        {
            var cutset_qty = theItem.Nodes.Count; // cutSets.Count;

            var max_cutset_length = 1;
            var andNodes = theItem.Nodes.OfType<AND>();
            if (andNodes.Count() > 0)
            {
                max_cutset_length = theItem.Nodes.OfType<AND>().Max(and => and.Nodes.Count); // Largest number of Nodes in an AND
            }
            var col_offset = max_cutset_length + 1;

            CreateRange(row - 2, 1, row - 2, max_cutset_length * 2 + 3)
                .MergeCells().AssignValue(theItem.Name).Bold().AlignCentre();

            // Put the heading in
            for (var i = 0; i < max_cutset_length; i++)
            {
                var node_col = i * 2 + 1;
                Cell(row - 1, node_col).AssignValue($"Node {i + 1}").Bold().AlignCentre();
                GetRange(row - 1, node_col, row - 1, node_col + 1).MergeCells();
                Cell(row, node_col + 0).AssignValue($"ID").Bold();
                Cell(row, node_col + 1).AssignValue($"λDU{i + 1}").Subscript(2, 3).Bold().AlignCentre();
            }

            var pti_col = max_cutset_length * 2 + 1;
            var pfd_col = pti_col + 1;
            var frate_col = pfd_col + 1;

            Cell(row - 1, pti_col).AssignValue("All Nodes").Bold().AlignCentre();
            Cell(row, pti_col).AssignValue("MDT").Bold().AlignCentre();
            Cell(row - 1, pfd_col).AssignValue("").Bold().AlignCentre(); // Cutset
            GetRange(row - 1, pfd_col, row - 1, frate_col).MergeCells();

            Cell(row, pfd_col).AssignValue("PFD").Bold().AlignCentre();
            Cell(row, frate_col).AssignValue("Fail Rate").Bold().AlignCentre();

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
                        // PTE is 100% so single entry
                        Cell(row, 1).AssignValue(cs.Name);
                        Cell(row, 2).AssignValue($"={cs.FormulaName}_lambda").SetScientificFormat();
                        Cell(row, pti_col).AssignValue($"={cs.FormulaName}_PTI / 2").SetThousandsFormat();
                        Cell(row, pfd_col).AssignValue($"={(row, pti_col).addr()}*{(row, 2).addr()}")
                            .SetScientificFormat();
                        Cell(row, frate_col).AssignValue($"={(row, 2).addr()}")
                            .SetScientificFormat();
                    }
                    else
                    {
                        // PTI < 100% so put in an entry for Proof Test and Life Time
                        Cell(row, 1).AssignValue(cs.Name + " (Proof)");
                        Cell(row, 2).AssignValue($"={cs.FormulaName}_lambda_P").SetScientificFormat();
                        Cell(row, pti_col).AssignValue($"={cs.FormulaName}_PTI / 2").SetThousandsFormat();
                        Cell(row, pfd_col).AssignValue($"={(row, pti_col).addr()}*{(row, 2).addr()}").SetScientificFormat();
                        Cell(row, frate_col).AssignValue($"={(row, 2).addr()}").SetScientificFormat();

                        row += 1;

                        Cell(row, 1).AssignValue(cs.Name + " (Life)");
                        Cell(row, 2).AssignValue($"={cs.FormulaName}_lambda_L").SetScientificFormat();

                        Cell(row, pti_col).AssignValue($"={cs.FormulaName}_LifeTime / 2").SetThousandsFormat();
                        Cell(row, pfd_col).AssignValue($"={(row, pti_col).addr()}*{(row, 2).addr()}").SetScientificFormat();
                        Cell(row, frate_col).AssignValue($"={(row, 2).addr()}").SetScientificFormat();
                    }
                }
                else if (cs is OR)
                {
                    Cell(row, 1).AssignValue(cs.Name).SetScientificFormat();
                    Cell(row, 2).AssignValue($"={cs.FormulaName}_lambda").SetScientificFormat();
                    Cell(row, pti_col).AssignValue($"={cs.FormulaName}_MDT").SetThousandsFormat();
                    Cell(row, pfd_col).AssignValue($"={(row, pti_col).addr()}*{(row, 2).addr()}").SetScientificFormat();
                    Cell(row, frate_col).AssignValue($"={(row, 2).addr()}").SetScientificFormat();
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
                        Cell(row, node_index * 2 + 1).AssignValue(n.Name);
                        Cell(row, node_index * 2 + 2).AssignValue(n.Lambda).SetScientificFormat();

                        if (prodString.Length != 0)
                            prodString += ",";
                        prodString += (row, node_index * 2 + 2).addr();
                        node_index += 1;
                    }

                    Cell(row, pti_col).AssignValue(max_mdt).SetThousandsFormat();
                    Cell(row, pfd_col).AssignValue($"=({(row, pti_col).addr()}*2)^{node_qty}*Product({prodString})/{node_qty + 1}").SetScientificFormat();
                    Cell(row, frate_col).AssignValue($"=({(row, pti_col).addr()}*2)^{node_qty - 1}*Product({prodString})").SetScientificFormat();

                    if (min_mdt != max_mdt)
                    {
                        Cell(row, frate_col + 1).AssignValue($"** Largest MDT used as a conservative estimate");
                        //theWorkSheet.Cells[row, frate_col + 1] = $"** Largest MDT used as a conservative estimate";
                    }
                }
            }

            Cell(row+1, pfd_col - 1).AssignValue("Total=").Bold().AlignRight();
            Cell(row+1, pfd_col)
                .AssignValue($"=Sum({(saveHeadingRow + 1, pfd_col).addr()}:{(row, pfd_col).addr()})")
                .Bold()
                .SetScientificFormat();

            Cell(row+1, pfd_col + 1)
                .AssignValue($"=Sum({(saveHeadingRow + 1, frate_col).addr()}:{(row, frate_col).addr()})")
                .Bold()
                .SetScientificFormat()
                .NameRange($"{theItem.FormulaName}_lambda");
            
            Cell(row + 2, pfd_col - 1).AssignValue("MDT=").Bold().AlignRight();
            Cell(row + 2, pfd_col).AssignValue($"={(row + 1, pfd_col).addr()}/{(row + 1, frate_col).addr()}")
                .Bold().SetThousandsFormat()
                .NameRange($"{theItem.FormulaName}_MDT");
            Cell(row + 2, pfd_col + 1).AssignValue($"hrs").Bold().AlignLeft();


            //_Worksheet lastWorksheet = theWorkSheet;            
            foreach (var subNode in theItem.Nodes.OfType<OR>())
            {
                row = DrawNode(subNode, row + 6);
            }
            foreach (var subANDNode in theItem.Nodes.OfType<AND>())
            {
                foreach (var subNode in subANDNode.Nodes.OfType<OR>())
                {
                    row = DrawNode(subNode, row + 6);
                }
            }

            return row;
        }

        private void ShowCutSetCalcs(GraphicItem theItem)
        {
            NewWorksheet();

            var row = DrawNode(/* theWorkBook, theWorkSheet,*/ theItem);

            row += 3;
            // if this table includes PTC values then extend the cell to include 10 columns
            // otherwise only 6 columns are required

            var listOfAlllNodes = allNodes(theItem);
            var anyPTE = listOfAlllNodes.Any(thisNode => thisNode.ProofTestEffectiveness != 1.0M);

            GetRange(row, 1, row, anyPTE?10:6).MergeCells();
            Cell(row, 1).AssignValue("List of Nodes").Bold();

            // Add an extra row for titles that take up two lines
            row += 2;

            Cell(row, 1).AssignValue("Ref").Bold();
            Cell(row, 2).AssignValue("Make/Model").Bold();
            Cell(row, 3).AssignValue("λDU").Subscript(2, 2).Bold().AlignCentre();
            Cell(row, 4).AssignValue("PTI").Bold().AlignRight();
            Cell(row, 5).AssignValue("β").Bold().AlignRight();
            Cell(row, 6).AssignValue("λDU*(1-β)").Subscript(2,2).Bold().AlignRight();

            if (anyPTE)
            {
                Cell(row, 7).AssignValue("C").Bold().AlignCentre();
                Cell(row, 8).AssignValue("λDU*(1-β)*C").Subscript(2,2).Bold().AlignCentre();
                Cell(row-1, 8).AssignValue("_P").Bold().AlignCentre();
                Cell(row, 9).AssignValue("λDU*(1-β)*(1-C)").Subscript(2,2).Bold().AlignCentre();
                Cell(row - 1, 9).AssignValue("_L").Bold().AlignCentre();
                Cell(row, 10).AssignValue("Lifetime").Bold().AlignCentre();
            }

            foreach (var node in listOfAlllNodes.OrderBy(theNode => theNode.Name).OrderByDescending(theNode => theNode.Lambda))
            {
                if (node is Node theNode)
                {
                    row += 1;
                    Cell(row, 1).AssignValue(node.Name);
                    Cell(row, 2).AssignValue(node.MakeModel);
                    Cell(row, 3).AssignValue(node.BetaFreeLambda).SetScientificFormat()
                        .NameRange($"{node.FormulaName}_lambda");
                    
                    Cell(row, 4).AssignValue(node.PTI)
                        .NameRange($"{node.FormulaName}_PTI");

                    if (node.Beta!=0)
                    {
                        Cell(row, 5).AssignValue(node.Beta/100).SetNumberFormat("0%");
                        Cell(row, 6).AssignValue($"={(row, 3).addr()} * (1-{(row, 5).addr()})")
                            .SetScientificFormat()
                            .NameRange($"{node.FormulaName}_lambda");
                    }
                    if (node.ProofTestEffectiveness != 1)
                    {
                        Cell(row, 7).AssignValue(node.ProofTestEffectiveness).SetNumberFormat("0%");
                        var pte_addr = $"{(row, 7).addr()}";

                        Cell(row, 8).AssignValue($"={node.FormulaName}_lambda * {pte_addr}").SetScientificFormat()
                            .NameRange($"{node.FormulaName}_lambda_P");

                        Cell(row, 9).AssignValue($"={node.FormulaName}_lambda * (1-{pte_addr})").SetScientificFormat()
                            .NameRange($"{node.FormulaName}_lambda_L");
             
                        Cell(row, 10).AssignValue(node.LifeTime).SetThousandsFormat()
                            .NameRange($"{node.FormulaName}_Lifetime");
                    }

                }
            }

            try
            {
                SetWorksheetName(theItem.FormulaName);
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
            //var theApp = new Application();
            //theApp.Visible = true;
            //var theWorkBook = theApp.Workbooks.Add(System.Reflection.Missing.Value);
            //_Worksheet theWorkSheet = theWorkBook.ActiveSheet;
            //Func<int, int, string> addr = (r, c) =>
            //    theWorkSheet.Cells[r, c].Address(false, false);

            Cell(1, 1).AssignValue(theItem.Name);
            //theWorkSheet.Cells[1, 1] = theItem.Name;
            //var row = 1;
            //var nodes = theItem.Nodes.Count;
            //if (theItem is OR theOR)
            //{
            //    foreach (var item in theOR.Nodes)
            //    {
            //        row += 1;
            //        theWorkSheet.Cells[row, 1] = item.Name;
            //        theWorkSheet.Cells[row, 2] = item.Lambda;
            //        theWorkSheet.Cells[row, 3] = item.MDT;
            //        theWorkSheet.Cells[row, 4] = $"={addr(row, 2)}*{addr(row, 3)}";
            //    }
            //    theWorkSheet.Cells[1, 4] = $"=Sum({addr(2, 4)}:{addr(row, 4)})";
            //}
            //else if (theItem is AND theAND)
            //{
            //    foreach (var item in theAND.Nodes)
            //    {
            //        row += 1;
            //        theWorkSheet.Cells[row, 1] = item.Name;
            //        theWorkSheet.Cells[row, 2] = item.Lambda;
            //        theWorkSheet.Cells[row, 3] = item.MDT;
            //        theWorkSheet.Cells[row, 4] = $"={addr(row, 2)}*{addr(row, 3)}";
            //    }
            //    theWorkSheet.Cells[row, 5] = $"=Product({addr(2, 4)}:{addr(row, 4)})/{nodes + 1}";
            //}

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
