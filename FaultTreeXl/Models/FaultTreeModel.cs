using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// Main Model used to display the Fault Tree
    /// </summary>
    public class FaultTreeModel : NotifyPropertyChangedItem
    {
        private string _status = "Started";
        [XmlIgnore] public string Status { get => _status; set => Changed(ref _status, value); }

        [XmlIgnore] public ObservableCollection<GraphicItem> FaultTree { get; set; } = new ObservableCollection<GraphicItem>();
        private double width = 3000;
        [XmlIgnore] public double Width { get => width; set => Changed(ref width, value); }
        private double height = 800;
        [XmlIgnore] public double Height { get => height; set => Changed(ref height, value); }

        [XmlIgnore] public bool ShowingCutsets { get; set; } = false;
        [XmlIgnore] public GraphicItem SavedRootNode { get; set; }

        private string filename = "<no filename>";
        [XmlIgnore]
        public string Filename { get => filename; set => Changed(ref filename, value); }

        private decimal _proofTestEffectiveness = 1;
        [XmlIgnore]
        public decimal ProofTestEffectiveness { get => _proofTestEffectiveness; set => Changed(ref _proofTestEffectiveness, value); }

        private decimal _missionTime = 8760;
        [XmlIgnore]
        public decimal MissionTime { get => _missionTime; set => Changed(ref _missionTime, value); }

        public GraphicItem RootNode { get; set; }

        private GraphicItem _highlightedNode = null;
        [XmlIgnore]
        public GraphicItem HighlightedNode
        {
            get => _highlightedNode;
            set => Changed(ref _highlightedNode, value);
        }

        private bool _showLifeInfo = true;
        [XmlIgnore]
        public bool ShowLifeInfo
        {
            get => _showLifeInfo;
            set => Changed(ref _showLifeInfo, value);
        }

        [XmlIgnore]
        public ObservableCollection<StandardFailure> FailureRates { get; set; } = new ObservableCollection<StandardFailure> { };
        public static void LoadList(ObservableCollection<StandardFailure> list)
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
            //Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            //System.Reflection.Assembly.GetExecutingAssembly().Location
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardFailure>));
                ObservableCollection<StandardFailure> data = serializer.Deserialize(streamReader) as ObservableCollection<StandardFailure>;
                list.Clear();
                foreach (StandardFailure x in data)
                {
                    list.Add(x);
                }
            }
        }


        private void DrawGraphics(GraphicItem graphic)
        {
            graphic.PropertyChanged -= Graphic_PropertyChanged;
            graphic.PropertyChanged += Graphic_PropertyChanged;

            FaultTree.Add(graphic);
            foreach (GraphicItem n in graphic.Nodes)
            {
                DrawGraphics(n);
            }
        }

        private void Graphic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is GraphicItem node)
            {
                if (node.IsSelected)
                {
                    HighlightedNode = node;
                }
            }
        }

        public void ReDrawRootNode()
        {
            FaultTree.ToList().ForEach(n => FaultTree.Remove(n));
            DrawGraphics(RootNode);
            RootNode.AssignXY(0, 0);
            Width = (FaultTree.Max(n => n.X) + GraphicItem.GRAPHIC_WIDTH + 50) * Scale;
            Height = (FaultTree.Max(n => n.Y) + GraphicItem.GRAPHIC_HEIGHT) * Scale;
        }

        public int NextNodeName(string leadString)
        {
            int result = 1;
            var unNamedNodes = from node in FaultTree
                               let regEx = new Regex(leadString + @"\s+(\d+)")
                               let match = regEx.Match(node.Name)
                               let success = match.Success
                               let group1 = match.Groups[1]
                               where success
                               select new { num = int.Parse(group1.Value) };
            if (unNamedNodes.Count() > 0)
            {
                int number = unNamedNodes.Max(x => x.num);
                result = number;
            }

            return result;
        }

        private double _Scale = 1.0;
        [XmlIgnore]
        public double Scale
        {
            get => _Scale;
            set => Changed(ref _Scale, value);
        }

        public FaultTreeModel()
        {
            Node Node1 = new Node { Name = "A", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node2 = new Node { Name = "B", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node3 = new Node { Name = "C", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node4 = new Node
            {
                Name = "D",
                Description = "DU Fault",
                Lambda = (decimal)1E-6,
                PTI = 8760
            };
            Node Node5 = new Node { Name = "E", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node6 = new Node { Name = "F", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node7 = new Node { Name = "G", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node8 = new Node { Name = "H", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node9 = new Node { Name = "I", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node10 = new Node { Name = "J", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node11 = new Node { Name = "K", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node12 = new Node { Name = "CCF", Lambda = (decimal)1E-6, PTI = 8760 };
            Node Node13 = new Node { Name = "L", Lambda = (decimal)1E-6, PTI = 8760 };
            DiagnosedFaultNode Node14 = new DiagnosedFaultNode
            {
                Name = "M",
                Description = "DD Fault (72 hrs)",
                Lambda = (decimal)1E-6,
                PTI = 72
            };

            OR oR1 = new OR { Name = "OR 1", Description = "Root Node" };

            RootNode = oR1;

            OR oR2 = new OR { Name = "OR 2", Description = "Sensors" };

            Voted2oo3 oR3 = new Voted2oo3(Node2, Node3, Node13) { Name = "2oo3", Description = "Logic" };

            OR oR8 = new OR { Name = "OR 8", Description = "DU & DD" };
            oR8.Nodes.Add(Node4);
            oR8.Nodes.Add(Node14);

            OR oR4 = new OR { Name = "OR 4", Description = "Final Elements" };
            oR4.Nodes.Add(oR8);
            oR4.Nodes.Add(Node5);
            oR4.Nodes.Add(Node7);

            OR oR5 = new OR { Name = "OR 5" };
            oR5.Nodes.Add(Node1);
            oR5.Nodes.Add(Node6);

            OR oR6 = new OR { Name = "OR 6" };
            oR6.Nodes.Add(Node8);
            oR6.Nodes.Add(Node9);

            OR oR7 = new OR { Name = "OR 7" };
            oR7.Nodes.Add(Node10);
            oR7.Nodes.Add(Node11);


            AND oAND1 = new AND { Name = "AND 1" };
            oAND1.Nodes.Add(oR5);
            oAND1.Nodes.Add(oR6);
            oAND1.Nodes.Add(oR7);

            oR1.Nodes.Add(oR2);
            oR1.Nodes.Add(oR3);
            oR1.Nodes.Add(oR4);

            oR2.Nodes.Add(oAND1);
            oR2.Nodes.Add(Node12);

            ReDrawRootNode();

            Status = "Loaded with Dummy Model";

            RootNode.Recalculating += (sender, _notused) => Status = "Recalculating...";
            RootNode.CalculationsComplete += (sender, _notused) => Status = "Calculations complete";

            LoadList(FailureRates);
        }
    }
}
