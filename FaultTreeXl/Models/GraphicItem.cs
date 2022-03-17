using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    /// <summary>
    /// This is the base class for the Fault Tree objects
    /// It is split into two definitions
    /// 1. 
    /// </summary>
    [XmlInclude(typeof(DiagnosedFaultNode))]
    [XmlInclude(typeof(Voted2oo3))]
    [XmlInclude(typeof(OR))]
    [XmlInclude(typeof(AND))]
    [XmlInclude(typeof(Node))]
    public partial class GraphicItem : NotifyPropertyChangedItem
    {
        public event EventHandler Recalculating;
        public event EventHandler CalculationsComplete;

        public ObservableCollection<GraphicItem> Nodes { get; set; } = new ObservableCollection<GraphicItem> { };

        public GraphicItem()
        {

        }
        /// <summary>
        /// Constructor allowing 
        /// </summary>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        public GraphicItem(double x2, double x3)
        {
            X2 = x2;
            X3 = x3;
        }

        /// <summary>
        /// Any two Nodes with the same name are considered to be equal
        /// Watch out for items with the same name and different failure/PTI rates
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = base.Equals(obj);
            if (obj is GraphicItem node)
            {
                result = Name.Equals(node.Name);
            }
            return result;
        }

        /// <summary>
        /// Indicates that the node is shown collapsed
        /// Should only affect ORs and ANDs
        /// </summary>
        private bool _collapsed = false; // show node by default
        [XmlIgnore]
        public bool Collapsed
        {
            get => _collapsed;
            set => Changed(ref _collapsed, value, nameof(Collapsed));
        }


        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        private string _name = "Node";
        [XmlAttribute]
        public string Name
        {
            get => _name;
            set
            {
                Changed(ref _name, value);
                Notify(nameof(IsCCF));
            }
        }

        public string FormulaName { get => _name.Replace("_", "").Replace(" ", "").Replace("+", ""); }

        /// <summary>
        /// Identifies node as Common Cause Factor
        /// </summary>
        [XmlIgnore]
        public bool IsCCF
        {
            get => this.Name.Contains("CCF");
        }

        private string _description = "No description";
        [XmlAttribute]
        public string Description { get => _description; set => Changed(ref _description, value); }

        private string _makeModel = "Generic";
        public string MakeModel { get => _makeModel; set => Changed(ref _makeModel, value); }

        private decimal _totalFailRate;
        public decimal TotalFailRate
        {
            get => _totalFailRate>0? _totalFailRate : BetaFreeLambda;
            set => Changed(ref _totalFailRate, value, nameof(TotalFailRate));
        }

        private bool _isA;

        public bool IsA
        {
            get => _isA;
            set => Changed(ref _isA, value, nameof(IsA));
        }

        public double SFF { get => (double)(1M-BetaFreeLambda/TotalFailRate); }
        public virtual int ArchSIL
        {
            get
            {
                if (IsA)
                {
                    if (SFF < 0.60) return 1;
                    else if (SFF < 0.90) return 2;
                    else if (SFF < 0.99) return 3;
                    else return 4;
                }
                else
                {
                    if (SFF < 0.90) return 1;
                    else if (SFF < 0.99) return 2;
                    else return 3;
                }
            }
        }

        [XmlIgnore]
        virtual public List<CutSet> CutSets { get; } = new List<CutSet>();

        /// <summary>
        /// Useful for debugging
        /// Returns a string representation of the cut sets 
        /// (Work is done in CutSet extension class)
        /// </summary>
        [XmlIgnore]
        public string CutSetsAsString { get => CutSets.AsString(); }

        private double _beta = 0.0;
        public double Beta { get => _beta; set => Changed(ref _beta, value); }

        private decimal _lambda = 0;
        /// <summary>
        /// Failure rate (often referred to as Lambda
        /// </summary>
        [XmlAttribute]
        virtual public decimal Lambda
        {
            get => _lambda * ((decimal)(1.0 - Beta / 100));
            set
            {
                if (Beta > 0)
                {
                    Changed(ref _lambda, value / ((decimal)(1.0 - Beta / 100)));
                }
                else
                {
                    Changed(ref _lambda, value);
                }

                Notify("PFD");
                Parent?.Recalculating?.Invoke(this, EventArgs.Empty);
                UpdateParent();
                CalculationsComplete?.Invoke(this, EventArgs.Empty);
            }
        }
        public virtual decimal BetaFreeLambda
        {
            get => _lambda;
            set
            {
                Changed(ref _lambda, value, nameof(Lambda));
                Notify(nameof(BetaFreeLambda));
            }
        }

        private decimal _pTI = 8760;
        /// <summary>
        /// Proof Test Interval in Hours
        /// </summary>
        [XmlAttribute]
        virtual public decimal PTI
        {
            get => _pTI;
            set
            {
                Changed(ref _pTI, value);
                Notify("PFD");
                UpdateParent();
            }
        }

        internal virtual void UpdateBeta(double v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Mean Downtime
        /// Uses the simple forumla of PFD/Failure Rate
        /// </summary>
        [XmlAttribute]
        virtual public decimal MDT { get => Lambda > 0 ? PFD / Lambda : 0; }

        private decimal? _proofTestEffectiveness = 1;
        [XmlElement(IsNullable = true)]
        public decimal? ProofTestEffectiveness { get => _proofTestEffectiveness; set => Changed(ref _proofTestEffectiveness, value); }

        private decimal? _lifeTime = 8760 * 10;
        [XmlElement(IsNullable = true)]
        public decimal? LifeTime { get => _lifeTime; set => Changed(ref _lifeTime, value); }

        [XmlIgnore]
        public virtual string NodeType { get => "GraphicItem"; }

        /// <summary>
        /// Probability of Failure on Demand
        /// Most of the work is done in the the CutSets Extension class
        /// the formula used is simple: OR together the PFD from each Minimal Cut Set
        /// i.e. Multiply them together and subtract from 1.
        /// This is only possible because I'm using a Decimal floating point, 
        /// the accuracy of Decimals is 128-bit floating point giving 28 to 29 decimal point accuracy
        /// </summary>
        [XmlIgnore]
        public decimal PFD
        {
            // calculated from the cutsets of the OR/AND/Node
            // commented out correct version 
            //get => 1M - CutSets.Select(cs => (1-cs.PFD)).Product();
            // dumbed-down version for simple calcs
            get => CutSets.Sum(cs => cs.PFD);
        }

        private bool _forceIntegration = false;
        [XmlIgnore]
        public bool ForceIntegration
        {
            get => _forceIntegration;
            set => Changed(ref _forceIntegration, value);
        }

        public virtual (string, string) FormulaString()
        {
            if (Beta > 0)
            {
                return ("Standard 1oo1 DU (beta)", $"((1-β).λ_{FormulaName}.T)/2");
            }
            else
            {
                return ("Standard 1oo1 DU", $"(λ_{FormulaName}.T)/2");
            }
        }

        public virtual (string, string) FormulaLambdaString()
        {
            if (Beta > 0)
            {
                if (ProofTestEffectiveness>0)
                return ("Standard 1oo1 DU Lambda β", $"(1-β).λ_{FormulaName}");
                else
                    return ("Standard 1oo1 DU Lambda β", $"(1-β).λ_{FormulaName}");
            }
            else
            {
                return ("Standard 1oo1 DU Lambda", $"λ_{FormulaName}");
            }

        }

        public virtual (string, string) ValuesString()
        {
            if (Beta == 0)
            {
                if (ProofTestEffectiveness == 1)
                {
                    return ("Standard 1oo1 DU", $"({Lambda.FormatDouble()} × {PTI})/2");
                }
                else
                {
                    return ("", $"({ProofTestEffectiveness} × {Lambda.FormatDouble()} × {PTI})/2");
                }
            }
            else
            {
                if (ProofTestEffectiveness == 1)
                {
                    return ("Standard 1oo1 DU", $"((1-{Beta/100}) × {BetaFreeLambda.FormatDouble()} × {PTI})/2");
                }
                else
                {
                    return ("", $"({ProofTestEffectiveness} × (1-{Beta/100.0}) × {BetaFreeLambda.FormatDouble()} × {PTI})/2");
                }
            }
        }

        public virtual (string, string) ValuesLambdaString()
        {
            if (Nodes.Count() > 0)
            {
                var indivdualNodes = Nodes.Select(n => n.ValuesLambdaString().Item2);

                return ("Nodes", string.Join(" + ", indivdualNodes));
            }
            else
                return ("Standard 1oo1 DU", $"{Lambda.FormatDouble()}");
        }

        public virtual (string, string) TotalString()
        {
            return ("From Calculated PFD", $"PFD_{FormulaName} = {PFD.FormatDouble()}");
        }

        public virtual (string, string) TotalLambdaString()
        {
            return ("From Calculated PFD", $"λ_{FormulaName}={BetaFreeLambda.FormatDouble()} × {Beta/100D}");
        }

        /// <summary>
        /// Update all the items above this one in the tree
        /// </summary>
        public void UpdateParent()
        {
            Recalculating?.Invoke(this, EventArgs.Empty);
            Notify("PFD");
            Notify("Lambda");
            //Notify("PTI");
            Parent?.UpdateParent();
            CalculationsComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Graphical elements of the Graphic Item
    /// </summary>
    public partial class GraphicItem
    {
        public const double GRAPHIC_HEIGHT = 145;
        public const double GRAPHIC_WIDTH = 110;

        [XmlIgnore]
        public GraphicItem Parent { get; set; }

        /// <summary>
        /// The Rectangle with the details in it
        /// </summary>
        [XmlIgnore]
        public Microsoft.Office.Interop.Visio.Shape Rectangle { get; set; }

        /// <summary>
        /// Node dependent shape (AND, OR, Node)
        /// </summary>
        [XmlIgnore]
        public Microsoft.Office.Interop.Visio.Shape BodyShape { get; set; }
        /// <summary>
        /// Assign the X,Y values to each node
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        virtual public double AssignXY(double x1, double y1)
        {
            double newX = x1;
            //double prevX = x1;
            if (!Collapsed)
            {
                GraphicItem[] nodeArray = Nodes.ToArray();
                for (int i = 0; i < nodeArray.Length; i++)
                {
                    GraphicItem g = nodeArray[i];
                    g.Parent = this;
                    if ((g.Nodes.Count > 0) || (i < 1))
                    {
                        newX = g.AssignXY(newX, y1 + GRAPHIC_HEIGHT);
                    }
                    else
                    {
                        double NodeX = g.AssignXY(nodeArray[i - 1].X + GRAPHIC_WIDTH, y1 + GRAPHIC_HEIGHT);
                        // don't update newX unless the NodeX is larger than newX
                        newX = Math.Max(NodeX, newX);
                    }
                    //prevX = newX;
                }

                if (Nodes.Count > 0)
                {   // this is a parent node
                    // Centre the item within its children
                    // and set up the X2/3 for the connecting line
                    X = (nodeArray[0].X + nodeArray[nodeArray.Length - 1].X) / 2;
                    this.Y = y1;

                    this.X2 = Nodes.First().X + 50 - X;
                    this.X3 = Nodes.Last().X + 50 - X;
                    return Math.Max(newX, GRAPHIC_WIDTH);
                }
                else
                {   // this is a Node (leaf)
                    // leave the item where it is (newX, y1)
                    this.X = newX;
                    this.Y = y1;
                    return newX + GRAPHIC_WIDTH;
                }
            }
            else
            {
                this.X = newX;
                this.Y = y1;
                return newX + GRAPHIC_WIDTH;
            }
        }

        [XmlIgnore]
        // used for displaying the tree
        public double Mod = 0;

        /// <summary>
        /// Top Left Hand Corner
        /// </summary>
        private double _x = 0D;
        [XmlIgnore] public double X { get => _x; set => Changed(ref _x, value); }

        private double _y = 0D;
        [XmlIgnore] public double Y { get => _y; set => Changed(ref _y, value); }

        /// <summary>
        /// Extent of the left line
        /// </summary>
        private double _x2 = 0D;
        [XmlIgnore] public double X2 { get => _x2; set => Changed(ref _x2, value); }

        private double _x3 = 0D;
        [XmlIgnore] public double X3 { get => _x3; set => Changed(ref _x3, value); }

        private bool _showLifeInfo = false;
        [XmlIgnore]
        public bool ShowLifeInfo
        {
            get => _showLifeInfo;
            set => Changed(ref _showLifeInfo, value);
        }

    }

    /// <summary>
    /// Extension to capture discrete simulation
    /// </summary>
    public partial class GraphicItem : NotifyPropertyChangedItem, IDataErrorInfo
    {
        [XmlIgnore]
        public SimulationState CurrentState = SimulationState.Working;
        [XmlIgnore]
        public double LastEvent = 0D;
        private long _failureCount = 0;
        [XmlIgnore]
        public long FailureCount { get => _failureCount; set => Changed(ref _failureCount, value, updateDirty: false); }
        private long _repairCount = 0;
        [XmlIgnore]
        public long RepairCount { get => _repairCount; set => Changed(ref _repairCount, value, updateDirty: false); }
        [XmlIgnore]
        public double Downtime = 0D;
        [XmlIgnore]
        public double Uptime = 0D;

        private double _nextEvent = 0D;
        [XmlIgnore]
        public double NextEvent { get => _nextEvent; set => Changed(ref _nextEvent, value); }

        [XmlIgnore]
        public double SimulatedPFD
        {
            get
            {
                if (Uptime + Downtime > 0D)
                    return Downtime / (Uptime + Downtime);
                else
                    return 0D;
            }
        }
        [XmlIgnore]
        public double SimulatedFailureRate
        {
            get
            {
                if ((Uptime + Downtime) > 0D)
                    return FailureCount / (Uptime + Downtime);
                else
                    return 0D;
            }
        }
        [XmlIgnore]
        public double SimulatedMeanDowntime
        {
            get
            {
                if (FailureCount > 0)
                    return Downtime / FailureCount;
                else
                    return 0D;
            }
        }

        //
        // Routines to support validation of edited objects
        //
        public string Error => Errors.Count > 0 ? Errors.First().Value : string.Empty;

        private Dictionary<string, string> Errors = new Dictionary<string, string>();
        private string SetError(string fieldName, string errorString)
        {
            Errors[fieldName] = errorString;
            return errorString;
        }
        private void ClearError(string fieldName)
        {
            if (Errors.ContainsKey(fieldName)) Errors.Remove(fieldName);
        }
        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == nameof(this.Name))
                {
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        result = SetError(nameof(Name), "Please enter a value for this field");
                    }
                    else
                    {
                        ClearError(nameof(Name));
                    }
                }
                if (columnName == nameof(BetaFreeLambda))
                {
                    if (BetaFreeLambda < 0 || BetaFreeLambda > 0.01M)
                    {
                        result = SetError(nameof(BetaFreeLambda), "Please enter a value for Lambda between 0 and 0.01");
                    }
                    else
                    {
                        ClearError(nameof(BetaFreeLambda));
                    }
                }
                if (columnName == nameof(Beta))
                {
                    if (Beta < 0 || Beta > 100)
                    {
                        result = SetError(nameof(Beta), "Please enter a value for Beta between 0 and 100");
                    }
                    else
                    {
                        ClearError(nameof(Beta));
                    }
                }
                if (columnName == nameof(ProofTestEffectiveness))
                {
                    if (ProofTestEffectiveness.HasValue)
                    {
                        if (ProofTestEffectiveness < 0M || ProofTestEffectiveness > 1M)
                        {
                            result = SetError(nameof(ProofTestEffectiveness), "Please enter a value for Proof Test Effectiveness between 0 and 1");
                        }
                        else
                        {
                            ClearError(nameof(ProofTestEffectiveness));
                        }
                    }
                    else
                    {
                        result = SetError(nameof(ProofTestEffectiveness), "Please enter a value for Proof Test Effectiveness between 0 and 1");
                    }
                }
                if (columnName == nameof(PTI))
                {
                    if (PTI < 4 || PTI > LifeTime)
                    {
                        result = SetError(nameof(PTI), "Please enter a value for PTI between 4hrs and the Lifetime");
                    }
                    else
                    {
                        ClearError(nameof(PTI));
                    }
                }
                if (columnName == nameof(LifeTime))
                {
                    if (LifeTime < PTI || LifeTime > 20 * 8760)
                    {
                        result = SetError(nameof(LifeTime), "Please enter a value for Lifetime between PTI and 20 years");
                    }
                    else
                    {
                        ClearError(nameof(LifeTime));
                    }
                }
                return result;
            }
        }

        public double CalculateNextEvent(double currentClock, Random uniformRandomValue)
        {
            double result = 0D;
            double doubleLambda = (double)-this.Lambda;
            double doubleMDT = -1D / (double)MDT;
            if (CurrentState == SimulationState.Working)
            {
                // Time of next breakdown
                result = Math.Log(1 - uniformRandomValue.NextDouble()) / doubleLambda;
            }
            else
            {
                // Time of next restoration to Working
                if (this.ProofTestEffectiveness == 1M)
                {
                    var interval = (double)PTI;
                    result = interval + Math.Floor(currentClock / interval) * interval - currentClock;
                }
                else
                {
                    // Imperfect proof-testing
                    // Generate two possible events and choose the smallest
                    if (uniformRandomValue.NextDouble() > (double)ProofTestEffectiveness)
                    {
                        // This is a lifetime event
                        var interval = (double)LifeTime;
                        result = interval + Math.Floor(currentClock / interval) * interval - currentClock;
                    }
                    else
                    {
                        // This is proof test event
                        var interval = (double)PTI;
                        result = interval + Math.Floor(currentClock / interval) * interval - currentClock;
                    }
                }
            }

            NextEvent = currentClock + result;
            return NextEvent;
        }

        public virtual SimulationState CalculateState(double currentClock)
        {
            SimulationState result = SimulationState.Broken;
            var timeElapsed = currentClock - LastEvent;

            if (_nextEvent <= currentClock)
            {
                if (CurrentState == SimulationState.Working)
                {
                    // A Breakdown/failure has occured
                    Uptime += timeElapsed;
                    FailureCount += 1;
                    CurrentState = SimulationState.Broken;
                    result = CurrentState;

                }
                else
                {
                    // A Repair has resore the system to Working
                    Downtime += timeElapsed;
                    RepairCount += 1;
                    CurrentState = SimulationState.Working;
                    result = CurrentState;
                }
            }

            LastEvent = currentClock;

            Notify(nameof(this.SimulatedFailureRate));
            Notify(nameof(this.SimulatedPFD));
            Notify(nameof(this.SimulatedMeanDowntime));

            Parent.CalculateState(currentClock);

            return result;
        }
    }
    public enum SimulationState
    {
        Working, Broken
    }
}
