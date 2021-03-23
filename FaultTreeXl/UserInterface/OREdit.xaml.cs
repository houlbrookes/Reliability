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
    /// Interaction logic for EditOR.xaml
    /// </summary>
    public partial class OREdit : Window
    {
        public OREdit()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is GraphicItem theGrapichItem)
            {
                if (! string.IsNullOrEmpty(theGrapichItem.Error))
                {
                    var result = MessageBox.Show("Please correct or hit cancel to ignore", "There are errors with the data", MessageBoxButton.OKCancel);
                    e.Cancel = result == MessageBoxResult.OK;
                }
            }
        }
    }
}
