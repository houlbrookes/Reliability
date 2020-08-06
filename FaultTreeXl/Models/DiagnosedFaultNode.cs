using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class DiagnosedFaultNode : GraphicItem
    {
        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            {
                return new List<CutSet>() { new CutSet(this) };
            }
        }

        public override decimal MDT => base.PTI;

    }
}
