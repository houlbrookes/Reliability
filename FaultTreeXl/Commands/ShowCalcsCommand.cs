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
//using Application = Microsoft.Office.Interop.Excel.Application;
using static FaultTreeXl.GlobalExcelApp;

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
            NewWorksheet();

            var cutSets = theItem.CutSets;
            var row = 3;

            var cutset_qty = cutSets.Count;
            var max_cutset_length = cutSets.Max(cs => cs.Count);
            var col_offset = max_cutset_length + 1;

            GetRange(1, 1, 1, max_cutset_length * 2 + 3).MergeCells();
            Cell(1, 1).AssignValue(theItem.Name).Bold().AlignCentre();

            // Put the headings for the Cutsets (up to maximum number of nodes)
            for (var i = 0; i < max_cutset_length; i++)
            {
                var node_col = i * 2 + 1;
                Cell(2, node_col).AssignValue($"Node {i + 1}").Bold().AlignCentre();
                GetRange(2, node_col, 2, node_col + 1).MergeCells();

                Cell(3, node_col + 0).AssignValue($"ID").Bold();
                Cell(3, node_col + 1).AssignValue($"λDU{i + 1}").Subscript(2,3).Bold().AlignCentre();
            }

            // Calculate the positions of the PTI, PFD and Failure Rate positions
            var pti_col = max_cutset_length * 2 + 1;
            var pfd_col = pti_col + 1;
            var frate_col = pfd_col + 1;

            // Headings for the Cutsets
            Cell(2, pti_col).AssignValue("All Nodes").Bold().AlignCentre();
            Cell(3, pti_col).AssignValue("PTI").Bold().AlignCentre();
            Cell(2, pfd_col).AssignValue("Cut Set").Bold().AlignCentre();
            GetRange(2, pfd_col, 2, frate_col).MergeCells();
            Cell(2, frate_col + 1).AssignValue("PFD").Bold();
            Cell(3, frate_col + 1).AssignValue("factor").Bold();
            Cell(2, frate_col + 2).AssignValue("Lambda").Bold();
            Cell(3, frate_col + 2).AssignValue("factor").Bold();
            Cell(2, frate_col + 3).AssignValue("PFD").Bold();
            Cell(3, frate_col + 3).AssignValue("Calc").Bold();
            Cell(2, frate_col + 4).AssignValue("Lambda").Bold();
            Cell(3, frate_col + 4).AssignValue("Calc").Bold();
            Cell(2, frate_col + 5).AssignValue("Used").Bold();
            Cell(3, frate_col + 5).AssignValue("Integration").Bold();

            Cell(3, pfd_col).AssignValue( "PFD").Bold().AlignCentre();
            Cell(3, frate_col).AssignValue("Fail Rate").Bold().AlignCentre();

            foreach (var cs in cutSets.OrderByDescending(c => c.PFD))
            {
                // Individual Cutset values
                row += 1;
                var node_index = 0;
                var node_qty = cs.Nodes.Count;
                // Put the highest PTI in the first column
                var min_pti = cs.Min(n => n.PTI);
                var max_pti = cs.Max(n => n.PTI);
                var no_of_min_ptis = cs.Count(n => n.PTI == min_pti);
                var prodString = "";
                foreach (var n in cs)
                {
                    // Individual nodes in the Cutset
                    Cell(row, node_index * 2 + 1).AssignValue(n.Name);
                    Cell(row, node_index * 2 + 2).AssignValue($"={n.FormulaName}_lambda").SetScientificFormat();

                    if (prodString.Length != 0)
                        prodString += ",";
                    prodString += (row, node_index * 2 + 2).addr();
                    node_index += 1;
                }

                Cell(row, pti_col).AssignValue(min_pti).SetThousandsFormat();

                (var pfdFactor, var lambdaFactor) = cs.PFDFactor();

                Cell(row, pfd_col).AssignValue($"={(row, pti_col).addr()}^{node_qty}*Product({prodString})*{pfdFactor}").SetScientificFormat();
                Cell(row, frate_col).AssignValue($"={(row, pti_col).addr()}^{node_qty - 1}*Product({prodString})*{lambdaFactor}").SetScientificFormat();
                
                // Extra information to compare against values displayed on the Fault Tree
                Cell(row, frate_col + 1).AssignValue($"={pfdFactor}").SetScientificFormat();
                Cell(row, frate_col + 2).AssignValue($"={lambdaFactor}").SetScientificFormat();
                Cell(row, frate_col + 3).AssignValue($"={cs.PFD}").SetScientificFormat();
                Cell(row, frate_col + 4).AssignValue($"={cs.Lambda}").SetScientificFormat();
                Cell(row, frate_col + 5).AssignValue($"={cs.UsedIntegration}").SetScientificFormat();
            }

            row += 1;
            Cell(row, pfd_col - 1).AssignValue("Total=").Bold().SetScientificFormat().AlignRight();
            Cell(row, pfd_col).AssignValue($"=Sum({(4, pfd_col).addr()}:{(row-1, pfd_col).addr()})").Bold().SetScientificFormat();
            Cell(row, frate_col).AssignValue($"=Sum({(4, frate_col).addr()}:{(row - 1, frate_col).addr()})").Bold().SetScientificFormat();

            // Add the headings for the Individual Nodes
            row += 2; // Leave blank line between Cutset values and Node values
            GetRange(row, 1, row, 4).MergeCells().AssignValue("List of Nodes").Bold().AlignCentre();

            row += 1;

            Cell(row, 1).AssignValue("Ref").Bold();
            Cell(row, 2).AssignValue("Make/Model").Bold();
            Cell(row, 3).AssignValue("λDU").Bold().Subscript(2,2).AlignCentre();
            Cell(row, 4).AssignValue("PTI").Bold().AlignRight();
            Cell(row, 5).AssignValue("β %").Bold().AlignRight();
            Cell(row, 6).AssignValue("λDU*(1-β)").Bold().Subscript(2,2).AlignRight();

            var everyNode = allNodes(theItem);
            var anyPTE = everyNode.Any(thisNode => thisNode.ProofTestEffectiveness != 1.0M);

            if (anyPTE)
            {
                // At least one of the nodes in the cut set has a PTE < 100%
                // So add in the other variables
                Cell(row, 7).AssignValue("C").AlignCentre().Bold();
                Cell(row, 8).AssignValue("λDU*(1-β)*C").Subscript(2,2).AlignCentre().Bold();
                Cell(row - 1, 8).AssignValue("_P").AlignCentre().Bold();
                Cell(row, 9).AssignValue("λDU*(1-β)*(1-C)").Subscript(2,2).AlignCentre().Bold();
                Cell(row - 1, 9).AssignValue("_L").AlignCentre().Bold();
                Cell(row, 10).AssignValue("Lifetime").AlignCentre().Bold();

            }

            foreach (var node in everyNode.OrderBy(theNode => theNode.Name))
            {
                // Double check that they are Nodes and not ANDs or ORs
                if (node is Node theNode)
                {
                    row += 1;
                    Cell(row, 1).AssignValue(node.Name);
                    Cell(row, 2).AssignValue(node.MakeModel);
                    Cell(row, 3).AssignValue(node.BetaFreeLambda).NameRange($"{node.FormulaName}_lambda").SetScientificFormat();
                    Cell(row, 4).AssignValue(node.PTI);
                    if (node.Beta != 0)
                    {
                        // We have some common cause factor so add that add extra columns for that
                        Cell(row, 5).AssignValue(node.Beta);
                        Cell(row, 6).AssignValue(node.Lambda).SetScientificFormat()
                            // switch the Lambda Reference to this cell
                            .NameRange($"{node.FormulaName}_lambda");
                    }
                    if (node.ProofTestEffectiveness != 1)
                    {
                        // We have an incomplete Proof-test node
                        Cell(row, 7).AssignValue(node.ProofTestEffectiveness).SetPercentFormat();
                        Cell(row, 8).AssignValue(node.Lambda * node.ProofTestEffectiveness).SetScientificFormat()
                            .NameRange($"{node.FormulaName}_P_lambda");
                        Cell(row, 9).AssignValue(node.Lambda * (1 - node.ProofTestEffectiveness)).SetScientificFormat()
                            .NameRange($"{node.FormulaName}_L_lambda");
                        Cell(row, 10).AssignValue(node.LifeTime).SetThousandsFormat();
                    }
                }
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
