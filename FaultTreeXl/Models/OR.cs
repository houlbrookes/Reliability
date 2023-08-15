using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using FaultTreeXl.Global;

namespace FaultTreeXl
{
    public partial class OR : GraphicItem
    {
        public override string NodeType => ORConstants.DEFAULT_OR_NODETYPE;


        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            // Just flatten the list of list of list of nodes
            // and remove duplicates
            {
                var allCutSets = new CutSetList(Nodes.SelectMany(n => n.CutSets));
                allCutSets.Distinct();
                allCutSets.RemoveSuperSets();
                return allCutSets.ToList();
            }
        }

        [XmlIgnore]
        public override decimal Lambda
        {
            get => Nodes.Sum(n => n.Lambda);
            set => base.Lambda = value;
        }

        public override decimal BetaFreeLambda 
        { 
            get => Nodes.Sum(n => n.BetaFreeLambda); 
            set => base.BetaFreeLambda = value; 
        }

        public override int ArchSIL => Nodes.Any(n => !n.IsCCF) ? Nodes.Where(n => !n.IsCCF).Min(n => n.ArchSIL) : 1;

        public override (string, string) FormulaString()
        {
            var result = ("", "Not calculated");
            if (!isVoted())
            {
                List<string> pfd1 = new List<string>();
                foreach (var nde in Nodes)
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
                result = ("Not Voted", $"PFD_{FormulaName}={string.Join("+", pfd1)}");
            }
            else
            {
                var Ands = Nodes.OfType<AND>();
                var theAnd = Ands.First();
                var nodes = Nodes.OfType<Node>();
                var nodeCount = theAnd.Nodes.Count();
                if (theAnd.isPerfectProof())
                {
                    if (theAnd.hasIdenticalLegs())
                    { // Perfect proof test and identical legs
                        result = ("Voted, c=1, Ident", $"PFD_{FormulaName}=(λ^{nodeCount} (1-β)^{nodeCount} T^{nodeCount})/{nodeCount + 1}+βλT/2");
                    }
                    else
                    { // Perfect proof test non-identical legs
                        var l_string = string.Join(".", theAnd.Nodes.Select(n => $"λ_{n.FormulaName}"));
                        result = ("Voted, c=1, non-ident", $"PFD_{FormulaName}=({l_string}.(1-β)^{nodeCount}.T^{nodeCount})/{nodeCount + 1}+βλT/2");
                    }
                }
                else
                { // imperfect proof testing
                    if (theAnd.hasIdenticalLegs())
                    {
                        result = ("Voted, c<>1, ident", $"PFD_{FormulaName}=(λ^2.(1-β)^2.(c^2.T^2 + (1-c)^2 L^2))/3 + (λ.β.(c.T + (1-c).L))/2");
                    }
                    else
                    {
                        var l_string = string.Join(".", theAnd.Nodes.Select(n => $"λ_{n.FormulaName}"));
                        result = ("Voted, c<>1, non-ident", $"PFD_{FormulaName}=({l_string}.(1-β)^{nodeCount}.(c^{nodeCount}.T^{nodeCount} + (1-c)^{nodeCount} L^{nodeCount}))/{nodeCount+1} + (λ_CCF.(c.T + (1-c).L))/2");
                    }
                }
            }
            return result;
        }

        public override (string, string) FormulaLambdaString()
        {
            if (Nodes.Count() > 0)
            {
                return ("Nodes", string.Join(" + ", Nodes.Select(n => n.FormulaLambdaString().Item2)));
            }
            else
                return ("OR (no nodes)", $"λ_{FormulaName}");
        }

