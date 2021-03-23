using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// Main Model used to display the Fault Tree
    /// </summary>
    public class FaultTreeModel : NotifyPropertyChangedItem
    {
        public const string NO_FILENAME = "<no filename>";
        [XmlIgnore]
        public Random TheRadomBase { get; set; } = new Random();
        bool _resetEachSimulation = false;
        public bool ResetEachSimulation { get => _resetEachSimulation; set => Changed(ref _resetEachSimulation, value, updateDirty: false); }
        bool _showSimulationResults = false;
        public bool ShowSimulationResults { get => _showSimulationResults; set => Changed(ref _showSimulationResults, value, updateDirty: false); }


        private string _status = "Started";
        [XmlIgnore] public string Status { get => _status; set => Changed(ref _status, value, updateDirty: false); }

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

        private GraphicItem _rootNode = null;
        public GraphicItem RootNode { get => _rootNode; set => Changed(ref _rootNode, value); }

        public int SILLevelPFD { get => RootNode.PFD < (decimal)1E-04 ? 4 : RootNode.PFD < (decimal)1E-03 ? 3 : RootNode.PFD < (decimal)1E-02 ? 2 : RootNode.PFD < (decimal)1E-01 ? 1 : 0; }

        private GraphicItem _highlightedNode = null;
        [XmlIgnore]
        public GraphicItem HighlightedNode
        {
            get => _highlightedNode;
            set => Changed(ref _highlightedNode, value);
        }

        private bool _showLifeInfo = false;
        [XmlIgnore]
        public bool ShowLifeInfo
        {
            get => _showLifeInfo;
            set
            {
                Changed(ref _showLifeInfo, value);
                foreach (GraphicItem node in FaultTree)
                {
                    node.ShowLifeInfo = _showLifeInfo;
                }

            }
        }

        [XmlIgnore]
        public ObservableCollection<StandardFailure> FailureRates { get; set; } = new ObservableCollection<StandardFailure> { };
        public static void LoadList(ObservableCollection<StandardFailure> list)
        {
            try
            {
                string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
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
            catch (Exception)
            {
                //There should be a "StandardParts.xml" folder on the desktop
                MessageBox.Show("Something went wrong loading the standard parts");
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
            if (sender is GraphicItem graphicItem)
            {   // Update the derived SIL Level PFD when a derivative changes
                if (e.PropertyName == nameof(graphicItem.PFD)
                 || e.PropertyName == nameof(graphicItem.Lambda)
                 || e.PropertyName == nameof(graphicItem.PTI))
                {
                    Notify("SILLevelPFD");
                }
                //// Check to see if item is selected
                //if (graphicItem.IsSelected)
                //{
                //    HighlightedNode = graphicItem;
                //}
                // Something has changed so set the dirty flag
                Dirty = true;
            }
        }

        public void ReDrawRootNode()
        {
            FaultTree.ToList().ForEach(n => FaultTree.Remove(n));
            DrawGraphics(RootNode);
            RootNode.AssignXY(0, 0);
            Width = (FaultTree.Max(n => n.X) + GraphicItem.GRAPHIC_WIDTH + 50);
            Height = (FaultTree.Max(n => n.Y) + GraphicItem.GRAPHIC_HEIGHT) + 80;
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
        double _simulationIterations = 10000;
        public double SimulationIterations { get => _simulationIterations; set => Changed(ref _simulationIterations, value); }

        public FaultTreeModel()
        {
            Node Node1 = new Node { Name = "A", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node2 = new Node { Name = "B", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node3 = new Node { Name = "C", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node4 = new Node
            {
                Name = "D",
                Description = "DU Fault",
                Lambda = (decimal)1E-6,
                PTI = 8760,
            };
            Node Node5 = new Node { Name = "E", Lambda = (decimal)1E-6, PTI = 4380, Description = "PTI=1/2 year" };
            Node Node6 = new Node { Name = "F", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node7 = new Node { Name = "G", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node8 = new Node { Name = "H", Lambda = (decimal)1E-6, PTI = 4380, Description = "PTI=1/2 year" };
            Node Node9 = new Node { Name = "I", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node10 = new Node { Name = "J", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node11 = new Node { Name = "K", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node12 = new Node { Name = "CCF", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node13 = new Node { Name = "L", Lambda = (decimal)1E-6, PTI = 8760, Description = "" };
            Node Node14 = new Node
            {
                Name = "M",
                Description = "DD Fault (72 hrs)",
                Lambda = (decimal)1E-6,
                PTI = 72
            };

            OR oR1 = new OR { Name = "OR 1", Description = "Root Node" };

            RootNode = oR1;

            OR oR2 = new OR { Name = "OR 2", Description = "Sensors" };

            AND oR3 = new AND() { Name = "AND", Description = "Logic" };
            oR3.Nodes.Add(Node2);
            oR3.Nodes.Add(Node3);

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

            RootNode.Recalculating += (_, __) => Status = "Recalculating...";
            RootNode.CalculationsComplete += (_, __) => Status = "Calculations complete";
            FailureRates.CollectionChanged += (_, b) => Dirty = true;

            LoadList(FailureRates);
            Dirty = false;
        }

        [XmlIgnore]
        public BackgroundWorker simulationProcess = null;
        private void simulationProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (e.Argument is FaultTreeModel model)
            {
                double clock = 0D;
                double previousClock = 0D;

                var nodes = model.FaultTree.OfType<Node>();
                foreach (var node in model.FaultTree)
                {
                    node.LastEvent = 0D;
                    if (model.ResetEachSimulation)
                    {
                        node.Downtime = 0D;
                        node.Uptime = 0D;
                        node.CurrentState = SimulationState.Working;
                        node.FailureCount = 0;
                        node.RepairCount = 0;
                    }
                }

                foreach (var node in nodes) node.CalculateNextEvent(clock, model.TheRadomBase);

                const int cancellationSteps = 1000;
                for (var iteration = 0; iteration < model.SimulationIterations/cancellationSteps; iteration++)
                {
                    if (simulationProcess.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    for (var step = 0; step < cancellationSteps; step++)
                    {
                        try
                        {
                            var nextEvent = nodes.Min(n => n.NextEvent);
                            var nextEventNode = nodes.First(n => n.NextEvent <= nextEvent);
                            previousClock = clock;
                            clock = nextEvent;

                            nextEventNode.CalculateState(clock);

                            nextEventNode.CalculateNextEvent(clock, model.TheRadomBase);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error");
                        }
                    }
                }
            }
        }

        public void StartSimulation()
        {
            Status = "Simulation Running...";
            simulationProcess = new System.ComponentModel.BackgroundWorker();
            simulationProcess.WorkerSupportsCancellation = true;
            simulationProcess.DoWork += simulationProcess_DoWork;
            simulationProcess.RunWorkerCompleted += (_, __) => { Status = "Simulation Complete"; simulationProcess.Dispose(); };
            simulationProcess.RunWorkerAsync(this);
        }

        public void CancelSimulation()
        {
            if (simulationProcess != null)
            {
                simulationProcess.CancelAsync();
            }
        }
    }
}
