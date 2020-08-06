using System;
using System.Collections.Generic;
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
    /// Interaction logic for StandardParts.xaml
    /// </summary>
    public partial class StandardParts : Window
    {
        public GraphicItem SelectedNode { get; set; } = null;
        public FaultTreeModel ParentModel { get; set; } = null;


        public StandardParts()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView thisListView)
            {
                var selected = thisListView.SelectedItem;
                if (selected is GraphicItem node)
                {
                    foreach (var item in thisListView.ItemsSource)
                    {
                        if (item is GraphicItem listNode)
                        {
                            if (listNode == selected)
                            {
                                listNode.IsSelected = true;
                                SelectedNode = listNode;
                            }
                            else
                            {
                                listNode.IsSelected = false;
                            }
                        }
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
