using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FaultTreeXl.Global;
using static System.Net.WebRequestMethods;

namespace FaultTreeXl
{


    public partial class AND : GraphicItem
    {
        public override string NodeType => ANDConstants.DEFAULT_AND_NODETYPE;

        [XmlIgnore]
        public AND TestValue
        {
            get => new AND()
            {
                Name = ANDConstants.DEFAULT_AND_NAME,
                Description = ANDConstants.DEFAULT_AND_DESCRIPTION,
                Beta = ANDConstants.DEFAULT_AND_BETA,
                Nodes = new ObservableCollection<GraphicItem>
                {
                     new Node { Name = NodeUtils.NextNodeName },
                     new Node { Name = NodeUtils.NextNodeName },
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

        public override int ArchSIL
        {
            get
            {
                int result = 0;
                if (Nodes.Any(n => !n.IsCCF))
                {
                    var nonCCFNodes = Nodes.Where(n => !n.IsCCF).Select(n => n.ArchSIL);
                    var result1 = nonCCFNodes.Max() + (nonCCFNodes.Count() - 1);
                    result = Math.Min(result1, 4); // Limit max value to SIL 4
                }
                return result;
            }
        }

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
            foreach (var node in Nodes) node.UpdateBeta(v);
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

        public override void SFFMatrix(int rowIndex, int colIndex, int childOffset = 1)
        {
            SFF_X = colIndex * 100.0;
            SFF_Y = rowIndex * 70.0;
            rowIndex += childOffset;
            var childIndex = Math.Max(Nodes.Count, 1);
            foreach (var node in Nodes)
            {
                node.SFFMatrix(rowIndex, colIndex, childIndex);
                rowIndex += 1;
            }
            SFFSpan = Nodes.Count() > 0 ? Nodes.Max(n => n.SFFSpan) : 1;
            SFFDepth = Nodes.Count() > 0 ? Nodes.Sum(n => n.SFFDepth) : 1;
            SFFWidth = SFFSpan * 100.0;
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
