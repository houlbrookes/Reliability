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
        public bool UsedIntegration { get; set; } = false;
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
                            UsedIntegration = false;
                        }
                        else
                        {
                            // use integration
                            result = nodes.PDFbyIntegration((double)maxMissionTime);
                            UsedIntegration = true;
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

        public (decimal, decimal) PFDFactor()
        {
            var result = (1.0M, 1.0M);
            var nodeQty = (decimal)Count;
            var minPTI = this.Min(n => n.PTI);
            var maxPTI = this.Max(n => n.PTI);
            var noOfmaxPTI = this.Count(n => n.PTI == maxPTI);
            var ratioOfPTI = maxPTI / minPTI;

            if (this.Any(n => minPTI != n.PTI && maxPTI != n.PTI))
            {
                // Cannot handle this calc at the moment
            }
            else if (minPTI == maxPTI)
            {
                // all nodes with the same PTI
                return (1.0M/(nodeQty + 1M), 1M);
            }
            else if (nodeQty == 2 )
            {
                // 1oo2 mixed PTI
                // (3n+1)/12
                return ((3.0M * ratioOfPTI + 1.0M) / 12.0M, (ratioOfPTI + 1.0M)/2);
            }
            else if (nodeQty == 3 && noOfmaxPTI == 1)
            {
                // 1oo3 mixed PTI only one maxPTI/Life
                return ((2.0M * ratioOfPTI + 1.0M) / 12.0M, (ratioOfPTI + 1.0M) / 2);
            }
            else if (nodeQty == 3 && noOfmaxPTI == 2)
            {
                // 1oo3 mixed PTI 2 maxPTI/Life
                return (ratioOfPTI * (2.0M * ratioOfPTI + 1.0M) / 12.0M, (ratioOfPTI + 1)*(2*ratioOfPTI + 1.0M) / 6);
            }
            else if (nodeQty == 4 && noOfmaxPTI == 1)
            {
                // 1oo3 mixed PTI 
                return ((5.0M * ratioOfPTI + 3.0M) / 40.0M, (ratioOfPTI + 1.0M) / 2);
            }
            else if (nodeQty == 4 && noOfmaxPTI == 2)
            {
                // 1oo4 mixed PTI
                return ((20 * ratioOfPTI * ratioOfPTI + 15.0M * ratioOfPTI + 1.0M) / 180.0M, (ratioOfPTI + 1) * (2 * ratioOfPTI + 1.0M) / 6);
            }
            else if (nodeQty == 4 && noOfmaxPTI == 3)
            {
                // 1oo4 mixed PTI
                return ((15 * ratioOfPTI * ratioOfPTI * ratioOfPTI + 10 * ratioOfPTI * ratioOfPTI - 1.0M) / 120.0M, ratioOfPTI * (ratioOfPTI + 1.0M) * (ratioOfPTI + 1.0M) / 4);
            }
            else
            {
                // two many nodes for current calculations
            }
            return result;
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
