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
            var commandToExecute = new EditCommand();
            var node = DataContext;
            var theWindow = Window.GetWindow(this);
            commandToExecute.Execute(new object[] { node, theWindow });
        }

        private Adorner myAdornment = null;

        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (e.Data.GetDataPresent(typeof(Node)) 
             || e.Data.GetDataPresent(typeof(OR)) 
             || e.Data.GetDataPresent(typeof(AND)))
            {
                myAdornment = new CanDropAdorner(theCanvas);
                AdornerLayer.GetAdornerLayer(theCanvas).Add(myAdornment);
            }
            else
            {
                myAdornment = new CannotDropAdorner(theCanvas);
                AdornerLayer.GetAdornerLayer(theCanvas).Add(myAdornment);
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
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

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            base.OnDragLeave(e);
            if (myAdornment != null)
            {
                AdornerLayer.GetAdornerLayer(theCanvas).Remove(myAdornment);
            }
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
                var theStdFail = (StandardFailure)e.Data.GetData(typeof(StandardFailure));
                var theNewNode = new Node()
                {
                    Name = Global.NodeUtils.NextNodeName,
                    Description = theStdFail.Name,
                    Lambda = theStdFail.Rate,
                    TotalFailRate = theStdFail.TotalRate,
                    IsA = theStdFail.IsA,
                };
                (DataContext as OR).Nodes.Add(theNewNode);
                Global.NodeUtils.ReDrawRootNode();

                // Open up an edit window for this node
                OREdit editingWindow = new OREdit();
                editingWindow.DataContext = theNewNode;
                editingWindow.Owner = Application.Current.MainWindow;
                editingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editingWindow.ShowDialog();
                theNewNode.UpdateParent();
                return;
            }
            if (e.KeyStates == DragDropKeyStates.ControlKey)
            {
                e.Effects = DragDropEffects.Copy;
                if (theItem != null && (theItem as OR) != (DataContext as OR))
                {
                    //theItem.Parent.Nodes.Remove(theItem);
                    //var newItem = theItem.DeepCopy
                    //(DataContext as OR).Nodes.Add(theItem);
                }
            }
            else
            {
                e.Effects = DragDropEffects.Move;
                if (theItem != null && (theItem as OR) != (DataContext as OR))
                {
                    if (theItem.Parent != null)
                        theItem.Parent.Nodes.Remove(theItem);
                    (DataContext as OR).Nodes.Add(theItem);
                }
            }
            (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is UIElement element)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    myAdornment = new BeginDraggedAdorner(theCanvas);
                    AdornerLayer.GetAdornerLayer(theCanvas).Add(myAdornment);
                    DragDrop.DoDragDrop(element, DataContext, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

    }
}
