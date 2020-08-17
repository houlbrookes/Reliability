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
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public NodeControl()
        {
            InitializeComponent();
        }
        private void EditClicked(object sender, RoutedEventArgs e)
        {
            var commandToExecute = new OREditCommand();
            var node = DataContext;
            var theWindow = Window.GetWindow(this);
            commandToExecute.Execute(new object[] { node, theWindow });
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            if (element != null && e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(element, DataContext, DragDropEffects.Copy | DragDropEffects.Move );
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void theCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        Brush _previousFill = null;
        private void theCanvas_DragEnter(object sender, DragEventArgs e)
        {
            base.OnDragEnter(e);
            _previousFill = NodeSymbol.Fill;
            if (e.Data.GetDataPresent(typeof(Node))
             || e.Data.GetDataPresent(typeof(OR))
             || e.Data.GetDataPresent(typeof(AND)))
            {
                NodeSymbol.Fill = Brushes.Yellow;
            }
        }

        private void theCanvas_DragLeave(object sender, DragEventArgs e)
        {
            base.OnDragLeave(e);
            NodeSymbol.Fill = _previousFill;
        }

        private void theCanvas_Drop(object sender, DragEventArgs e)
        {
            GraphicItem theItem = null;
            if (e.Data.GetDataPresent(typeof(Node)))
            {
                theItem = (Node)e.Data.GetData(typeof(Node));
            }
            if (e.KeyStates == DragDropKeyStates.ControlKey)
            {
                e.Effects = DragDropEffects.Copy;
                if (theItem != null && (theItem as Node) != (DataContext as Node))
                {
                    var thisItem = DataContext as Node;
                    thisItem.Lambda = theItem.Lambda;
                    thisItem.PTI = theItem.PTI;
                    thisItem.Description = theItem.Description;
                    thisItem.LifeTime = theItem.LifeTime;
                }
            }
            else
            {
            if (e.Data.GetDataPresent(typeof(StandardFailure)))
                {
                    var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
                    if (ftm != null)
                    {
                        var theStdFail = (StandardFailure)e.Data.GetData(typeof(StandardFailure));
                        var thisItem = DataContext as Node;
                        thisItem.Lambda = theStdFail.Rate;
                        thisItem.MakeModel =  theStdFail.Type+ "/" + theStdFail.Name;
                        NodeSymbol.Fill = _previousFill;
                        ftm.ReDrawRootNode();
                    }
                    return;
                }
            }
            NodeSymbol.Fill = _previousFill;
            (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
        }
    }
}
