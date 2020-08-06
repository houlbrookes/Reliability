using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FaultTreeXl
{
    internal class GraphicItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {
                var window = Application.Current.MainWindow;
                if (item is OR)
                    return (DataTemplate)window.FindResource("ORTemplate");
                if (item is AND)
                    return (DataTemplate)window.FindResource("ANDTemplate");
                if (item is Node)
                    return (DataTemplate)window.FindResource("NodeTemplate");
                if (item is DiagnosedFaultNode)
                    return (DataTemplate)window.FindResource("DiagnosedFaultTemplate");
                else
                    return (DataTemplate)window.FindResource("GrahicItemTemplate");
            }
            catch
            {
                return null;
            }
        }
    }
}
