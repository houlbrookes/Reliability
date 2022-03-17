using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class Node : GraphicItem
    {
        public override string NodeType => "Node";

        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            {
                List<CutSet> result = null;
                decimal c = ProofTestEffectiveness??1M;
                decimal LT = LifeTime??PTI * 10M; // default 10 years
                if (c != 1M)
                {
                    // Split each node into two: one for Proof Test and one for Mission Time replacement
                    var Effective = new Node { Name = Name + "_P", Description = Description, Lambda = Lambda * c, PTI = PTI };
                    var NotEffective = new Node { Name = Name + "_L", Description = Description, Lambda = Lambda * (1 - c), PTI = LT };
                    result = new List<CutSet> { new CutSet(Effective), new CutSet(NotEffective) };
                }
                else
                {
                    // Perfect proof testing so return the node
                    result = new List<CutSet> { new CutSet ( this )};
                }
                return result;
            }
        }
        internal override void UpdateBeta(double v)
        {
            Beta=v;
        }
    }
}
