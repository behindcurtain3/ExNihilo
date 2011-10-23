using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Ants
{
    public class AStar
    {
        public static int Height { get; set; }
        public static int Width { get; set; }

        public static Path getPath(ANode start, List<ANode> targets)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Path empty = new Path();

            List<ANode> openList = new List<ANode>();
            List<ANode> closedList = new List<ANode>();

            Boolean foundTarget = false;
            ANode currentNode = start;

            start.Parent = null;
            openList.Add(start);

            while (!foundTarget)
            {
                if (sw.ElapsedMilliseconds > 50)
                {
                    // We found the target node
                    Path path = new Path();
                    ANode pathNode = currentNode;
                    while (pathNode.Parent != null)
                    {
                        path.Push(pathNode);
                        pathNode = (ANode)pathNode.Parent;
                    }
                    return path;
                }

                // If the openlist has no ANodes return an empty path
                if (openList.Count == 0)
                    return empty;

                float lowestFScore = 9999f;

                foreach (ANode c in openList)
                {
                    if (c.F < lowestFScore)
                    {
                        lowestFScore = c.F;
                        currentNode = c;
                    }
                }

                if (targets.Contains(currentNode))
                {
                    // We found the target node
                    Path path = new Path();

                    foundTarget = true;
                    ANode pathNode = currentNode;
                    while (pathNode.Parent != null)
                    {
                        path.Push(pathNode);
                        pathNode = (ANode)pathNode.Parent;
                    }
                    return path;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (ANode neighbor in currentNode.Neighbors)
                {
                    if (neighbor.Passable)
                    {
                        if (!closedList.Contains(neighbor) && !openList.Contains(neighbor))
                        {
                            neighbor.Parent = currentNode;
                            neighbor.G = neighbor.Parent.G + getDistance(currentNode, neighbor);
                            neighbor.H = getClosestTarget(targets, neighbor);
                            neighbor.F = neighbor.G + neighbor.H;
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return empty;
        }

        public static int getDistance(ANode a, ANode b)
        {
            // calculate the closest distance between two locations
            int d_row = Math.Abs(a.Location.row - b.Location.row);
            d_row = Math.Min(d_row, Height - d_row);

            int d_col = Math.Abs(a.Location.col - a.Location.col);
            d_col = Math.Min(d_col, Width - d_col);

            return d_row + d_col;

            //return Math.Abs(a.Location.row - b.Location.row) + Math.Abs(a.Location.col - b.Location.col);
        }

        public static int getClosestTarget(List<ANode> targets, ANode position)
        {
            int smallestDistance = 9999;

            foreach (ANode c in targets)
            {
                int d = getDistance(c, position);
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                }
            }

            return smallestDistance;
        }
    }
}
