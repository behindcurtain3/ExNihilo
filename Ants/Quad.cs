using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class Quad
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public List<Quad> Children { get; private set; }

        public Quad(int x, int y, int h, int w, int depth, int maxDepth = 2)
        {
            X = x;
            Y = y;
            Height = h;
            Width = w;

            Children = new List<Quad>();
            if (depth < maxDepth && Height % 2 == 0 && Width % 2 == 0)
            {
                int nh = Height / 2;
                int nw = Width / 2;
                Children.Add(new Quad(x, y, nh, nw / 2, depth + 1, maxDepth));
                Children.Add(new Quad(x + nw, y, nh, nw, depth + 1, maxDepth));
                Children.Add(new Quad(x, y + nh, nh, nw, depth + 1, maxDepth));
                Children.Add(new Quad(x + nw, y + nh, nh, nw, depth + 1, maxDepth));
            }
        }
        /*
        public List<LocAndDist> check(AntLoc ant, Location loc)
        {
            List<LocAndDist> results = new List<LocAndDist>();

            if (loc.row >= X && loc.row <= X + Height && loc.col >= Y && loc.col <= Y + Width)
            {
                if (Children.Count == 0)
                {

                }
                else
                {
                    foreach (Quad child in Children)
                        results.AddRange(child.check(ant, loc));
                }
            }
            else
            {

            }
        }
        */
        public void update(List<AntLoc> ants)
        {
            foreach(AntLoc ant in ants)
            {
                if(ant.row >= X && ant.row <= X + Height && ant.col >= Y && ant.col <= Y + Width)
                {
                    foreach (Quad child in Children)
                        child.update(ants);
                    return;
                }
            }
        }
    }
}
