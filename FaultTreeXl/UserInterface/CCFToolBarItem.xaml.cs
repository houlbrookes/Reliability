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
    /// Interaction logic for CCFToolBarItem.xaml
    /// </summary>
    public partial class CCFToolBarItem : UserControl
    {
        public CCFToolBarItem()
        {
            InitializeComponent();
        }

        private void MouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            var data = new Node()
            {
                Name = $"CCF {ftm.NextNodeName("CCF") + 1}",
                Description = "Please Update",
                Lambda = 1E-6M,
                PTI = 8760M,
                Beta = 0,
            };
            if (sender != null && data != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var obj = new DataObject(data);
                DragDrop.DoDragDrop((Control)sender, obj, DragDropEffects.Copy | DragDropEffects.Move);
            }

        }
    }
}
