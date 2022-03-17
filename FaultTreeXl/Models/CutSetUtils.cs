using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FaultTreeXl
{
    public static class CutSetUtils
    {
        /// <summary>
        /// Shorthand for Func<double, double>
        /// </summary>
        /// <param name="t">time</param>
        /// <returns></returns>
        public delegate double CutSetCalc(double t);

        /// <summary>
        /// Create the PFD function from a Node
        /// </summary>
        /// <param name="g">Node</param>
        /// <returns></returns>
        public static CutSetCalc pfdFunc(GraphicItem g)
            => new CutSetCalc((t => (t - Math.Floor(t / (double)g.PTI) * (double)g.PTI) * (double)g.Lambda));
        
        /// <summary>
        /// Combine two PFD functions
        /// </summary>
        /// <param name="a">first function</param>
        /// <param name="b">second function</param>
        /// <returns></returns>
        public static CutSetCalc Multiply(CutSetCalc a, CutSetCalc b) => 
            new CutSetCalc(t => a(t) * b(t));

        /// <summary>
        /// Takes an item out of a list and returns the new list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cs">list of items</param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<T> Without<T>(this List<T> cs, T node) => cs.Except(new List<T> { node });

        public static decimal ANDFailureRate(this List<GraphicItem> cs, double missionTime)
            => cs.Count() > 1
             ? cs.Sum(node => node.Lambda * cs.Without(node).CutSetPFD(missionTime))
             : cs.Count() == 1
               ? cs.First().Lambda
               : 0M;

        /// <summary>
        /// Return the PFD for a Cut Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cutset"></param>
        /// <param name="missionTime"></param>
        /// <returns></returns>
        public static decimal CutSetPFD<T>(this T cutset, double missionTime) where T : IEnumerable<GraphicItem>
        {
            const int INTEGRATION_PARTIONS = 10000;
            // create a list of pfd functions
            var functionList = cutset.Select(pfdFunc);
            // combine them into a single function
            var combinedFunction = functionList.Skip(1).Aggregate(functionList.First(), Multiply);

            // integrate over the mission time and divide by the mission time
            return (decimal)(MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(
                           f: t => combinedFunction(t),
                           intervalBegin: 0,
                           intervalEnd: missionTime,
                           numberOfPartitions: INTEGRATION_PARTIONS) / missionTime);
        }

        public static IEnumerable<IEnumerable<T>> Combs<T>(IEnumerable<T> theSet, int size)
        {
            if (size == 1)
            {
                foreach (var c in theSet)
                    yield return new List<T> { c };
            }
            else if (size == theSet.Count())
            {
                yield return theSet;
            }
            else
            {
                var first = theSet.First();
                var restOfItems = theSet.Skip(1);
                foreach (var combRest in Combs(restOfItems, size - 1))
                {
                    var result = new List<T> { first };
                    result.AddRange(combRest);
                    yield return result;
                }

                if (restOfItems.Count() >= size)
                    foreach (var nnn in Combs(restOfItems, size))
                        yield return nnn;
            }
        }


        public static decimal Product(this IEnumerable<decimal> fromList)
        {
            return fromList.Aggregate((decimal)1, (a, b) => a * b);
        }

        public static string AsString(this List<CutSet> cutSetList)
        {
            var sorted = new SortedList<string, CutSet>();
            cutSetList.ForEach(cs => { if (!sorted.ContainsKey(cs.AsString())) sorted.Add(cs.AsString(), cs); });
            return string.Join("∪", sorted.Keys);
        }

        public static string AsString(this List<List<CutSet>> cutSetList) 
        {
            return "("+string.Join(") (", cutSetList.Select(cs => cs.AsString()))+")";
        }

        public static (T, List<T>) HeadTail<T>(this List<T> csList)
        {
            return (csList.First(), csList.Skip(1).ToList());
        }

        public static CutSetList CartesianJoin(this CutSetList body, CutSetList other)
        {
            var result = from a in body
                         from b in other
                         select new CutSet(a).Concat(b);

            return new CutSetList(result);
        }


        public static CutSetList ANDCutSets(this List<CutSetList> theCutsets)
        {
            var result = new CutSetList();
            if (theCutsets.Count > 1)
            {
                (var first, var rest) = theCutsets.HeadTail();
                result = rest.ANDCutSets();
                result = result.CartesianJoin(first);
            }
            if (theCutsets.Count == 1)
            {
                return theCutsets.First();
            }



            return result;
        }
    }
}

