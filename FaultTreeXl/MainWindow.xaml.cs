using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Win32 = Microsoft.Win32;

namespace FaultTreeXl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Point? lastCenterPositionOnTarget;
        //Point? lastMousePositionOnTarget;
        //Point? lastDragPoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TheScrollViewer.Width = this.Width - 25;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is FaultTreeModel ftModel)
            {
                if (ftModel.Dirty)
                {
                    MessageBoxResult reply = MessageBox.Show("Do you want to save before closing (y/n)", "Unsaved Edits", MessageBoxButton.YesNo);
                    if (reply == MessageBoxResult.Yes)
                    {
                        var saveCommand = new SaveCommand();
                        saveCommand.Execute(ftModel);
                    }
                }
            }
        }

        private void TheScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scv)
            {
                if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift)
                {
                    FaultTreeModel ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
                    if (e.Delta > 0)
                    {
                        ftm.Scale += 0.1;
                    }
                    else
                    {
                        ftm.Scale -= 0.1;
                    }
                    ftm.ReDrawRootNode();
                }
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
                }
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FaultTreeModel ftm)
            {
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    // application has been started with a fta filename
                    var fileName = args[1];
                    var extension = System.IO.Path.GetExtension(fileName);
                    if (File.Exists(fileName) && extension == ".fta")
                    {
                        var loadCommand = new LoadCommand();
                        loadCommand.LoadFromFile(fileName, ftm);
                    }
                }
            }
        }

        private void OR_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            GraphicItem data = null;
            var ftm = Application.Current.FindResource("GlobalFaultTreeModel") as FaultTreeModel;
            var tag = "";
            if (element is Control control) tag = control.Tag as string;
            else if (element is Viewbox viewbox) tag = viewbox.Tag as string;
            if (tag != null)
            {
                switch (tag)
                {
                    case "AND":
                        data = new AND()
                        {
                            Name = $"AND {ftm.NextNodeName("AND") + 1}",
                            Description = "Please Update",
                        };
                        break;
                    case "OR":
                        data = new OR()
                        {
                            Name = $"OR {ftm.NextNodeName("OR") + 1}",
                            Description = "Please Update",
                        };
                        break;
                    case "Node":
                        data = new Node()
                        {
                            Name = $"Node {ftm.NextNodeName("Node") + 1}",
                            Description = "Please Update",
                            Lambda = 1E-6M,
                            PTI = 8760M,
                        };
                        break;
                    case "CCF":
                        data = new Node()
                        {
                            Name = $"CCF {ftm.NextNodeName("CCF") + 1}",
                            Description = "Please Update",
                            Lambda = 1E-6M,
                            PTI = 8760M,
                        };
                        break;
                }
            }

            if (sender != null && data != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var obj = new DataObject(data);
                //var adLayer = AdornerLayer.GetAdornerLayer(element);
                //var myAdornment = new DraggableAdorner(element);
                //adLayer.Add(myAdornment);
                DragDrop.DoDragDrop(element, obj, DragDropEffects.Copy | DragDropEffects.Move);
                //adLayer.Remove(myAdornment);
            }
        }

    }
}
