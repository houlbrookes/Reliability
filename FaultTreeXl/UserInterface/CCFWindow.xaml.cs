using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
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

        private IEnumerable<GraphicItem> GetChildren(GraphicItem node)
        {
            var result = new List<GraphicItem> { node };

            var subNodes = node.Nodes.SelectMany(n => GetChildren(n));

            foreach (var subNode in subNodes) result.Add(subNode);

            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as CCFWindowController).Model = Model;

            // Filter the list of nodes to the nodes within any adjacent ANDs
            var neighboringANDs = Node.Parent.Nodes.Where(n => n is AND).SelectMany(n => GetChildren(n)).OrderBy(n => n.Name);

            (DataContext as CCFWindowController).Nodes = new ObservableCollection<GraphicItem>(neighboringANDs); //Model.FaultTree;
            (DataContext as CCFWindowController).Node2Update = Node;

        }

    }
}
