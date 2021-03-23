using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeXl
{
    public static class DrawTreeHelpers
    {
        private static int nodeSize = 1;
        private static double siblingDistance = 0.0D;
        private static double treeDistance = 0.0D;

        public static void CalculateNodePositions(GraphicItem rootNode)
        {
            // initialize node x, y, and mod values
            InitializeNodes(rootNode, 0);

            // assign initial X and Mod values for nodes
            CalculateInitialX(rootNode);

            // ensure no node is being drawn off screen
            CheckAllChildrenOnScreen(rootNode);

            // assign final X values to nodes
            CalculateFinalPositions(rootNode, 0);
        }

        // recusrively initialize x, y, and mod values of nodes
        private static void InitializeNodes(GraphicItem node, int depth)
        {
            node.X = -1;
            node.Y = depth;
            node.Mod = 0;

            foreach (var child in node.Nodes)
                InitializeNodes(child, depth + 1);
        }

        private static void CalculateFinalPositions(GraphicItem node, double modSum)
        {
            node.X += modSum;
            modSum += node.Mod;

            foreach (var child in node.Nodes)
                CalculateFinalPositions(child, modSum);

            //if (node.Nodes.Count == 0)
            //{
            //    node.Width = node.X;
            //    node.Height = node.Y;
            //}
            //else
            //{
            //    node.Width = node.Nodes.OrderByDescending(p => p.Width).First().Width;
            //    node.Height = node.Nodes.OrderByDescending(p => p.Height).First().Height;
            //}
        }

        private static void CalculateInitialX(GraphicItem node)
        {
            foreach (var child in node.Nodes)
                CalculateInitialX(child);

            // if no children
            if (node.IsLeaf())
            {
                // if there is a previous sibling in this set, set X to prevous sibling + designated distance
                if (!node.IsLeftMost())
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                else
                    // if this is the first node in a set, set X to 0
                    node.X = 0;
            }
            // if there is only one child
            else if (node.Nodes.Count == 1)
            {
                // if this is the first node in a set, set it's X value equal to it's child's X value
                if (node.IsLeftMost())
                {
                    node.X = node.Nodes[0].X;
                }
                else
                {
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                    node.Mod = node.X - node.Nodes[0].X;
                }
            }
            else
            {
                var leftChild = node.GetLeftMostChild();
                var rightChild = node.GetRightMostChild();
                var mid = (leftChild.X + rightChild.X) / 2;

                if (node.IsLeftMost())
                {
                    node.X = mid;
                }
                else
                {
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                    node.Mod = node.X - mid;
                }
            }

            if (node.Nodes.Count > 0 && !node.IsLeftMost())
            {
                // Since subtrees can overlap, check for conflicts and shift tree right if needed
                CheckForConflicts(node);
            }

        }

        private static bool IsLeaf(this GraphicItem node)
        {
            return node.Nodes.Count == 0;
        }

        private static GraphicItem GetLeftMostChild(this GraphicItem node)
        {
            return node.Nodes.First();
        }
        private static GraphicItem GetRightMostChild(this GraphicItem node)
        {
            return node.Nodes.Last();
        }

        private static GraphicItem GetLeftMostSibling(this GraphicItem node)
        {
            return node.Parent.Nodes.First();
        }

        private static bool IsLeftMost(this GraphicItem node)
        {
            return node.GetLeftMostSibling() == node;
        }

        private static GraphicItem GetNextSibling(this GraphicItem node)
        {
            var nextIndex = node.Parent.Nodes.IndexOf(node) + 1;
            if (nextIndex < node.Parent.Nodes.Count)
                return node.Parent.Nodes.ElementAt(nextIndex);
            else
                return node;
        }

        private static GraphicItem GetPreviousSibling(this GraphicItem node)
        {
            var currentIndex = node.Parent.Nodes.IndexOf(node);
            if (currentIndex > 0)
                return node.Parent.Nodes.ElementAt(currentIndex - 1);
            else
                return node;
        }

        private static void CheckForConflicts(GraphicItem node)
        {
            var minDistance = treeDistance + nodeSize;
            var shiftValue = 0D;

            var nodeContour = new Dictionary<int, double>();
            GetLeftContour(node, 0, ref nodeContour);

            var sibling = node.GetLeftMostSibling();
            while (sibling != null && sibling != node)
            {
                var siblingContour = new Dictionary<int, double>();
                GetRightContour(sibling, 0, ref siblingContour);

                for (int level = (int)node.Y + 1; level <= Math.Min(siblingContour.Keys.Max(), nodeContour.Keys.Max()); level++)
                {
                    var distance = nodeContour[level] - siblingContour[level];
                    if (distance + shiftValue < minDistance)
                    {
                        shiftValue = minDistance - distance;
                    }
                }

                if (shiftValue > 0)
                {
                    node.X += shiftValue;
                    node.Mod += shiftValue;

                    CenterNodesBetween(node, sibling);

                    shiftValue = 0;
                }

                sibling = sibling.GetNextSibling();
            }
        }

        private static void CenterNodesBetween(GraphicItem leftNode, GraphicItem rightNode)
        {
            var leftIndex = leftNode.Parent.Nodes.IndexOf(rightNode);
            var rightIndex = leftNode.Parent.Nodes.IndexOf(leftNode);

            var numNodesBetween = (rightIndex - leftIndex) - 1;

            if (numNodesBetween > 0)
            {
                var distanceBetweenNodes = (leftNode.X - rightNode.X) / (numNodesBetween + 1);

                int count = 1;
                for (int i = leftIndex + 1; i < rightIndex; i++)
                {
                    var middleNode = leftNode.Parent.Nodes[i];

                    var desiredX = rightNode.X + (distanceBetweenNodes * count);
                    var offset = desiredX - middleNode.X;
                    middleNode.X += offset;
                    middleNode.Mod += offset;

                    count++;
                }

                CheckForConflicts(leftNode);
            }
        }

        private static void CheckAllChildrenOnScreen(GraphicItem node)
        {
            var nodeContour = new Dictionary<int, double>();
            GetLeftContour(node, 0, ref nodeContour);

            double shiftAmount = 0;
            foreach (var y in nodeContour.Keys)
            {
                if (nodeContour[y] + shiftAmount < 0)
                    shiftAmount = (nodeContour[y] * -1);
            }

            if (shiftAmount > 0)
            {
                node.X += shiftAmount;
                node.Mod += shiftAmount;
            }
        }

        private static void GetLeftContour(GraphicItem node, double modSum, ref Dictionary<int, double> values)
        {
            if (!values.ContainsKey((int)node.Y))
                values.Add((int)node.Y, node.X + modSum);
            else
                values[(int)node.Y] = Math.Min(values[(int)node.Y], node.X + modSum);

            modSum += node.Mod;
            foreach (var child in node.Nodes)
            {
                GetLeftContour(child, modSum, ref values);
            }
        }

        private static void GetRightContour(GraphicItem node, double modSum, ref Dictionary<int, double> values)
        {
            if (!values.ContainsKey((int)node.Y))
                values.Add((int)node.Y, node.X + modSum);
            else
                values[(int)node.Y] = Math.Max(values[(int)node.Y], node.X + modSum);

            modSum += node.Mod;
            foreach (var child in node.Nodes)
            {
                GetRightContour(child, modSum, ref values);
            }
        }
    }
}
