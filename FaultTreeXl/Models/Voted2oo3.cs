using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// This class implements a voted 2 oo 3 set
    /// By generating the relevant cut sets
    /// </summary>
    public class Voted2oo3 : AND
    {
        public Voted2oo3(GraphicItem item1, GraphicItem item2, GraphicItem item3)
        {
            Nodes.Add(item1);
            Nodes.Add(item2);
            Nodes.Add(item3);
        }

        public Voted2oo3()
        {

        }
        [XmlIgnore]
        public override List<CutSet> CutSets
        {
            get
            {
                if (Nodes.Count > 2)
                {
                    var and1 = new AND() { Nodes = new ObservableCollection<GraphicItem> { Nodes[0], Nodes[1] } };
                    var and2 = new AND() { Nodes = new ObservableCollection<GraphicItem> { Nodes[1], Nodes[2] } };
                    var and3 = new AND() { Nodes = new ObservableCollection<GraphicItem> { Nodes[2], Nodes[0] } };
                    var or1 = new OR() { Nodes = new ObservableCollection<GraphicItem> { and1, and2, and3 } };
                    return or1.CutSets;
                }
                else
                {
                    // Not enough nodes to produce the correct cutsets
                    return (new OR()).CutSets;
                }
            }
        }


    }
}
