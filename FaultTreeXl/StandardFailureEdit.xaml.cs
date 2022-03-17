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
    /// Interaction logic for StandardFailureEdit.xaml
    /// </summary>
    public partial class StandardFailureEdit : Window
    {
        StandardFailure Failure2Edit { get; set; } = null;
        public StandardFailureEdit()
        {
            InitializeComponent();
        }
        public StandardFailureEdit(StandardFailure theStandardFailure)
        {
            Failure2Edit = theStandardFailure;
            DataContext = theStandardFailure;
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
