using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ArchConstr.xaml
    /// </summary>
    public partial class ArchConstr : Window
    {
        public ArchConstr()
        {
            InitializeComponent();
            DataContext = new ArchitecturalConstraintsContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ArchitecturalConstraintsContext dc)
            {
                dc.Loaded();
            }
        }
    }

}
