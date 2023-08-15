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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaultTreeXl.UserInterface
{
    /// <summary>
    /// Interaction logic for ORToolBarItem.xaml
    /// </summary>
    public partial class ORToolBarItem : UserControl
    {
        public ORToolBarItem()
        {
            InitializeComponent();
        }

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            var data = new OR()
            {
                Name = $"OR {ftm.NextNodeName("OR") + 1}",
                Description = "Please Update",
            };
            if (sender != null && data != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var obj = new DataObject(data);
                DragDrop.DoDragDrop((Control)sender, obj, DragDropEffects.Copy | DragDropEffects.Move);
            }

        }
    }
}
