using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FaultTreeXl
{
    public class CutSetList : IEnumerable<CutSet>
    {
        public List<CutSet> CutSets = new List<CutSet>();

        public IEnumerator<CutSet> GetEnumerator()
        {
            return CutSets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CutSets.GetEnumerator();
        }

        public CutSetList() { }

        public CutSetList(CutSetList otherCutSetList)
        {
            CutSets.AddRange(otherCutSetList.CutSets);
        }

        public CutSetList(IEnumerable<CutSet> cutSets)
        {
            CutSets.AddRange(cutSets);
        }

        public CutSetList DeepCopy()
        {
            CutSetList result = new CutSetList(this);
            return result;
        }

        public CutSetList Add(CutSet cutSet)
        {
            CutSetList result = DeepCopy();
            //todo: check this is ok
            if (!result.Any(x => x.IsSubset(cutSet)))
            {
                var rejects = new CutSetList();
                foreach (var y in result)
                {
                    if (cutSet.IsSubset(y))
                    {
                        rejects.Add(y);
                    }
                }
                result.Remove(rejects);
                result.CutSets.Add(cutSet);
            }
            CutSets = result.CutSets;
            return result;
        }

        public CutSetList Add(CutSetList cutSetList)
        {
            CutSetList result = this.DeepCopy();
            result.CutSets.AddRange(cutSetList.CutSets);
            return result;
        }

        public void Remove(CutSet cutSet)
        {
            CutSets.Remove(cutSet);
        }

        public void Remove(CutSetList cutSetList)
        {
            CutSets = CutSets.Except(cutSetList).ToList();
        }

        public void Distinct()
        {
            CutSets = CutSets.Distinct(new CutSetComparitor()).ToList();
        }

        public void RemoveSuperSets()
        {
            var result = new CutSetList();
            foreach (var cs in CutSets)
            {
                if (!result.Any(x => x.IsSubset(cs)))
                {
                    var rejects = new CutSetList();
                    foreach (var y in result)
                    {
                        if (cs.IsSubset(y))
                        {
                            rejects.Add(y);
                        }
                    }

                    result.Remove(rejects);
                    result.Add(cs);
                }
            }
            CutSets = result.ToList();
        }

    }
}
