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

        Brush _previousFillNodeSymbol = null;

        Adorner myAdornment;

        private void theCanvas_DragEnter(object sender, DragEventArgs e)
        {
            base.OnDragEnter(e);

            bool thisIsADifferentNode = true;

            Node theDraggedItem = null;

            if (e.Data.GetDataPresent(typeof(Node)))
            {
                theDraggedItem = (Node)e.Data.GetData(typeof(Node));
                thisIsADifferentNode = theDraggedItem != (DataContext as Node);
            }

            if (thisIsADifferentNode)
            {
                if (e.Data.GetDataPresent(typeof(OR))
                 || e.Data.GetDataPresent(typeof(AND)))
                {
                    myAdornment = new CannotDropAdorner(theCanvas);
                    AdornerLayer.GetAdornerLayer(theCanvas).Add(myAdornment);
                }
                else
                {
                    myAdornment = new CanDropAdorner(theCanvas);
                    AdornerLayer.GetAdornerLayer(theCanvas).Add(myAdornment);
                }
            }
            else
            {
                // Do nothing
            }
        }

        private void theCanvas_DragLeave(object sender, DragEventArgs e)
        {
            base.OnDragLeave(e);

            bool thisIsADifferentNode = true;

            if (e.Data.GetDataPresent(typeof(Node)))
            {
                Node theDraggedItem = (Node)e.Data.GetData(typeof(Node));
                thisIsADifferentNode = theDraggedItem != (DataContext as Node);
            }

            if (thisIsADifferentNode)
            {
                if (myAdornment != null)
                {
                    AdornerLayer.GetAdornerLayer(theCanvas).Remove(myAdornment);
                }

            }
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
                    thisItem.BetaFreeLambda = theItem.BetaFreeLambda;
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
                        thisItem.BetaFreeLambda = theStdFail.Rate;
                        thisItem.MakeModel =  theStdFail.Type+ "/" + theStdFail.Name;
                        ftm.ReDrawRootNode();
                    }
                    return;
                }
            }

            (Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel).ReDrawRootNode();
        }
    }
}
