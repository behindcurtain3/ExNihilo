using System;
using System.Collections.Generic;
using System.Text;

namespace Ants
{
    public class AStar
    {
        public static int Height { get; set; }
        public static int Width { get; set; }

        public static Path getPath(ANode start, List<ANode> targets)
        {
            Debug.Write("Finding path from " + start.Coords);
            Debug.Write("To:");
            foreach (ANode n in targets)
            {
                Debug.Write("--- " + n.Coords);
            }
            Path empty = new Path();

            List<ANode> openList = new List<ANode>();
            List<ANode> closedList = new List<ANode>();

            Boolean foundTarget = false;
            ANode currentNode = start;

            start.Parent = null;
            openList.Add(start);

            while (!foundTarget)
            {
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

                foreach (KeyValuePair<ANode, Boolean> neighbor in currentNode.Neighbors)
                {
                    if (neighbor.Value && neighbor.Key.Passable)
                    {
                        if (!closedList.Contains(neighbor.Key) && !openList.Contains(neighbor.Key))
                        {
                            neighbor.Key.Parent = currentNode;
                            neighbor.Key.G = neighbor.Key.Parent.G + getDistance(currentNode, neighbor.Key);
                            neighbor.Key.H = getClosestTarget(targets, neighbor.Key.Coords);
                            neighbor.Key.F = neighbor.Key.G + neighbor.Key.H;
                            openList.Add(neighbor.Key);
                        }
                    }
                }
            }

            return empty;
        }

        public static float getDistance(ANode a, ANode b)
        {
            // calculate the closest distance between two locations
            int d_row = Math.Abs(a.Coords.row - b.Coords.row);
            d_row = Math.Min(d_row, Height - d_row);

            int d_col = Math.Abs(a.Coords.col - a.Coords.col);
            d_col = Math.Min(d_col, Width - d_col);

            return d_row + d_col;

            //return Math.Abs(a.Coords.row - b.Coords.row) + Math.Abs(a.Coords.col - b.Coords.col);
        }

        public static float getClosestTarget(List<ANode> targets, Location position)
        {
            float smallestDistance = 9999f;

            foreach (ANode c in targets)
            {
                float d = Math.Abs(c.Coords.row - position.row) + Math.Abs(c.Coords.col - position.col);
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                }
            }

            return smallestDistance;
        }
    }
}
