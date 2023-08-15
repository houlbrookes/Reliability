using System.Windows;
using System.Windows.Controls;

namespace FaultTreeXl
{
    internal class SFFGraphicItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {
                FrameworkElement element = container as FrameworkElement;
                if (item is OR)
                    return (DataTemplate)element.FindResource("SFFORTemplate");
                if (item is AND)
                    return (DataTemplate)element.FindResource("SFFANDTemplate");
                if (item is Node)
                    return (DataTemplate)element.FindResource("SFFNodeTemplate");
                else
                    return (DataTemplate)element.FindResource("SFFGrahicItemTemplate");
            }
            catch
            {
                return null;
            }
        }

    }
}
