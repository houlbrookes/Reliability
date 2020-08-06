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
    /// Interaction logic for ORControl.xaml
    /// </summary>
    public partial class ORControl : UserControl
    {
        public ORControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Open up an editing window with this window as its parent
        /// A command cannot be used because there is no simple way to
        /// get back to the this window. (Although it may be possible by
        /// using tricks, this is the best way to do it.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OREditClicked(object sender, RoutedEventArgs e)
        {
            var commandToExecute = new OREditCommand();
            var node = DataContext;
            var theWindow = Window.GetWindow(this);
            commandToExecute.Execute(new object[] { node, theWindow });
        }
    }
}
