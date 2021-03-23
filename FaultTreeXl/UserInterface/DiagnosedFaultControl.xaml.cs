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
    /// Interaction logic for DiagnosedFaultControl.xaml
    /// </summary>
    public partial class DiagnosedFaultControl : UserControl
    {
        public DiagnosedFaultControl()
        {
            InitializeComponent();
        }

        private void EditClicked(object sender, RoutedEventArgs e)
        {
            var commandToExecute = new EditCommand();
            var node = DataContext;
            var theWindow = Window.GetWindow(this);
            commandToExecute.Execute(new object[] { node, theWindow });
        }

    }
}
