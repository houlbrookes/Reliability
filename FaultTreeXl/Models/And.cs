using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{


    public partial class AND : GraphicItem
    {
        public override string NodeType => "AND";

        [XmlIgnore]
        public AND TestValue {
            get => new AND() { Name = "AND1", Description = "Test Description", Beta=10.0, 
                               Nodes= new ObservableCollection<GraphicItem> 
                               { 
                                    new Node { Name="Node1", Lambda=1E-06M, PTI=8760, Beta=10.0, ProofTestEffectiveness=1.0M, LifeTime=87600, TotalFailRate=1E-05M, IsA=true},
                                    new Node { Name="Node2", Lambda=1E-06M, PTI=8760, Beta=10.0, ProofTestEffectiveness=1.0M, LifeTime=87600, TotalFailRate=1E-05M, IsA=true},
                               }
            };
    }

        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            {
                List<CutSetList> x = Nodes.Select(n => new CutSetList(n.CutSets)).ToList();
                var result = x.ANDCutSets();
                result.RemoveSuperSets();
                return result.ToList();
            }
        }

        /// <summary>
        /// Peform a cartesian join on the two lists of lists
        /// remove any duplicate entries at both list level and list of list level
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private List<CutSet> Merge(List<CutSet> A, List<CutSet> B)
        {
            // Cartesian join
            var result = 
                (from a in A
                    from b in B
                        select a.Concat(b));
            // convert back to CS
            return result.Select(l => l).ToList();
        }

        [XmlIgnore]
        public override decimal Lambda
        {
            get => CutSets.Sum(cs => cs.Lambda);
            set => base.Lambda = value; // dummy 
        }

        public override decimal BetaFreeLambda
        {
            get => CutSets.Sum(cs => cs.Lambda);
            set => base.BetaFreeLambda = value;
        }

        public override int ArchSIL => Nodes.Any(n => !n.IsCCF) ? Math.Min(Nodes.Where(n => !n.IsCCF).Sum(n => n.ArchSIL),4):1;

        public override (string, string) FormulaLambdaString()
        {
            // a list of formula strings for each node 
            var pfdStrings = from node in Nodes
                             let otherNodes = Nodes.Where(n => n != node)
                             let otherPFD = otherNodes.Select(n => n.FormulaString().Item2)
                             let pfdFormula = string.Join(" + ", otherPFD)
                             select $"λ_{node.Name} × {pfdFormula}";

            return ("AND formula", string.Join(" + ", pfdStrings));
        }

        public override (string, string) ValuesLambdaString()
        {
            // a list of formula strings for each node 
            var pfdStrings = from node in Nodes
                             let otherNodes = Nodes.Where(n => n != node)
                             let otherPFD = otherNodes.Select(n => n.ValuesString().Item2)
                             let pfdFormula = string.Join(" + ", otherPFD)
                             select $"{node.BetaFreeLambda.FormatDouble()} × {pfdFormula}";

            return ("AND formula", string.Join(" + ", pfdStrings));
        }

        public override (string, string) TotalLambdaString()
        {
            return base.TotalLambdaString();
        }

        internal override void UpdateBeta(double v)
        {
            foreach(var node in Nodes) node.UpdateBeta(v);
        }

        public bool hasIdenticalLegs()
        {
            if (Nodes.Count() > 0)
            {
                var n = Nodes.First();
                var l = n.BetaFreeLambda;
                var T = n.PTI;
                var L = n.LifeTime;
                var b = n.Beta;
                var c = n.ProofTestEffectiveness;
                return Nodes.All(node => node.BetaFreeLambda == l && node.PTI == T && node.LifeTime == L && node.Beta == b && node.ProofTestEffectiveness == c);
            }
            else
                return false;
        }

        public bool isPerfectProof()
        {
            if (Nodes.Count() > 0)
            {
                return Nodes.All(n => n.ProofTestEffectiveness == 1);
            }
            else
                return false;
        }
    }

    public partial class AND
    {
        public override SimulationState CalculateState(double currentClock)
        {
            SimulationState result = CurrentState;
            var timeElapsed = currentClock - LastEvent;

            var isWorking = Nodes.Any(n => n.CurrentState == SimulationState.Working);
            if (!isWorking && CurrentState == SimulationState.Working)
            {
                // Just failied
                CurrentState = SimulationState.Broken;
                Uptime += timeElapsed;
                FailureCount += 1;
                result = CurrentState;
                LastEvent = currentClock;

            }
            else if (isWorking && CurrentState == SimulationState.Broken)
            {
                // Just repaired
                CurrentState = SimulationState.Working;
                Downtime += timeElapsed;
                RepairCount += 1;
                result = CurrentState;
                LastEvent = currentClock;
            }
            else
            {
                result = CurrentState;
            }

            Notify(nameof(this.SimulatedFailureRate));
            Notify(nameof(this.SimulatedPFD));
            Notify(nameof(this.SimulatedMeanDowntime));

            Parent?.CalculateState(currentClock);

            return result;
        }
    }

}
