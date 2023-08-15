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
        // initial value of the stored filename
        public const string NO_FILENAME = "<no filename>";
        [XmlIgnore]
        public Random TheRadomBase { get; set; } = new Random();
        bool _resetEachSimulation = false;
        public bool ResetEachSimulation { get => _resetEachSimulation; set => Changed(ref _resetEachSimulation, value, updateDirty: false); }
        bool _showSimulationResults = false;
        public bool ShowSimulationResults
        {
            get => _showSimulationResults;
            set
            {
                Changed(ref _showSimulationResults, value, updateDirty: false);
                if (value)
                {
                    ShowArchConstraints = false;
                    Notify(nameof(ShowArchConstraints));
                    //ShowCalculationResults = false;
                }
                Notify(nameof(ShowCalculationResults));
            }
        }

        //private bool _showCalculationResults;

        public bool ShowCalculationResults
        {
            get => !(ShowArchConstraints || ShowSimulationResults);
            //set
            //{
            //    Changed(ref _showCalculationResults, value, nameof(ShowCalculationResults));
            //    if (value)
            //    {
            //        ShowArchConstraints = false;
            //        ShowSimulationResults = false;
            //    }
            //}
        }


        private bool _showArchContraints;

        public bool ShowArchConstraints
        {
            get => _showArchContraints;
            set
            {
                Changed(ref _showArchContraints, value, nameof(ShowArchConstraints));
                if (value)
                {
                    ShowSimulationResults = false;
                    Notify(nameof(ShowSimulationResults));
                    //ShowCalculationResults = false;
                }
                Notify(nameof(ShowCalculationResults));
            }
        }


        private string _status = "Started";
        [XmlIgnore] public string Status { get => _status; set => Changed(ref _status, value, updateDirty: false); }

        /// <summary>
        /// An observable list of GraphicItems (Nodes, ORs and ANDs)
        /// Used to display the Fault Tree in the main window, each grapicItem has an (X,Y) coordinate of
        /// where it should be displayed
        /// </summary>
        [XmlIgnore] public ObservableCollection<GraphicItem> FaultTree { get; set; } = new ObservableCollection<GraphicItem>();
        /// <summary>
        /// Width of the displayed Fault Tree, used for resizing the background canvas
        /// </summary>
        private double width = 3000;
        [XmlIgnore] public double Width { get => width; set => Changed(ref width, value); }
        private double height = 800;
        /// <summary>
        /// Height of the dispayed Fault Tree, used for resizing the background canvas
        /// </summary>
        [XmlIgnore] public double Height { get => height; set => Changed(ref height, value); }
        /// <summary>
        /// Set true when the display is showing the minimal cutset version of the tree
        /// Set false when the fault tree is being displayed
        /// </summary>
        [XmlIgnore] public bool ShowingCutsets { get; set; } = false;
        /// <summary>
        /// When switching to display the cutsets, the original tree is saved here
        /// so that it can be restored later
        /// </summary>
        [XmlIgnore] public GraphicItem SavedRootNode { get; set; }
        /// <summary>
        /// Current file being edited
        /// </summary>
        private string filename = "<no filename>";
        [XmlIgnore]
        public string Filename { get => filename; set => Changed(ref filename, value); }
        /// <summary>
        /// Generic value for proof test effectiveness (pte)
        /// After applying this value every node will receive the PTE value
        /// but they can be amended later on an individual basis
        /// </summary>
        private decimal _proofTestEffectiveness = 1;
        [XmlIgnore]
        public decimal ProofTestEffectiveness { get => _proofTestEffectiveness; set => Changed(ref _proofTestEffectiveness, value); }
        /// <summary>
        /// Mission time used when calculating the imperfect proof testing
        /// </summary>
        private decimal _missionTime = 8760;
        [XmlIgnore]
        public decimal MissionTime { get => _missionTime; set => Changed(ref _missionTime, value); }
        /// <summary>
        /// An anchor point for the root node in the tree
        /// Note that we cannot use this for display in a List View because an observable collection 
        /// is required
        /// </summary>
        private GraphicItem _rootNode = null;
        public GraphicItem RootNode { get => _rootNode; set => Changed(ref _rootNode, value); }
        /// <summary>
        /// Converts the Root Node PFD to a SIL Value (0 to 4)
        /// </summary>
        public int SILLevelPFD { get => RootNode.PFD < 1E-04M ? 4 : RootNode.PFD < 1E-03M ? 3 : RootNode.PFD < 1E-02M ? 2 : RootNode.PFD < 1E-01M ? 1 : 0; }
        /// <summary>
        /// Converts the Root Node Dangerous Failure Rate to a SIL Value (0 to 4)
        /// </summary>
        public int SILLevelPFH { get => RootNode.Lambda < 1E-8M ? 4 : RootNode.Lambda < 1E-7M ? 3 : RootNode.Lambda < 1E-6M ? 2 : RootNode.Lambda < 1E-5M ? 1 : 0; }
        /// <summary>
        /// Set to the currently highlighted node
        /// This functionality was largely removed because it inteferred with
        /// the drag and drop functions that were more important
        /// </summary>
        private GraphicItem _highlightedNode = null;
        [XmlIgnore]
        public GraphicItem HighlightedNode
        {
            get => _highlightedNode;
            set => Changed(ref _highlightedNode, value);
        }
        /// <summary>
        /// 
        /// </summary>
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
        public bool DifferentPTIs
        {
            get
            {
                bool result = false;
                var nodeOnlyList = FaultTree.OfType<Node>();
                if (nodeOnlyList.Count() > 0)
                {
                    var firstNodePTI = nodeOnlyList.FirstOrDefault()?.PTI;
                    result = !nodeOnlyList.All(node => node.PTI == firstNodePTI);
                }
                return result;
            }
        }
        /// <summary>
        /// Observable collection of Failure Rates used for Drag/Drop into nodes
        /// </summary>
        [XmlIgnore]
        public ObservableCollection<StandardFailure> FailureRates { get; set; } = new ObservableCollection<StandardFailure> { };
        /// <summary>
        /// Loads the FailureRates collection from an XML File
        /// </summary>
        /// <param name="list"></param>
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
        /// <summary>
        /// Save the FailureRates collection to an XML File
        /// </summary>
        public void SaveStandardFailures()
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
            using (var streamWriter = new StreamWriter(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardFailure>));
                serializer.Serialize(streamWriter, FailureRates);
            }
        }
        /// <summary>
        /// Populates/Repopulates FaultTree from the Root Node
        /// Note, it needs to remove/reattach the event callbacks on each nodes and the list itself
        /// </summary>
        /// <param name="graphic"></param>
        private void DrawGraphics(GraphicItem graphic)
        {
            graphic.PropertyChanged -= Graphic_PropertyChanged;
            graphic.PropertyChanged += Graphic_PropertyChanged;

            FaultTree.Add(graphic);
            // don't put the subnodes of a collapsed node into the displ
            if (!graphic.Collapsed)
                foreach (GraphicItem n in graphic.Nodes)
                {
                    DrawGraphics(n);
                }
        }
        /// <summary>
        /// Respond to an event generated by the FaultTree collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Graphic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is GraphicItem graphicItem)
            {   // Update the derived SIL Level PFD when a derivative changes
                if (e.PropertyName == nameof(graphicItem.PFD)
                 || e.PropertyName == nameof(graphicItem.Lambda)
                 || e.PropertyName == nameof(graphicItem.PTI))
                {
                    Notify(nameof(SILLevelPFD));
                    Notify(nameof(SILLevelPFH));
                    Notify(nameof(DifferentPTIs));

                }
                Dirty = true;
            }
        }
        /// <summary>
        /// Internal class used to build a map of the nodes
        /// so that it can be placed in the optimal place on the screen
        /// </summary>
        class TaggedNode
        {
            public int Depth { get; set; }
            public int Width { get; set; }
            public GraphicItem Node { get; set; }
            public double ShiftX { get; set; }


        }
        class DepthWidth : IComparable<DepthWidth>
        {
            public int Depth { get; set; }
            public int Width { get; set; }
            public int CompareTo(DepthWidth other)
            {
                return Width.CompareTo(other.Width);
            }
        }

        private (List<TaggedNode>, int) Map(GraphicItem node, List<TaggedNode> previous, int depth, int width)
        {
            previous.Add(new TaggedNode { Depth = depth, Width = width, Node = node });
            foreach (var n in node.Nodes)
            {
                (previous, width) = Map(n, previous, depth + 1, width);
                width += 1;
            }
            return (previous, width);
        }
        private DepthWidth WidestLevel(List<TaggedNode> nodes)
        {
            var res = from n in nodes
                      group n by n.Depth into depthGroup
                      let maxWidth = depthGroup.Count()
                      select new DepthWidth{ Depth=depthGroup.Key, Width=maxWidth };
            var maxItem = res.Max();
            return maxItem;
        }

        private void ShiftX(GraphicItem node, double xShift)
        {
            node.X -= xShift;
            foreach (var n in node.Nodes) 
                ShiftX(n, xShift);
        }
        private void ShiftXParent(GraphicItem node, double xShift)
        {
            if (node.Parent != null)
            {
                var parent = node.Parent;
                if (parent.Parent == null)
                { // Top Node
                    parent.X -= xShift;
                }
                else
                { // Middle Node
                    var index = parent.Nodes.IndexOf(node);
                    if (index == 0)
                    { // First Node of Parent.Parent
                        parent.X -= xShift;
                        ShiftXParent(parent, xShift);
                        // If any of the parent's siblings are singletons, move them too
                        foreach(var singleton in parent.Parent.Nodes.Where(n => !n.Nodes.Any()))
                        {
                            singleton.X -= xShift;
                        }
                    }
                }
            }
        }

        private void ResetTree(List<TaggedNode> taggedNodes, DepthWidth maxWidth)
        {
            // Position the max width row from x=0 to n*width + margin
            var x = 0D;
            foreach(var node in taggedNodes.Where(n => n.Depth == maxWidth.Depth))
            {
                node.ShiftX = node.Node.X - x;
                ShiftX(node.Node, node.ShiftX);
                ShiftXParent(node.Node, node.ShiftX);
                x += GraphicItem.GRAPHIC_WIDTH;
            }
        }

        public void ReDrawRootNode()
        {
            FaultTree.ToList().ForEach(n => FaultTree.Remove(n));
            DrawGraphics(RootNode);
            RootNode.AssignXY(0, 0);

            var map = Map(RootNode, new List<TaggedNode>(), 1, 1);
            //var deepestLevel = map.Max(n => n.Depth);
            //var widestLevel = WidestLevel(map);
            //ResetTree(map, widestLevel);

            Width = FaultTree.Max(n => n.X) + GraphicItem.GRAPHIC_WIDTH; // +50
            Height = FaultTree.Max(n => n.Y) + GraphicItem.GRAPHIC_HEIGHT; // +80
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
            OR oR1 = new OR { Name = "OR 1", Description = "Root Node" };

            RootNode = oR1;

            OR oR2 = new OR { Name = "OR 2", Description = "Sensors" };
            OR oR3 = new OR() { Name = "OR 3", Description = "Logic" };
            OR oR4 = new OR { Name = "OR 4", Description = "Final Elements" };

            oR1.Nodes.Add(oR2);
            oR1.Nodes.Add(oR3);
            oR1.Nodes.Add(oR4);

            ReDrawRootNode();

            Status = "Loaded with a blank model";

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
                for (var iteration = 0; iteration < model.SimulationIterations / cancellationSteps; iteration++)
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

        /// <summary>
        /// Place to store the version of the fault tree
        /// </summary>
        private string _version = "0.1"; // Start the version at 0.1
        [XmlAttribute]
        public string Version
        {
            get => _version;
            set => Changed(ref _version, value, nameof(Version));
        }
        private string _notes;
        /// <summary>
        /// General Notes about the fault tree
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => Changed(ref _notes, value, nameof(Notes));
        }


    }
}
