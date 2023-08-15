using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class CCFWindowController : NotifyPropertyChangedItem
    {
        private FaultTreeModel _Model = null;
        public FaultTreeModel Model
        {
            get => _Model;
            set
            {
                Changed(ref _Model, value);
                Notify("Nodes");
            }
        }

        public ObservableCollection<string> Choices { get; set; } = new ObservableCollection<string>
        {
            "Largest",
            "Smallest",
            "Geo. Mean",
        };

        private string choice = "Largest";
        public string Choice
        {
            get => choice;
            set => Changed(ref choice, value, nameof(Choice));
        }

        private double _CCFRate = 0.10;
        public double CCFRate { get => _CCFRate; set => Changed(ref _CCFRate, value); }

        private ObservableCollection<GraphicItem> _Nodes = new ObservableCollection<GraphicItem>();
        public ObservableCollection<GraphicItem> Nodes
        {
            get => _Nodes;
            set
            {
                Changed(ref _Nodes, value);
                Func<object, bool> theFilter = listItem => 
                {
                    return NodesOnly ? (listItem is Node) : true; 
                };
                CollectionViewSource.GetDefaultView(_Nodes).Filter = listItem => NodesOnly ? (listItem is Node) : true;
            }
        }

        private bool _UpdateSource = true;
        public bool UpdateSource { get => _UpdateSource; set => Changed(ref _UpdateSource, value); }

        private bool _NodesOnly = true;
        public bool NodesOnly
        {
            get => _NodesOnly;
            set
            {
                Changed(ref _NodesOnly, value);
                if (_Nodes != null)
                    CollectionViewSource.GetDefaultView(_Nodes).Refresh();
            }
        }

        private bool _everything;

        public bool Everything
        {
            get => _everything;
            set
            {
                Changed(ref _everything, value, nameof(Everything));
                if (_Nodes != null)
                {
                    if (Everything)
                    {
                        if (App.Current.FindResource("GlobalFaultTreeModel") is FaultTreeModel model)
                        {
                            Nodes = new ObservableCollection<GraphicItem>(model.FaultTree);
                        }
                    }
                    CollectionViewSource.GetDefaultView(_Nodes).Refresh();
                }
            }
        }


        public Node Node2Update { get; set; }

        private GraphicItem _Node2UpdateFrom = null;
        public GraphicItem Node2UpdateFrom { get => _Node2UpdateFrom; set => Changed(ref _Node2UpdateFrom, value); }

        /// <summary>
        /// Update the Selected Node with a CCF value from the Target Node
        /// </summary>
        public ICommand UpdateCommand { get; set; } = new GenericCommand<object[]>
        {
            CanExecuteProxy = _x => true,
            ExecuteProxy = Update,
        };
        /// <summary>
        /// Implement the Update Node Function
        /// Use the CCFRate to calculate the new CCF
        /// If the tickbox is set, also update the Node 2 Update From 
        /// by removing this amount of failure rate
        /// </summary>
        /// <param name="param"></param>
        private static void Update(object[] parameters)
        {
            try
            {
                if ((parameters[0] is CCFWindowController aCCFWindowContext) && (parameters[1] is ListView listView))
                {
                    // Multiple nodes should have been selected
                    // decide which node to use as the basis for the CCF value
                    // if the nodes selected are ORs, update all subnodes with the beta value
                    // Update the CCF Node
                    decimal choice = 0.0M;
                    var nodes2Update = listView.SelectedItems.OfType<GraphicItem>();
                    var nodeqty = nodes2Update.Count();

                    if (nodeqty < 2)
                    {
                        MessageBox.Show("Select more than one node");
                        return;
                    }

                    var lambdas = nodes2Update.Select(n => n.Lambda);

                    // Clear Beta from any selected nodes
                    foreach (var n in aCCFWindowContext.Nodes)
                    {
                        n.UpdateBeta(0);
                    }

                    // Choice of how to calculate the Beta failure rate based
                    // on the drop-down selection
                    switch (aCCFWindowContext.Choice)
                    {
                        case "Largest":
                            choice = lambdas.Max();
                            break;
                        case "Smallest":
                            choice = lambdas.Min();
                            break;
                        case "Geo. Mean":
                            var prodOfLambdas = lambdas.Aggregate((a, l) => a * l);
                            var floatProdOfLambdas = (float)prodOfLambdas;
                            var power = 1.0 / nodeqty;
                            choice = (decimal)Math.Pow(floatProdOfLambdas, power);
                            break;
                        default:
                            MessageBox.Show("Invalid choice detected");
                            return;
                    }
                    aCCFWindowContext.Node2Update.Lambda = choice * (decimal)aCCFWindowContext.CCFRate;
                    // Take beta from each of the selected nodes (if UpdateSource is selected)
                    if (aCCFWindowContext.UpdateSource)
                    {
                        foreach (var item in nodes2Update)
                            item.UpdateBeta(aCCFWindowContext.CCFRate * 100);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
            if (parameters[2] is Window theWindow)
                theWindow.Close();
        }

        public CCFWindowController()
        {
        }
    }
}
