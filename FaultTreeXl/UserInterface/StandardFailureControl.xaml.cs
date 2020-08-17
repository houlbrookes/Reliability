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

namespace FaultTreeXl
{
    /// <summary>
    /// Interaction logic for StandardFailureControl.xaml
    /// </summary>
    public partial class StandardFailureControl : UserControl
    {
        public StandardFailureControl()
        {
            InitializeComponent();
        }

        private void STD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            if (element != null && e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(element, DataContext, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void STD_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void STD_DragOver(object sender, DragEventArgs e)
        {

        }

        private void STD_DragLeave(object sender, DragEventArgs e)
        {

        }
    }
}
