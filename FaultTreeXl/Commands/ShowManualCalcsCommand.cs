using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Word = Microsoft.Office.Interop.Word;


namespace FaultTreeXl
{
    internal class ShowManualCalcsCommand : ICommand
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
            if (parameter is GraphicItem node)
            {
                if (node is Node n1)
                {
                    // Not used currently because node does not have the context menu for this
                    GlobalWordApp.Document.AddEquation(@"PFD_AVG= βλ(cT+(1-c)L)/2");
                    GlobalWordApp.Document.Paragraphs.Add();
                    var l = n1.BetaFreeLambda.FormatDouble();
                    var T = 8760;
                    var L = 10 * 8760;
                    var b = 0.1;
                    var c = 0.9;
                    GlobalWordApp.Document.AddEquation($"PFD_AVG=({b} × ({l}) × {c} × {T}+(1-{c}) × {L})/2");
                }
                else if (node is OR or1)
                {
                    GlobalWordApp.AddSection(or1.FormulaString());
                    GlobalWordApp.AddSection(or1.ValuesString());
                    GlobalWordApp.AddSection(or1.TotalString());
                }
            }

        }

        void SameLambdasPerfectProof(IEnumerable<GraphicItem> nodes, Node cCF, OR topNode, Word.Document document)
        {
            var nodeCount = nodes.Count();
            if (nodeCount == 0) return;

            var a1 = nodes.First();
            var l = a1.BetaFreeLambda.FormatDouble();
            var b = a1.Beta > 0 ? a1.Beta / 100 : 0.1; // use 10% if beta is zero
            var T = a1.PTI;
            var l_beta = (cCF.BetaFreeLambda / (decimal)b);
            document.AddEquation($"PFD_{topNode.FormulaName}=(λ^{nodeCount} (1-β)^{nodeCount} T^{nodeCount})/{nodeCount + 1}+βλT/2");
            document.Paragraphs.Add();
            document.AddEquation($"PFD_{topNode.FormulaName}=(({l})^{nodeCount} × (1-{b})^{nodeCount} × {T}^{nodeCount})/{nodeCount + 1} + ({b} × ({l_beta.FormatDouble()}) × {T})/2");
            document.Paragraphs.Add();
            var eq1 = nodes.Select(n => n.BetaFreeLambda).Product() * (decimal)Math.Pow((1 - b) * (double)T, nodeCount) / (nodeCount + 1);
            var eq2 = (decimal)b * l_beta * T / 2;
            document.AddEquation($"PFD_{topNode.FormulaName}={eq1.FormatDouble()} + {eq2.FormatDouble()}");
            document.Paragraphs.Add();
            document.AddEquation($"PFD_{topNode.FormulaName}={topNode.PFD.FormatDouble()}");
            document.Paragraphs.Add();
        }
        void DifferentLambdasPerfectProof(IEnumerable<GraphicItem> nodes, Node cCF, OR topNode, Word.Document document)
        {
            var nodeCount = nodes.Count();
            if (nodeCount == 0) return;

            var a1 = nodes.First();
            var l = a1.BetaFreeLambda.FormatDouble();
            var b = a1.Beta > 0 ? a1.Beta / 100 : 0.1; // use 10% if beta is zero
            var T = a1.PTI;
            var l_beta = cCF.BetaFreeLambda / (decimal)b;
            var l_string = string.Join(".", nodes.Select(n => $"λ_{n.FormulaName}"));
            document.AddEquation($"PFD_{topNode.FormulaName}=({l_string}.(1-β)^{nodeCount}.T^{nodeCount})/{nodeCount + 1}+βλT/2");
            document.Paragraphs.Add();
            var l_stringN = string.Join(" × ", nodes.Select(n => $"{n.BetaFreeLambda.FormatDouble()}"));
            document.AddEquation($"PFD_{topNode.FormulaName}=({l_stringN} × (1-{b})^{nodeCount} × {T}^{nodeCount})/{nodeCount + 1} + ({b} × ({l_beta.FormatDouble()}) × {T})/2");
            document.Paragraphs.Add();
            var eq1 = nodes.Select(n => n.BetaFreeLambda).Product() * (decimal)Math.Pow((1 - b) * (double)T, nodeCount) / (nodeCount + 1);
            var eq2 = (decimal)b * l_beta * T / 2;
            document.AddEquation($"PFD_{topNode.FormulaName}={eq1.FormatDouble()} + {eq2.FormatDouble()}");
            document.Paragraphs.Add();
            document.AddEquation($"PFD_{topNode.FormulaName}={topNode.PFD.FormatDouble()}");
            document.Paragraphs.Add();
        }
        void AllOrs(IEnumerable<GraphicItem> nodes, OR topNode, Word.Document document)
        {
            var pfd1 = string.Join(" + ", nodes.Select(n => $"PFD_{n.FormulaName}"));
            document.AddEquation($"PFD_AVG={pfd1}");
            document.Paragraphs.Add();
            var pfd2 = string.Join(" + ", nodes.Select(n => $"{n.PFD.FormatDouble()}"));
            document.AddEquation($"PFD_AVG={pfd2}");
            document.Paragraphs.Add();
        }
        void MixedOrNodes(IEnumerable<GraphicItem> nodes, OR topNode, Word.Document document)
        {
            List<string> pfd1 = new List<string>();
            foreach(var nde in nodes)
            {
                if (nde is OR or1)
                {
                    pfd1.Add("PFD_" + nde.FormulaName);
                }
                else if (nde is Node)
                {
                    pfd1.Add($"(λ_{nde.FormulaName} × T)/2");
                }
            }
            document.AddEquation($"PFD_{topNode.FormulaName}={string.Join("+", pfd1)}");
            document.Paragraphs.Add();

            List<string> pfd2 = new List<string>();
            foreach (var nde in nodes)
            {
                if (nde is OR or1)
                {
                    pfd2.Add("PFD_" + nde.FormulaName);
                }
                else if (nde is Node)
                {
                    pfd2.Add($"({nde.BetaFreeLambda.FormatDouble()} × {nde.PTI})/2");
                }
            }
            document.AddEquation($"PFD_{topNode.FormulaName}={string.Join("+", pfd2)}");
            document.Paragraphs.Add();


            var pfd3 = string.Join(" + ", nodes.Select(n => $"{n.PFD.FormatDouble()}"));
            document.AddEquation($"PFD_{topNode.FormulaName}={pfd3}");
            document.Paragraphs.Add();
        }
    }

}
