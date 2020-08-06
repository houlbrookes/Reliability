using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaultTreeXl
{
    /// <summary>
    /// Interaction logic for CCFWindow.xaml
    /// </summary>
    public partial class CCFWindow : Window
    {
        public FaultTreeModel Model { get; set; }
        public Node Node { get; set; }

        public CCFWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as CCFWindowController).Model = Model;
            (DataContext as CCFWindowController).Nodes = Model.FaultTree;
            (DataContext as CCFWindowController).Node2Update = Node;
        }
    }

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

        private double _CCFRate = 0.1;
        public double CCFRate { get => _CCFRate; set => Changed(ref _CCFRate, value); }

        private ObservableCollection<GraphicItem> _Nodes = null;
        public ObservableCollection<GraphicItem> Nodes
        {
            get => _Nodes;
            set => Changed(ref _Nodes, value);
        }

        private bool _UpdateSource = true;
        public bool UpdateSource { get => _UpdateSource; set => Changed(ref _UpdateSource, value); }

        public Node Node2Update { get; set; }

        private GraphicItem _Node2UpdateFrom = null;
        public GraphicItem Node2UpdateFrom { get => _Node2UpdateFrom; set => Changed(ref _Node2UpdateFrom, value); }

        /// <summary>
        /// Update the Selected Node with a CCF value from the Target Node
        /// </summary>
        public ICommand UpdateCommand { get; set; } = new GenericCommand<CCFWindowController>
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
        private static void Update(CCFWindowController param)
        {
            try
            {
                if (param.Node2UpdateFrom != null)
                {
                    param.Node2Update.Lambda = param.Node2UpdateFrom.Lambda * (decimal)param.CCFRate;
                    if (param.UpdateSource)
                    {
                        param.Node2UpdateFrom.Lambda = param.Node2UpdateFrom.Lambda * (decimal)(1 - param.CCFRate);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        public CCFWindowController()
        {
        }
    }
}
