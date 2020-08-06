using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FaultTreeXl
{
    public class CutSet : IEnumerable<GraphicItem>
    {
        const int INTEGRATION_PARTITIONS = 10000;

        private List<GraphicItem> _nodes = new List<GraphicItem>();
        public List<GraphicItem> Nodes { get => _nodes; set => _nodes = value; }

        public int Count
        {
            get => this.Count();
        }

        public CutSet Concat(CutSet cs) => new CutSet(Nodes.Union(cs.Nodes));

        public IEnumerator<GraphicItem> GetEnumerator() => Nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Nodes.GetEnumerator();

        public CutSet() { }

        public CutSet(params GraphicItem[] nodes) => Nodes.AddRange(nodes.Where(n => !Nodes.Contains(n)));

        public CutSet(IEnumerable<GraphicItem> nodes) => Nodes.AddRange(nodes.Where(n => !Nodes.Contains(n)));

        public override bool Equals(object obj)
        {
            if (obj is CutSet otherCS)
            {
                var b1 = Nodes.All(n => otherCS.Contains(n));
                var b2 = otherCS.All(n => Nodes.Contains(n));
                return b1 && b2;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSubset(CutSet otherCS)
        {
            return Nodes.All(n => otherCS.Contains(n));
        }

        public override int GetHashCode()
        {
            return AsString().GetHashCode();
        }

        public decimal PDFbyIntegration(double missionTime)
        {
            var functionList = this.Select(CutSetUtils.pfdFunc);
            // combine them into a single function
            var combinedFunction = functionList.Skip(1).Aggregate(functionList.First(), CutSetUtils.Multiply);
            // integrate over the mission time and divide by the mission time
            return (decimal)(MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(
                f: t => combinedFunction(t),
                intervalBegin: 0,
                intervalEnd: missionTime,
                numberOfPartitions: INTEGRATION_PARTITIONS) 
                        / missionTime);
        }
        public decimal PFD
        {
            get
            {
                decimal result = 1M;
                if (Count > 0)
                {
                    // split the cutset into nodes and DiagnosedNodes
                    var nodes = new CutSet(this.OfType<Node>());
                    var diagnosed = new CutSet(this.OfType<DiagnosedFaultNode>());
                    if (nodes.Count() > 0)
                    {// check for all items have same PTI
                        var pti = nodes.First().PTI;
                        var allHaveSamePTI = nodes.All(n => n.PTI == pti);
                        var allHavePerfectProofTest = nodes.All(n => n.ProofTestEffectiveness == 1);
                        var anyHaveForceIntegration = nodes.Any(n => n.ForceIntegration);
                        var maxMissionTime = nodes.Max(n => n.LifeTime);
                        if (allHaveSamePTI && allHavePerfectProofTest && !anyHaveForceIntegration)
                        {
                            // use simple calcs
                            result = (nodes.Product(g => g.Lambda * g.PTI) / (nodes.Count + 1));
                        }
                        else
                        {
                            // use integration
                            result = nodes.PDFbyIntegration((double)maxMissionTime);
                        }
                    }
                    if (diagnosed.Count()>0)
                    {
                        result *= diagnosed.Product(g => g.Lambda * g.MDT);
                    }
                }
                else
                {
                    // we have a choice of 1 or 0 for this, if we choose 1 then failure rate
                    // of a single node can use the same calculation as for n nodes
                    result = 1M;
                }
                return result;
            }
        }

        public decimal MDT { get => PFD / Lambda; }

        public CutSet Without(GraphicItem node)
            => new CutSet(this.Where(n => !n.Equals(node)));

        public decimal Lambda
        {
            get=> this.Sum(node => node.Lambda * Without(node).PFD);
        }

        decimal Product(Func<GraphicItem, decimal> func)
            => this.Aggregate(1M, (agg, item) => agg * func(item));

        public string AsString()
        {
            var sorted = new SortedList<string, GraphicItem>();
            Nodes.ForEach(
                n => {
                    if (!sorted.ContainsKey(n.Name))
                        sorted.Add(n.Name, n);
                    else
                        System.Diagnostics.Debug.Assert(false);
                });
            return string.Join("", sorted.Keys);
        }
    }

}
