using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FaultTreeXl
{
    public class ArchitecturalConstraintsContext : NotifyPropertyChangedItem
    {

        private IEnumerable<Arch> Process(IEnumerable<GraphicItem> li, double off)
        {
            return (
                from tuple in li.Select((node, index) => (node, index))
                let offset = off + tuple.index * 60.0
                let n1 = new Arch { X = offset, Y = 40, LinkedNode = tuple.node }
                let l2 = Process(tuple.node.Nodes, offset)
                select l2).SelectMany(n => n);
        }

        public (IEnumerable<Arch> items, double x_return, double y_return) Proc1(Arch currentNode, double x_offset, double y_offset)
        {
            var result = new List<Arch>();

            double new_x = x_offset;
            double new_y = y_offset;

            double x_inc = 0;
            double y_inc = 0;

            if (currentNode.LinkedNode is AND)
            {
                y_inc = 0;
                x_inc = 0;
                foreach (var node1 in currentNode.LinkedNode.Nodes)
                {
                    var node2 = new Arch() { X = new_x, Y = new_y, LinkedNode = node1 };
                    result.Add(node2);
                    //y_inc += 50;
                    var (list, new_x2, new_y2) = Proc1(node2, new_x, new_y+50);
                    new_y = new_y2;
                    new_x = new_x2;
                    result.AddRange(list);
                }
                return (result, new_x, y_offset);
            }
            else
            {
                foreach (var node1 in currentNode.LinkedNode.Nodes)
                {
                    var node2 = new Arch() { X = new_x, Y = new_y, LinkedNode = node1 };
                    result.Add(node2);
                    var (list, new_x2, new_y2) = Proc1(node2, new_x+50, new_y+50);
                    //new_y = new_y2;
                    new_x = new_x2;
                    result.AddRange(list);
                }
                return (result, new_x, y_offset);
            }

        }

        public void Loaded()
        {
            if (Application.Current.FindResource("GlobalFaultTreeModel") is FaultTreeModel ftm)
            {
                var result = new List<Arch>();
                var currentNode = ftm.RootNode;
                var rootArch = Arch.Create(currentNode);
                var (list, _dontcare1, _dontcare2, _dontcare3) = rootArch.DrawNode(0, 0);
                result.AddRange(list);
                Nodes = new ObservableCollection<Arch>(result);
            }
        }

        #region properties

        private double _width = 400;
        public double Width
        {
            get => _width;
            set => Changed(ref _width, value, nameof(Width));
        }

        private double _height = 400;
        public double Height
        {
            get => _height;
            set => Changed(ref _height, value, nameof(Height));
        }


        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set => Changed(ref _scale, value, nameof(Scale));
        }


        private ObservableCollection<Arch> _nodes = new ObservableCollection<Arch>();
        public ObservableCollection<Arch> Nodes
        {
            get => _nodes;
            set => Changed(ref _nodes, value, nameof(Nodes));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            
        }

        #endregion
    }

}
