using System.Collections.Generic;
using System.Linq;

namespace FaultTreeXl
{
    public class CutSetComparitor : IEqualityComparer<CutSet>
    {
        public bool Equals(CutSet x, CutSet y)
        {
            var b1 = x.All(n => 
                y.Contains(n));
            var b2 = y.All(n => 
                x.Contains(n));
            return b1 && b2;
        }

        public int GetHashCode(CutSet obj) => base.GetHashCode();
    }

}
