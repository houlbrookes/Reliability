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


    public class AND : GraphicItem
    {
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

    }
}
