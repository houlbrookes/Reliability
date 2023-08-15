using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FaultTreeXl
{
    public class LaunchSFFWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel ftm)
            {
                var rootNode = ftm.RootNode;
                //ftm.RootNode.SFFMatrix(0, 0);
                var maxDepth = ftm.RootNode.Depth - 1;
                List<SFFDisplay> SFFNodes = new List<SFFDisplay>();
                var row = 0;
                for (var i = maxDepth; i >= 0; i--)
                {
                    SFFNodes.AddRange(ftm.RootNode.NodesForLevel(i, row, 0, i));
                    row = SFFNodes.Max(sff => sff.Row) + 2;
                }

                var maxLevel = SFFNodes.Max(sff => sff.Level);

                var yOffset = SFFNodes.Where(sff => sff.Level == maxLevel).GroupBy(sff => sff.Col).Max(grp => grp.Count()) * 70 / 2.0;

                for (var i = maxLevel; i >= 0; i--)
                {
                    var lineList = SFFNodes.Where(sff => sff.Level == i);
                    var maxRow = lineList.Max(sff => sff.Row);
                    var maxCol = lineList.Max(sff => sff.Col);
                    for (var c = 0; c <= maxCol; c++)
                    {
                        var cList = lineList.Where(sff => sff.Col == c);
                        var maxC = cList.Count();
                        foreach (var sffnode in cList)
                        {
                            var pList = lineList.Where(sff => sff.Col == c - 1 && sff.Node.Parent == sffnode.Node.Parent && sff.Level==i);
                            if (pList.Count() > 0)
                            {
                                var firstSibling = pList.First();
                                sffnode.X = sffnode.Col * 110;
                                //sffnode.Y = ((sffnode.Row ) - (maxC - 1.0) / 2.0) * 70 + yOffset;
                                sffnode.Y = (sffnode.Row - (sffnode.Node.Parent.Nodes.Count() - 1.0) / 2.0) * 70 + yOffset;
                                sffnode.Y = firstSibling.Y;
                            }
                            else
                            {
                                sffnode.X = sffnode.Col * 110;
                                sffnode.Y = (sffnode.Row - (maxC - 1.0) / 2.0) * 70 + yOffset;
                            }

                        }


                    }
                }

                var theWindow = new SFFWindow();
                theWindow.DataContext = SFFNodes;
                theWindow.ShowDialog();
            }
        }
    }

    public class SFFDisplay : NotifyPropertyChangedItem
    {
        private int _level;
        public int Level
        {
            get => _level;
            set => Changed(ref _level, value, nameof(Level));
        }

        private int _row;
        public int Row
        {
            get => _row;
            set => Changed(ref _row, value, nameof(Row));
        }

        private int _col;
        public int Col
        {
            get => _col;
            set => Changed(ref _col, value, nameof(Col));
        }

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

        private GraphicItem _node;
        public GraphicItem Node
        {
            get => _node;
            set => Changed(ref _node, value, nameof(Node));
        }

    }
}
