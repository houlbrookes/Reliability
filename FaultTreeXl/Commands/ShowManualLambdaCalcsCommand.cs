using System;
using System.Linq;
using System.Windows.Input;


namespace FaultTreeXl
{
    internal class ShowManualLambdaCalcsCommand : ICommand
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

        private string NodeLambdaFormula(Node theNode)
        {
            string result = "";
            if (theNode.Beta == 0)
            {
                result = $" λ_{theNode.FormulaName}";
            }
            else
            {
                result = $" λ_{theNode.FormulaName} × (1-β)";
            }

            return result;
        }
        private string NodeLambdaValue(Node theNode)
        {
            string result = "";
            if (theNode.Beta == 0)
            {
                result = $" {theNode.Lambda.FormatDouble()}";
            }
            else
            {
                result = $" {theNode.BetaFreeLambda.FormatDouble()} × (1-{theNode.Beta/100})";
            }

            return result;
        }

        private string ORLambdaFormula(OR theOR)
        {
            Func<GraphicItem, string> process = node =>
            {
                if (node is Node n1)
                {
                    return NodeLambdaFormula(n1);
                }
                else if (node is OR or1)
                {
                    return ORLambdaFormula(or1);
                }
                else if (node is AND and1)
                {
                    return ANDLambdaFormula(and1);
                }
                else
                    return "Not Implemented";

            };

            var formulas = from node in theOR.Nodes
                           select process(node);

            return string.Join(" + ", formulas);
        }

        private string ORLambdaValue(OR theOR)
        {
            Func<GraphicItem, string> process = node =>
            {
                if (node is Node n1)
                {
                    return NodeLambdaValue(n1);
                }
                else if (node is OR or1)
                {
                    return ORLambdaValue(or1);
                }
                else if (node is AND and1)
                {
                    return ANDLambdaValue(and1);
                }
                else
                    return "Not Implemented";

            };

            var formulas = from node in theOR.Nodes
                           select process(node);

            return string.Join(" + ", formulas);
        }

        private string ORLambdaResults(OR theOR)
        {
            var formulas = from node in theOR.Nodes
                           select node.Lambda.FormatDouble();

            return string.Join(" + ", formulas);
        }
        private string ANDLambdaFormula(AND theAND)
        {
            string result = "";

            if (theAND.Nodes.Count() == 2)
            {
                var n1 = theAND.Nodes.First();
                var n2 = theAND.Nodes.Skip(1).First();
                if (n1.Lambda == n2.Lambda)
                {
                    result = $" λ^2 (1-β)^2 T_i ";
                }
                else
                {
                    result = $" (1-β).λ_{n1.FormulaName}.((1-β).λ_{n2.FormulaName}.T_i)/2";
                    result += $" + (1-β).λ_{n2.FormulaName}.((1-β).λ_{n1.FormulaName}.T_i)/2";
                }
            }
            else if (theAND.Nodes.Count() == 3)
            {
                var n1 = theAND.Nodes.First();
                var n2 = theAND.Nodes.Skip(1).First();
                var n3 = theAND.Nodes.Skip(2).First();
                if (n1.Lambda == n2.Lambda)
                {
                    result = $" λ^3 (1-β)^3 (T_i)^2 ";
                }
                else
                {
                    result = $" (1-β).λ_{n1.FormulaName}.((1-β).λ_{n2.FormulaName}.(1-β).λ_{n3.FormulaName}.(T_i)^2)/3";
                    result += $" + (1-β).λ_{n2.FormulaName}.((1-β).λ_{n1.FormulaName}.(1-β).λ_{n3.FormulaName}.(T_i)^2)/3";
                    result += $" + (1-β).λ_{n3.FormulaName}.((1-β).(λ_{n1.FormulaName}.(1-β).λ_{n2.FormulaName}.(T_i)^2)/3";
                }
            }
            return result;
        }

        private string GetLambda(GraphicItem item)
        {
            if (item is Node n1)
            {
                if (n1.Beta > 0)
                    return $"(1 -{ n1.Beta / 100}) × {n1.BetaFreeLambda.FormatDouble()}";
                else
                    return $"{n1.BetaFreeLambda.FormatDouble()}";
            }
            else if (item is OR o1)
            {
                var list = from n in o1.Nodes
                           select $"{GetLambda(n)}";
                return "(" + string.Join(" + ", list) + ")";
            }
            else 
            {
                return "???";
            }
        }
        private string ANDLambdaValue(AND theAND)
        {
            string result = "";

            if (theAND.Nodes.Count() == 2)
            {
                var n1 = theAND.Nodes.First();
                var n2 = theAND.Nodes.Skip(1).First();
                if (n1.Lambda == n2.Lambda)
                {
                    result = $"({n1.BetaFreeLambda.FormatDouble()})^2 × (1-{n1.Beta/100})^2 × {n1.PTI} ";
                }
                else
                {
                    result = $" {GetLambda(n1)} × ({GetLambda(n2)} × {n2.PTI})/2";
                    result += $" + {GetLambda(n2)} × ({GetLambda(n1)} × {n1.PTI})/2";
                }
            }

            return result;
        }

        public void Execute(object parameter)
        {
            if (parameter is GraphicItem node)
            {
                if (node is Node n1)
                {
                    // Not used currently because nodes don't have menu option for this
                    GlobalWordApp.NewLine();
                    GlobalWordApp.Document.AddEquation($"λ = {NodeLambdaValue(n1)}");
                    GlobalWordApp.NewLine();
                }
                else if (node is OR or1)
                {
                    GlobalWordApp.NewLine();
                    GlobalWordApp.AddSection(ORLambdaFormula(or1));
                    GlobalWordApp.AddSection(ORLambdaValue(or1));
                    GlobalWordApp.AddSection(ORLambdaResults(or1));
                    GlobalWordApp.AddSection(or1.Lambda.FormatDouble());
                }
            }
        }
    }
}
