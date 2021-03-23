using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private double _CCFRate = 0.10;
        public double CCFRate { get => _CCFRate; set => Changed(ref _CCFRate, value); }

        private ObservableCollection<GraphicItem> _Nodes = new ObservableCollection<GraphicItem>();
        public ObservableCollection<GraphicItem> Nodes
        {
            get => _Nodes;
            set 
            { 
                Changed(ref _Nodes, value);
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

        public Node Node2Update { get; set; }

        private GraphicItem _Node2UpdateFrom = null;
        public GraphicItem Node2UpdateFrom { get => _Node2UpdateFrom; set => Changed(ref _Node2UpdateFrom, value); }

        /// <summary>
        /// Update the Selected Node with a CCF value from the Target Node
        /// </summary>
        public ICommand UpdateCommand { get; set; } = new GenericCommand<object[]>
        {
            CanExecuteProxy = x => true,
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
                if ((parameters[0] is CCFWindowController controller) && (parameters[1] is ListView listView))
                {
                    foreach(GraphicItem item in listView.SelectedItems)
                    {
                        controller.Node2Update.Lambda = item.BetaFreeLambda * (decimal)controller.CCFRate;
                        if (controller.UpdateSource)
                        {
                            item.Beta = controller.CCFRate * 100;
                        }
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
