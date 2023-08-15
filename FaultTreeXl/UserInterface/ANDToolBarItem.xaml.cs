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
    /// Interaction logic for ANDToolBarItem.xaml
    /// </summary>
    public partial class ANDToolBarItem : UserControl
    {
        public ANDToolBarItem()
        {
            InitializeComponent();
        }

        private void MouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            var data = new AND()
            {
                Name = $"AND {ftm.NextNodeName("AND") + 1}",
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
