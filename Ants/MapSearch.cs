using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class MapSearch
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int Sight { get; set; } // How far do our units see?
        public double Range { get; set; }

        public List<Location> Points { get; set; }
        public double[,,] Distances { get; set; } // table of distances from one loc to another

        public MapSearch(int h, int w, int s)
        {
            Height = h;
            Width = w;
            Sight = s;
            Range = Sight;

            int start = (int)(s * 0.5);

            Points = new List<Location>();
            String line = "";
            for (int y = start; y < Height; y += Sight)
            {
                line = "";
                for (int x = start; x < Width; x += Sight)
                {
                    Location l = new Location(y, x);
                    line += " " + l;
                    Points.Add(l);
                }
                Debug.Write(line);
            }



            Distances = new double[Height, Width, Height * Width];

            for (int a = 0; a < Height; a++)
            {
                for (int b = 0; b < Width; b++)
                {
                    for (int c = 0; c < Height * Width; c++)
                    {
                        Distances[a, b, c] = 0;
                    }
                }
            }
        }

        public Location nearest(GameState state, AntLoc ant)
        {
            if (Points.Count == 0)
                return ant;

            Dictionary<Location, double> antDist = new Dictionary<Location, double>();
            foreach (Location p in Points)
            {
                int index = ant.row * Width + ant.col;
                if (Distances[p.row, p.col, index] == 0)
                {
                    Distances[p.row, p.col, index] = state.distance(p, ant);
                }
                if(!antDist.ContainsKey(p))
                    antDist.Add(p, Distances[p.row, p.col, index]);
            }

            antDist = antDist.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (Location loc in antDist.Keys)
                return loc;

            return ant;
        }

        public void addAnchor(Location loc)
        {

        }

        public void update(GameState state, List<AntLoc> ants)
        {
            for (int i = Points.Count - 1; i >= 0; i--)
            {
                foreach (AntLoc ant in ants)
                {
                    if (state.distance(ant, Points[i]) < Range)
                    {
                        Debug.Write("Removed point: " + Points[i]);
                        Points.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
