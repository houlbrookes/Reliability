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
    /// Interaction logic for StandardFailureControl.xaml
    /// </summary>
    public partial class StandardFailureControl : UserControl
    {
        public StandardFailureControl()
        {
            InitializeComponent();
        }

        private void STD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Mouse Down initiates a drag session
            // DataContext is of type StandardFailure from the collection 
            // declared in FaultTreeModel

            if (e.ClickCount >= 2)
            {
                if (DataContext is StandardFailure theStandardFailure)
                {
                    EditItem(theStandardFailure);
                }
            }
            else
            {
                var element = (UIElement)sender;
                if (element != null && e.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(element, DataContext, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void STD_DragEnter(object sender, DragEventArgs e)
        {
            // Not used
        }

        private void STD_DragOver(object sender, DragEventArgs e)
        {
            // Not used
        }

        private void STD_DragLeave(object sender, DragEventArgs e)
        {
            // Not used
        }



        /// <summary>
        /// Edit the current Standard Failure Item and Save it back 
        /// to file if the Ok button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StandardFailure theStandardFailure)
            {
                EditItem(theStandardFailure);
            }
        }

        private void EditItem(StandardFailure theStandardFailure)
        {
            // Make and editable copy
            var editableCopy = new StandardFailure().CopyFrom(theStandardFailure);
            var window = new StandardFailureEdit(editableCopy) { Owner = App.Current.MainWindow };
            
            if (window.ShowDialog() == true)
            {
                // Ok button clicked
                if (App.Current.MainWindow.DataContext is FaultTreeModel context)
                {
                    // Copy back over the edited data
                    theStandardFailure.CopyFrom(editableCopy);
                    // Save data to file
                    context.SaveStandardFailures();
                }
            }
        }

        private void mnuAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StandardFailure theStandardFailure)
            {
                // Make and editable copy
                var editableCopy = new StandardFailure()
                {
                    Name = "New Name",
                    Type = "New Type",
                    Rate = 1E-07M,
                    TotalRate = 1E-06M,
                    IsA = false,
                };
                var window = new StandardFailureEdit(editableCopy) { Owner = App.Current.MainWindow };
                var result = window.ShowDialog();
                if (result == true)
                {
                    // Ok button clicked
                    if (App.Current.MainWindow.DataContext is FaultTreeModel context)
                    {
                        var indexOfThis = context.FailureRates.IndexOf(theStandardFailure);
                        if (indexOfThis > -1)
                        {
                            // Add it a the current location
                            context.FailureRates.Insert(indexOfThis, editableCopy);
                            // Save data to file
                            context.SaveStandardFailures();
                        }
                    }
                }
            }
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StandardFailure theStandardFailure)
            {
                var result = MessageBox.Show("Do you really want to delete this entry?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Ok button clicked
                    if (App.Current.MainWindow.DataContext is FaultTreeModel context)
                    {
                        // Copy back over the edited data
                        context.FailureRates.Remove(theStandardFailure);
                        // Save data to file
                        context.SaveStandardFailures();
                    }
                }
            }
        }

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is StandardFailure theStandardFailure)
            {
                if (e.Data.GetDataPresent(typeof(StandardFailure)))
                {
                    var theItem = (StandardFailure)e.Data.GetData(typeof(StandardFailure));

                    if (App.Current.MainWindow.DataContext is FaultTreeModel context)
                    {
                        var indexOfThis = context.FailureRates.IndexOf(theStandardFailure);
                        if (indexOfThis > -1)
                        {
                            // Remove the dragged item from where it is now
                            context.FailureRates.Remove(theItem);
                            // Add it a the current location
                            context.FailureRates.Insert(indexOfThis, theItem);
                            // Save data to file
                            context.SaveStandardFailures();
                        }
                    }
                }
            }
        }
    }
}
