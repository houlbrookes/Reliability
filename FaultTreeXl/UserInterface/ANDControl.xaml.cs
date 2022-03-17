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
    /// Interaction logic for ANDControl.xaml
    /// </summary>
    public partial class ANDControl : UserControl
    {
        public ANDControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (DataContext is GraphicItem gi)
            //    gi.DisplayingControl = this;
        }

        private void EditClicked(object sender, RoutedEventArgs e)
        {
            var commandToExecute = new EditCommand();
            var node = DataContext;
            var theWindow = Window.GetWindow(this);
            commandToExecute.Execute(new object[] { node, theWindow });
        }

        private Brush _previousFill = null;
        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            _previousFill = ANDSymbol.Fill;

            if (e.Data.GetDataPresent(typeof(Node))
                || e.Data.GetDataPresent(typeof(OR))
                || e.Data.GetDataPresent(typeof(AND)))
            {
                ANDSymbol.Fill = Brushes.Yellow;
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            ANDSymbol.Fill = _previousFill;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            GraphicItem theItem = null;
            if (e.Data.GetDataPresent(typeof(Node)))
            {
                theItem = (Node)e.Data.GetData(typeof(Node));
            }
            else if (e.Data.GetDataPresent(typeof(OR)))
            {
                theItem = (OR)e.Data.GetData(typeof(OR));
            }
            else if (e.Data.GetDataPresent(typeof(AND)))
            {
                theItem = (AND)e.Data.GetData(typeof(AND));
            }
            else if (e.Data.GetDataPresent(typeof(StandardFailure)))
            {
                var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
                if (ftm != null)
                {
                    var theStdFail = (StandardFailure)e.Data.GetData(typeof(StandardFailure));
                    var theNewNode = new Node()
                    {
                        Name = $"Node {ftm.NextNodeName("Node") + 1}",
                        Description = theStdFail.Name,
                        Lambda = theStdFail.Rate,
                        TotalFailRate = theStdFail.TotalRate,
                        IsA = theStdFail.IsA,
                    };
                    (DataContext as AND).Nodes.Add(theNewNode);
                    ANDSymbol.Fill = _previousFill;
                    ftm.ReDrawRootNode();
                    // Open up an edit window for this node
                    OREdit editingWindow = new OREdit();
                    editingWindow.DataContext = theNewNode;
                    editingWindow.Owner = Application.Current.MainWindow;
                    editingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    editingWindow.ShowDialog();
                    theNewNode.UpdateParent();

                }
                return;
            }
            if (theItem != null && (theItem as AND) != (DataContext as AND))
            {
                theItem.Parent.Nodes.Remove(theItem);
                (DataContext as AND).Nodes.Add(theItem);
            }
            ANDSymbol.Fill = _previousFill;
            (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is UIElement element)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragDrop.DoDragDrop(element, DataContext, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }
    }
}