        public override (string, string) ValuesString()
        {
            var result = ("", "No result calculated");
            if (!isVoted())
            {
                List<string> pfd2 = new List<string>();
                var calcResult = 0M;
                foreach (var nde in Nodes)
                {
                    calcResult += nde.PFD;
                    if (nde is OR)
                    {
                        pfd2.Add($"{nde.PFD.FormatDouble()}");
                    }
                    else if (nde is Node)
                    {
                        pfd2.Add($"({nde.BetaFreeLambda.FormatDouble()} × {nde.PTI})/2");
                    }
                }
                result = ("Not Voted", $"PFD_{FormulaName}={string.Join("+", pfd2)} \n ={calcResult.FormatDouble()}");
            }
            else
            {   // voted
                var Ands = Nodes.OfType<AND>();
                var theAnd = Ands.First();
                var nodes = Nodes.OfType<Node>();
                var theCCF = nodes.First();
                var nodeCount = theAnd.Nodes.Count();
                if (theAnd.isPerfectProof())
                {
                    var a1 = theAnd.Nodes.First();
                    if (theAnd.hasIdenticalLegs())
                    { // Perfect proof test and identical legs
                        var l = a1.BetaFreeLambda.FormatDouble();
                        var b = a1.Beta > 0 ? a1.Beta / 100 : 0.1; // use 10% if beta is zero
                        var T = a1.PTI;
                        var l_beta = (nodes.First().BetaFreeLambda / (decimal)b);

                        result = ("Voted, c=1, ident", $"PFD_{FormulaName}=(({l})^{nodeCount} × (1-{b})^{nodeCount} × {T}^{nodeCount})/{nodeCount + 1} + ({b} × ({l_beta.FormatDouble()}) × {T})/2 \n {theAnd.PFD.FormatDouble()} + {theCCF.PFD.FormatDouble()}");
                    }
                    else 
                    { // Perfect proof test non-identical legs
                        var a2 = nodes.First();
                        var l = a2.BetaFreeLambda.FormatDouble();
                        var b = a2.Beta > 0 ? a2.Beta / 100 : 0.1; // use 10% if beta is zero
                        var T = a2.PTI;
                        var l_beta = nodes.First().BetaFreeLambda / (decimal)b;
                        var l_stringN = string.Join(" × ", theAnd.Nodes.Select(n => $"{n.BetaFreeLambda.FormatDouble()}"));
                        result = ("Voted, c=1, non-ident", $"PFD_{FormulaName}=({l_stringN} × (1-{b})^{nodeCount} × {T}^{nodeCount})/{nodeCount + 1} + ({b} × ({l_beta.FormatDouble()}) × {T})/2 \n {theAnd.PFD.FormatDouble()} + {theCCF.PFD.FormatDouble()}");
                    }
                }
                else
                { // imperfect proof testing
                    var a2 = theAnd.Nodes.First();
                    var l = a2.BetaFreeLambda.FormatDouble();
                    var b = a2.Beta > 0 ? a2.Beta / 100 : 0.1; // use 10% if beta is zero
                    var c = (double)(a2.ProofTestEffectiveness ?? 1);
                    var T = (double)a2.PTI;
                    var L = (double)a2.LifeTime;
                    var l_beta = nodes.First().BetaFreeLambda / (decimal)b;
                    if (theAnd.hasIdenticalLegs())
                    {
                        var pfdAND = Math.Pow((double)a2.BetaFreeLambda, nodeCount);
                        pfdAND *= Math.Pow((1-b), nodeCount);
                        pfdAND *= Math.Pow(c, nodeCount) * Math.Pow(T, nodeCount) + Math.Pow((1 - c), nodeCount) * Math.Pow(L, nodeCount);
                        pfdAND /= nodeCount + 1;
                        var pfdCCF = (double)a2.BetaFreeLambda * (c*T + (1-c)*L) / 2;
                        var variables = $"Voted, c<>1, ident:";
                        variables += $"\nλ = {l}";
                        var formatPercentBeta = b.ToString("P");
                        variables += $"\nβ = {formatPercentBeta}";
                        variables += $"\nProof Test Interval, T = {T}";
                        variables += $"\nLift Time, L = {L}";
                        var formatPercentPTE = c.ToString("P");
                        variables += $"\nProof Test effectiveness, c = {formatPercentPTE}";
                        result = (variables, $"PFD_{FormulaName}=(({l})^{nodeCount} × (1-{b})^{nodeCount} × ({c}^{nodeCount} × {T}^{nodeCount} + (1-{c})^{nodeCount} × {L}^{nodeCount}))/{nodeCount + 1} + ({l} × {b} × ({c} × {T} + (1-{c}) × {L}))/2 \n = {pfdAND.FormatDouble()} + {pfdCCF.FormatDouble()} \n = {(pfdAND + pfdCCF).FormatDouble()}");
                    }
                    else
                    {
                        var l_string = string.Join(" × ", theAnd.Nodes.Select(n => $"{n.BetaFreeLambda.FormatDouble()}"));
                        var pfdAND = (double)theAnd.Nodes.Select(n => n.BetaFreeLambda).Product();
                        pfdAND *= Math.Pow((1 - b), nodeCount);
                        pfdAND *= Math.Pow(c, nodeCount) * Math.Pow(T, nodeCount) + Math.Pow((1 - c), nodeCount) * Math.Pow(L, nodeCount);
                        pfdAND /= nodeCount + 1;
                        var pfdCCF = (double)a2.BetaFreeLambda * (c * T + (1 - c) * L) / 2;
                        result = ("Voted, c<>1, non-ident", $"PFD_{FormulaName}=({l_string} × (1-{b})^{nodeCount} × ({c}^{nodeCount} × {T}^{nodeCount} + (1-{c})^{nodeCount} × {L}^{nodeCount}))/{nodeCount + 1} + ({l} × {b} × ({c} × {T} + (1-{c}) × {L}))/2 \n {pfdAND.FormatDouble()} + {pfdCCF.FormatDouble()} \n = {(pfdAND + pfdCCF).FormatDouble()}");
                    }

                }
            }
            return result;
        }

        public override (string, string) ValuesLambdaString()
        {
            return base.ValuesLambdaString();
        }

        public override (string, string) TotalLambdaString()
        {
            return base.TotalLambdaString();
        }

        private bool isVoted()
        {
            var ands = Nodes.OfType<AND>();
            var nodes = Nodes.OfType<Node>();
            return (ands.Count() == 1 && nodes.Count() == 1);
        }

    }

    /// <summary>
    /// Simulation Extensions
    /// </summary>
    public partial class OR
    {
        public override SimulationState CalculateState(double currentClock)
        {
            SimulationState result = CurrentState;
            var timeElapsed = currentClock - LastEvent;

            var isBroken = Nodes.Any(n => n.CurrentState == SimulationState.Broken);
            if (isBroken && CurrentState==SimulationState.Working)
            {
                // Just failied
                CurrentState = SimulationState.Broken;
                Uptime += timeElapsed;
                FailureCount += 1;
                result = CurrentState;
                Parent?.CalculateState(currentClock);
                LastEvent = currentClock;
            }
            else if (!isBroken && CurrentState == SimulationState.Broken)
            {
                // Just repaired
                CurrentState = SimulationState.Working;
                Downtime += timeElapsed;
                RepairCount += 1;
                result = CurrentState;
                Parent?.CalculateState(currentClock);
                LastEvent = currentClock;
            }


            Notify(nameof(this.SimulatedFailureRate));
            Notify(nameof(this.SimulatedPFD));
            Notify(nameof(this.SimulatedMeanDowntime));


            return result;
        }

        internal override void UpdateBeta(double v)
        {
            foreach (var node in Nodes)
                node.UpdateBeta(v);
        }
    }
}
