using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class  GraphicItem : NotifyPropertyChangedItem
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

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        private string name = "Node";
        [XmlAttribute]
        public string Name { get => name; set => Changed(ref name, value); }

        private string description = "No description";
        [XmlAttribute]
        public string Description { get => description; set => Changed(ref description, value); }

        private string makeModel = "Generic";
        public string MakeModel { get => makeModel; set => Changed(ref makeModel, value); }


        [XmlIgnore]
        virtual public List<CutSet> CutSets { get; } = new List<CutSet>();

        /// <summary>
        /// Useful for debugging
        /// Returns a string representation of the cut sets 
        /// (Work is done in CutSet extension class)
        /// </summary>
        [XmlIgnore]
        public string CutSetsAsString { get => CutSets.AsString();  }

        private decimal lambda = 0;
        /// <summary>
        /// Failure rate (often referred to as Lambda
        /// </summary>
        [XmlAttribute]
        virtual public decimal Lambda
        {
            get => lambda;
            set
            {
                Changed(ref lambda, value);
                Notify("PFD");
                Parent?.Recalculating?.Invoke(this, EventArgs.Empty);
                UpdateParent();
                CalculationsComplete?.Invoke(this, EventArgs.Empty);
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

        /// <summary>
        /// Mean Downtime
        /// Uses the simple forumla of PFD/Failure Rate
        /// </summary>
        [XmlAttribute]
        virtual public decimal MDT { get => Lambda>0 ? PFD / Lambda : 0; }

        private decimal? _proofTestEffectiveness = 1;
        [XmlElement(IsNullable = true)]
        public decimal? ProofTestEffectiveness { get => _proofTestEffectiveness; set => Changed(ref _proofTestEffectiveness, value); }

        private decimal? _lifeTime = 8760 * 10;
        [XmlElement(IsNullable = true)]
        public decimal? LifeTime { get => _lifeTime; set => Changed(ref _lifeTime, value); }

        /// <summary>
        /// Probability of Failure on Demand
        /// Most of the word is done in the the CutSets Extension class
        /// the formula used is simple: OR together the PFD from each Minimal Cut Set
        /// i.e. Multiply them together and subtract from 1.
        /// This is only possible because I'm using a Decimal floating point, 
        /// the accuracy of Decimals is 128-bit floating point giving 28 to 29 decimal point accuracy
        /// </summary>
        [XmlIgnore]
        public decimal PFD
        {
            // calculated from the cutsets of the OR/AND/Node
            get => 1M - CutSets.Select(cs => (1-cs.PFD)).Product();
        }

        private bool _forceIntegration = false;
        [XmlIgnore]
        public bool ForceIntegration
        {
            get => _forceIntegration;
            set => Changed(ref _forceIntegration, value);
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

        virtual public double AssignXY(double x1, double y1)
        {
            double newX = x1;
            //double prevX = x1;
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
            {// Parent
                //this.X = x1 + (newX - x1) / 2 - 110 / 2;
                X = (nodeArray[0].X + nodeArray[nodeArray.Length - 1].X) / 2;
                this.Y = y1;

                this.X2 = Nodes.First().X + 50 - X;
                this.X3 = Nodes.Last().X + 50 - X;
                return Math.Max(newX, GRAPHIC_WIDTH);
            }
            else
            {//Node
                this.X = newX;
                this.Y = y1;
                return newX + GRAPHIC_WIDTH;
            }

        }

        /// <summary>
        /// Top Left Hand Corner
        /// </summary>
        private double x = 0D;
        [XmlIgnore] public double X { get => x; set => Changed(ref x, value); }

        private double y = 0D;
        [XmlIgnore] public double Y { get => y; set => Changed(ref y, value); }

        /// <summary>
        /// Extent of the left line
        /// </summary>
        private double x2 = 0D;
        [XmlIgnore] public double X2 { get => x2; set => Changed(ref x2, value); }

        private double x3 = 0D;
        [XmlIgnore] public double X3 { get => x3; set => Changed(ref x3, value); }

        private bool isSelected = false;
        [XmlIgnore] public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value)
                {
                    var topNode = this;
                    while (topNode.Parent != null)
                        topNode = topNode.Parent;
                    topNode.ClearSelection();
                }
                Changed(ref isSelected, value);
            }
        }

        private bool _showLifeInfo = true;
        [XmlIgnore]
        public bool ShowLifeInfo
        {
            get => _showLifeInfo;
            set => Changed(ref _showLifeInfo, value);
        }


        private void ClearSelection()
        {
            IsSelected = false;
            foreach (var subNode in Nodes)
                subNode.ClearSelection();
        }

    }
}
