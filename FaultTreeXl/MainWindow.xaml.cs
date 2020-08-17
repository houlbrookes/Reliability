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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TheScrollViewer.Width = this.Width - 25;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult reply = MessageBox.Show("Do you want to close (y/n)", "Exit", MessageBoxButton.YesNo);
            if (reply == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void TheScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scv)
            {
                if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift)
                {
                    FaultTreeModel ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
                    if (e.Delta > 0)
                    {
                        ftm.Scale += 0.1;
                    }
                    else
                    {
                        ftm.Scale -= 0.1;
                    }
                    ftm.ReDrawRootNode();
                }
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
                }
                e.Handled = true;
            }
        }

    }
}
