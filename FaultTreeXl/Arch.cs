using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeXl
{
    /// <summary>
    /// A class to descibe a unit of architectrual constraint
    /// </summary>
    public class Arch : NotifyPropertyChangedItem
    {
        public virtual (IEnumerable<Arch> nodeList, double x_return, double y_return, double height) DrawNode(double x_offset, double y_offset)
        {
            X = x_offset;
            Y = y_offset;
            Height = 50;
            Width = 50;
            return (new List<Arch> { this }, x_offset + 50, y_offset, 50);
        }

        public static Arch Create(GraphicItem node)
        {
            if (node is Node) return new Arch { LinkedNode = node };
            else if (node is OR) return new ArchOR { LinkedNode = node };
            else if (node is AND) return new ArchAND { LinkedNode = node };
            else throw new NotImplementedException();
        }

        #region properties
        private double _x;
        public double X
        {
            get => _x;
            set => Changed(ref _x, value, nameof(X));
        }

        private double _y;
        public double Y
        {
            get => _y;
            set => Changed(ref _y, value, nameof(Y));
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => Changed(ref _height, value, nameof(Height));
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => Changed(ref _width, value, nameof(Width));
        }


        private GraphicItem _linkedNode;
        public GraphicItem LinkedNode
        {
            get => _linkedNode;
            set => Changed(ref _linkedNode, value, nameof(LinkedNode));
        }

        #endregion
    }

    public class ArchOR : Arch
    {
        public override (IEnumerable<Arch> nodeList, double x_return, double y_return, double height) DrawNode(double x_offset, double y_offset)
        {
            double x = x_offset;
            double y = y_offset;
            double h = 0D;
            double new_h = 0D;

            X = x;
            Y = y;
            var list = new List<Arch>() { this };
            IEnumerable<Arch> newList = null;

            foreach(var n in LinkedNode.Nodes)
            {
                var arch = Arch.Create(n);
                (newList, x, y, new_h) = arch.DrawNode(x, y_offset + 50);
                h = Math.Max(h, new_h);
                list.AddRange(newList);
            }
            if (LinkedNode.Nodes.Count == 0)
            {
                x = x_offset + 50;
            }

            Height = h + 50;
            Width = x - x_offset;

            X = x_offset + Width / 2 - 25;

            return (list, x, y, Height);
        }

    }

    public class ArchAND : Arch
    {
        public override (IEnumerable<Arch> nodeList, double x_return, double y_return, double height) DrawNode(double x_offset, double y_offset)
        {
            double x = x_offset;
            double y = y_offset;
            double h = 0D;
            double new_h = 0D;
            
            X = x;
            Y = y;
            
            var list = new List<Arch>() { this };
            IEnumerable<Arch> newList = null;

            //y += 50;

            foreach (var n in LinkedNode.Nodes)
            {
                var arch = Arch.Create(n);
                (newList, x, y, new_h) = arch.DrawNode(x_offset, y+50);
                h = Math.Max(h, new_h);
                list.AddRange(newList);
            }
            if (LinkedNode.Nodes.Count == 0)
            {
                x = x_offset + 50;
            }

            Height = h + 50;
            Width = x - x_offset;

            X = x_offset + Width / 2 - 25;

            return (list, x, y, Height);
        }
    }

}
