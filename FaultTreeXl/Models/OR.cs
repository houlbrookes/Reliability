using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class OR : GraphicItem
    {
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
    }
}
