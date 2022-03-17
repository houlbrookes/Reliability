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

        public int SILLevelPFD { get => RootNode.PFD < 1E-04M ? 4 : RootNode.PFD < 1E-03M ? 3 : RootNode.PFD < 1E-02M ? 2 : RootNode.PFD < 1E-01M ? 1 : 0; }
        public int SILLevelPFH { get => RootNode.Lambda < 1E-8M ? 4 : RootNode.Lambda < 1E-7M ? 3 : RootNode.Lambda < 1E-6M ? 2 : RootNode.Lambda < 1E-5M ? 1 : 0; }

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

        public void SaveStandardFailures()
        {
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StandardParts.xml");
            using (var streamWriter = new StreamWriter(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardFailure>));
                serializer.Serialize(streamWriter, FailureRates);
            }
        }

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
    }
}
